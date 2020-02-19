using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class MapEnts : IAssetPool
        {
            #region AssetStructures
            /// <summary>
            /// TTF Asset Structure
            /// </summary>
            private struct MapEntsAsset
            {
                public long NamePointer;
                public long RawDataPtr;
            }
            #endregion

            public override string Name => "Map Entity";
            public override int Index => (int)AssetPool.map_ents;

            public override long EndAddress { get { return StartAddress + (AssetCount * AssetSize); } set => throw new NotImplementedException(); }
            public override List<GameAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameAsset>();

                var poolInfo = instance.Reader.ReadStruct<AssetPoolInfo>(instance.Game.BaseAddress + instance.Game.AssetPoolsAddresses[instance.Game.ProcessIndex] + (Index * 24));

                StartAddress = poolInfo.PoolPtr;
                AssetSize = poolInfo.AssetSize;
                AssetCount = poolInfo.PoolSize;

                for (int i = 0; i < AssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<MapEntsAsset>(StartAddress + (i * AssetSize));

                    if (IsNullAsset(header.NamePointer))
                        continue;

                    results.Add(new GameAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        HeaderAddress = StartAddress + (i * AssetSize),
                        AssetPool = this,
                        Type = Name,
                        Information = "N/A"
                    });
                }

                return results;
            }

            public override JekyllStatus Export(GameAsset asset, JekyllInstance instance)
            {
                var header = instance.Reader.ReadStruct<MapEntsAsset>(asset.HeaderAddress);

                if (asset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                    return JekyllStatus.MemoryChanged;

                string path = Path.Combine(instance.ExportFolder, asset.Name);

                // Create path
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                File.WriteAllText(path, instance.Reader.ReadNullTerminatedString(header.RawDataPtr));

                Console.WriteLine($"Exported {Name} {asset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}