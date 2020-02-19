using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class SoundGlobal : IXAssetPool
        {
            /// <summary>
            /// Sound XAsset Structure
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

            public override string Name => "Sound Global";
            public override int Index => (int)XAssetPool.soundbank;
            public override long EndAddress { get; set; }
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameXAsset>();

                foreach (int index in new int[] { (int)XAssetPool.soundglobals, (int)XAssetPool.soundbank })
                {
                    var poolInfo = instance.Reader.ReadStruct<XAssetPoolInfo>(instance.Game.BaseAddress + instance.Game.XAssetPoolsAddresses[instance.Game.ProcessIndex] + (index * 24));

                    StartAddress = poolInfo.PoolPtr;
                    XAssetSize = poolInfo.XAssetSize;
                    XAssetCount = poolInfo.PoolSize;

                    for (int i = 0; i < XAssetCount; i++)
                    {
                        var header = instance.Reader.ReadStruct<SoundXAsset>(StartAddress + (i * XAssetSize));

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
                            Information = $"Aliases: {header.AliasCount}"
                        });
                    }
                }

                return results;
            }

            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                var header = instance.Reader.ReadStruct<SoundXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer).Split(':')[0])
                {
                    return JekyllStatus.MemoryChanged;
                }

                Directory.CreateDirectory(instance.SoundZoneFolder);

                using (var writer = new StreamWriter(Path.Combine(instance.SoundZoneFolder, xasset.Name + "_alias.csv")))
                {
                    writer.WriteLine("Name,Secondary,FileSpec,");

                    for (int j = 0; j < header.AliasCount; j++)
                    {
                        var aliasData = instance.Reader.ReadStruct<SoundAlias>(header.AliasesPointer + (j * 32));

                        for (int k = 0; k < aliasData.EntriesCount; k++)
                        {
                            var aliasEntryData = instance.Reader.ReadStruct<SoundAliasEntry>(aliasData.EntriesPointer + (k * 0xE8));
                            writer.WriteLine(instance.Reader.ReadNullTerminatedString(aliasEntryData.NamePtr) + "," + instance.Reader.ReadNullTerminatedString(aliasEntryData.SecondaryPointer) + "," + instance.Reader.ReadNullTerminatedString(aliasEntryData.FileSpec));
                        }
                    }
                }

                Console.WriteLine($"Exported {Name} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}