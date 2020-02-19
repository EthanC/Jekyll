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
        /// Gets or Sets the List of Supported Games
        /// </summary>
        public List<IGame> Games { get; set; }

        /// <summary>
        /// Gets or Sets the current Game
        /// </summary>
        public IGame Game { get; set; }

        /// <summary>
        /// Gets or Sets the current Process Reader
        /// </summary>
        public ProcessReader Reader { get; set; }

        /// <summary>
        /// Gets or Sets the loaded XAssets
        /// </summary>
        public List<GameXAsset> XAssets { get; set; }

        /// <summary>
        /// Gets the Export Path
        /// </summary>
        public string ExportFolder { get { return Path.Combine("export", Game.Name); } }

        /// <summary>
        /// Gets the Sound Path
        /// </summary>
        public string SoundFolder { get { return Path.Combine(ExportFolder, "sound"); } }

        /// <summary>
        /// Gets the Sound Zone Path
        /// </summary>
        public string SoundZoneFolder { get { return Path.Combine(SoundFolder, "zone"); } }

        public JekyllInstance()
        {
            Games = new List<IGame>();
            var gameType = typeof(IGame);
            var games = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => gameType.IsAssignableFrom(p));

            foreach (var game in games)
            {
                if (!game.IsInterface)
                {
                    Games.Add((IGame)Activator.CreateInstance(game));
                }
            }
        }

        /// <summary>
        /// Checks to ensure game is running and hasn't changed
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
                if (processes[0].Id != Reader.ActiveProcess.Id)
                {
                    return JekyllStatus.MemoryChanged;
                }

                return JekyllStatus.Success;
            }

            return JekyllStatus.GameClosed;
        }

        /// <summary>
        /// Gets XAsset Pools for the given Game
        /// </summary>
        public static List<IXAssetPool> GetXAssetPools(IGame game)
        {
            var poolType = typeof(IXAssetPool);
            var results = new List<IXAssetPool>();

            var pools = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => poolType.IsAssignableFrom(p));

            foreach (var pool in pools)
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

            var status = JekyllStatus.FailedToFindGame;

            foreach (var process in processes)
            {
                foreach (var game in Games)
                {
                    for (int i = 0; i < game.ProcessNames.Length; i++)
                    {
                        if (process.ProcessName == game.ProcessNames[i])
                        {
                            Game = (IGame)game.Clone();
                            Game.ProcessIndex = i;
                            Reader = new ProcessReader(process);

                            if (Game.ValidateAddresses(this))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Loaded Call of Duty: {Game.Name} (0x{Game.BaseAddress})");
                                Console.ResetColor();

                                Game.XAssetPools = GetXAssetPools(Game);

                                XAssets = new List<GameXAsset>();

                                foreach (var xassetPool in Game.XAssetPools)
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