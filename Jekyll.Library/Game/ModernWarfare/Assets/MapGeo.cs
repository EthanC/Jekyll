using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class MapGeo : IAssetPool
        {
            #region AssetStructures
            /// <summary>
            /// MapGeo Asset Structures
            /// </summary>
            private struct GfxMap
            {
                /// <summary>
                /// A pointer to the name of this GfxMap Asset
                /// </summary>
                public long NamePointer { get; set; }
                public long nullPtr { get; set; }
                public int SomethingCount { get; set; }
                public int Something2Count { get; set; }
                public long unk3 { get; set; }
                public long MapNamePointer { get; set; }
            }
            #endregion

            public override string Name => "Map Geometry";
            public override int Index => (int)AssetPool.gfx_map;
            public override long EndAddress { get { return StartAddress + (AssetCount * AssetSize); } set => throw new NotImplementedException(); }
            public override List<GameAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameAsset>();

                // Incomplete
                /*
                var poolInfo = instance.Reader.ReadStruct<AssetPoolInfo>(instance.Game.BaseAddress + instance.Game.AssetPoolsAddresses[instance.Game.ProcessIndex] + (Index * 24));

                StartAddress = poolInfo.PoolPtr;
                AssetSize = poolInfo.AssetSize;
                AssetCount = poolInfo.PoolSize;

                for (int i = 0; i < AssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<GfxMap>(StartAddress + (i * AssetSize));

                    if (IsNullAsset(header.NamePointer))
                        continue;

                    var RawData = instance.Reader.ReadBytes(StartAddress + (i * AssetSize), (int)AssetSize);
                    string exportName = Path.Combine("iw8_maps", instance.Reader.ReadNullTerminatedString(header.NamePointer));
                    Directory.CreateDirectory(Path.GetDirectoryName(exportName));
                    File.WriteAllBytes(exportName, RawData);

                    RawData = instance.Reader.ReadBytes(header.unk3, 100000);
                    exportName = Path.Combine("iw8_maps", instance.Reader.ReadNullTerminatedString(header.NamePointer) + "stdsfgdfg");
                    Directory.CreateDirectory(Path.GetDirectoryName(exportName));
                    File.WriteAllBytes(exportName, RawData);

                    Console.WriteLine(instance.Reader.ReadNullTerminatedString(header.MapNamePointer));
                }
                */

                return results;
            }

            public override JekyllStatus Export(GameAsset asset, JekyllInstance instance)
            {
                Console.WriteLine($"Exported {Name} {asset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}