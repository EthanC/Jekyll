using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class Sound : IAssetPool
        {
            #region AssetStructures
            /// <summary>
            /// Sound Asset Structure
            /// </summary>
            private struct SoundAsset
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
            #endregion

            public override string Name => "Sound Alias";
            public override int Index => (int)AssetPool.soundbank;
            public override long EndAddress { get; set; }
            public override List<GameAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameAsset>();

                foreach (int index in new int[] { (int)AssetPool.soundglobals, (int)AssetPool.soundbank })
                {
                    var poolInfo = instance.Reader.ReadStruct<AssetPoolInfo>(instance.Game.BaseAddress + instance.Game.AssetPoolsAddresses[instance.Game.ProcessIndex] + (index * 24));

                    StartAddress = poolInfo.PoolPtr;
                    AssetSize = poolInfo.AssetSize;
                    AssetCount = poolInfo.PoolSize;

                    for (int i = 0; i < AssetCount; i++)
                    {
                        var header = instance.Reader.ReadStruct<SoundAsset>(StartAddress + (i * AssetSize));

                        if (IsNullAsset(header.NamePointer))
                            continue;

                        results.Add(new GameAsset()
                        {
                            Name = instance.Reader.ReadNullTerminatedString(header.NamePointer).Split(':')[0],
                            HeaderAddress = StartAddress + (i * AssetSize),
                            AssetPool = this,
                            Type = Name,
                            Information = string.Format("Aliases: {0}", header.AliasCount)
                        });
                    }
                }

                return results;
            }

            public override JekyllStatus Export(GameAsset asset, JekyllInstance instance)
            {
                var header = instance.Reader.ReadStruct<SoundAsset>(asset.HeaderAddress);

                if (asset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer).Split(':')[0])
                    return JekyllStatus.MemoryChanged;

                Directory.CreateDirectory(instance.SoundZoneFolder);

                using (var writer = new StreamWriter(Path.Combine(instance.SoundZoneFolder, asset.Name + "_alias.csv")))
                {
                    writer.WriteLine("Name,Secondary,FileSpec,");

                    for (int j = 0; j < header.AliasCount; j++)
                    {
                        var aliasData = instance.Reader.ReadStruct<SoundAlias>(header.AliasesPointer + (j * 32));
                        for (int k = 0; k < aliasData.EntriesCount; k++)
                        {
                            var aliasEntryData = instance.Reader.ReadStruct<SoundAliasEntry>(aliasData.EntriesPointer + (k * 0xE8));
                            writer.WriteLine("{0},{1},{2},",
                                instance.Reader.ReadNullTerminatedString(aliasEntryData.NamePtr),
                                instance.Reader.ReadNullTerminatedString(aliasEntryData.SecondaryPointer),
                                instance.Reader.ReadNullTerminatedString(aliasEntryData.FileSpec));
                        }
                    }
                }

                Console.WriteLine($"Exported {Name} {asset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}