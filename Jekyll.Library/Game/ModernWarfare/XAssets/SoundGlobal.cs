using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class SoundGlobal : IXAssetPool
        {
            public override string Name => "Sound Bank";

            public override int Index => (int)XAssetPool.soundbank + 1;

            public override long EndAddress { get; set; }

            /// <summary>
            /// Structure of a Modern Warfare SoundBank XAsset.
            /// </summary>
            private struct SoundXAsset
            {
                public long NamePointer { get; set; }
                public long ZoneNamePointer { get; set; }
                public long LanguageIDPointer { get; set; }
                public long LanguagePointer { get; set; }
                public long AliasCount { get; set; }
                public long AliasesPointer { get; set; }
            }

            public struct SoundAlias
            {
                public long NamePtr { get; set; }
                public long UnkPtr { get; set; }
                public long EntriesPointer { get; set; }
                public long EntriesCount { get; set; }
            }

            public struct SoundAliasEntry
            {
                public long NamePtr { get; set; }
                public long UnkPtr { get; set; }
                public long SecondaryPointer { get; set; }
                public long FileSpec { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the SoundBank XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of SoundBank XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                foreach (int index in new int[] { (int)XAssetPool.soundglobals + 1, (int)XAssetPool.soundbank + 1 })
                {
                    XAssetPoolData poolInfo = instance.Reader.ReadStruct<XAssetPoolData>(instance.Game.BaseAddress + instance.Game.XAssetPoolsAddress + (index * 24));

                    StartAddress = poolInfo.PoolPointer;
                    XAssetSize = poolInfo.XAssetSize;
                    XAssetCount = poolInfo.PoolSize;

                    for (int i = 0; i < XAssetCount; i++)
                    {
                        SoundXAsset header = instance.Reader.ReadStruct<SoundXAsset>(StartAddress + (i * XAssetSize));

                        if (IsNullXAsset(header.NamePointer))
                        {
                            continue;
                        }

                        results.Add(new GameXAsset()
                        {
                            Name = instance.Reader.ReadNullTerminatedString(header.NamePointer).Split(':')[0],
                            HeaderAddress = StartAddress + (i * XAssetSize),
                            XAssetPool = this,
                            Type = Name,
                        });
                    }
                }

                return results;
            }

            /// <summary>
            /// Exports the specified SoundBank XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                SoundXAsset header = instance.Reader.ReadStruct<SoundXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer).Split(':')[0])
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportPath, "sound", xasset.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (StreamWriter writer = new StreamWriter(Path.Combine(path + ".csv")))
                {
                    for (int j = 0; j < header.AliasCount; j++)
                    {
                        SoundAlias aliasData = instance.Reader.ReadStruct<SoundAlias>(header.AliasesPointer + (j * 32));

                        for (int k = 0; k < aliasData.EntriesCount; k++)
                        {
                            SoundAliasEntry aliasEntryData = instance.Reader.ReadStruct<SoundAliasEntry>(aliasData.EntriesPointer + (k * 0xE8));

                            writer.WriteLine(instance.Reader.ReadNullTerminatedString(aliasEntryData.NamePtr) + "," + instance.Reader.ReadNullTerminatedString(aliasEntryData.SecondaryPointer) + "," + instance.Reader.ReadNullTerminatedString(aliasEntryData.FileSpec));
                        }
                    }
                }

                Console.WriteLine($"Exported {xasset.Type} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}