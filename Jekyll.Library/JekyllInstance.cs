using PhilLibX.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JekyllLibrary.Library
{
    public class JekyllInstance
    {
        /// <summary>
        /// Gets or sets the list of supported games.
        /// </summary>
        public List<IGame> Games { get; set; }

        /// <summary>
        /// Gets or sets the current game.
        /// </summary>
        public IGame Game { get; set; }

        /// <summary>
        /// Gets or sets the current process reader.
        /// </summary>
        public ProcessReader Reader { get; set; }

        /// <summary>
        /// Gets or sets the loaded XAssets.
        /// </summary>
        public List<GameXAsset> XAssets { get; set; }

        /// <summary>
        /// Gets the export path for the current game.
        /// </summary>
        public string ExportPath { get { return Path.Combine("export", Game.Name); } }

        public JekyllInstance()
        {
            Games = new List<IGame>();
            Type gameType = typeof(IGame);
            IEnumerable<Type> games = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => gameType.IsAssignableFrom(p));

            foreach (Type game in games)
            {
                if (!game.IsInterface)
                {
                    Games.Add((IGame)Activator.CreateInstance(game));
                }
            }
        }

        /// <summary>
        /// Validate that the current game is running and hasn't changed.
        /// </summary>
        public JekyllStatus ValidateGame()
        {
            if (Reader != null && Game != null)
            {
                Process[] processes = Process.GetProcessesByName(Game.ProcessNames[Game.ProcessIndex]);

                if (processes.Length == 0)
                {
                    return JekyllStatus.GameClosed;
                }
                else if (processes[0].Id != Reader.ActiveProcess.Id)
                {
                    return JekyllStatus.MemoryChanged;
                }

                return JekyllStatus.Success;
            }

            return JekyllStatus.GameClosed;
        }

        /// <summary>
        /// Gets the XAsset Pools for the current game.
        /// </summary>
        public static List<IXAssetPool> GetXAssetPools(IGame game)
        {
            Type poolType = typeof(IXAssetPool);
            List<IXAssetPool> results = new List<IXAssetPool>();

            IEnumerable<Type> pools = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => poolType.IsAssignableFrom(p));

            foreach (Type pool in pools)
            {
                if (!pool.IsInterface)
                {
                    if (pool.DeclaringType is Type gameType)
                    {
                        if (gameType == game.GetType())
                        {
                            results.Add((IXAssetPool)Activator.CreateInstance(pool));
                        }
                    }
                }
            }


            return results;
        }

        public JekyllStatus LoadGame()
        {
            Process[] processes = Process.GetProcesses();

            JekyllStatus status = JekyllStatus.FailedToFindGame;

            foreach (Process process in processes)
            {
                foreach (IGame game in Games)
                {
                    for (int i = 0; i < game.ProcessNames.Length; i++)
                    {
                        if (process.ProcessName == game.ProcessNames[i])
                        {
                            Game = (IGame)game.Clone();
                            Game.ProcessIndex = i;
                            Reader = new ProcessReader(process);

                            if (Game.InitializeGame(this))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Loaded Call of Duty: {Game.Name} (0x{Game.XAssetPoolsAddress})");
                                Console.ResetColor();

                                Game.XAssetPools = GetXAssetPools(Game);

                                XAssets = new List<GameXAsset>();

                                foreach (IXAssetPool xassetPool in Game.XAssetPools)
                                {
                                    XAssets.AddRange(xassetPool.Load(this));
                                }

                                status = JekyllStatus.Success;
                            }
                            else
                            {
                                status = JekyllStatus.UnsupportedBinary;
                            }

                            break;
                        }
                    }
                }
            }

            return status;
        }
    }
}