using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class GfxMap : IXAssetPool
        {
            /// <summary>
            /// GfxMap XAsset Structures
            /// </summary>
            private struct GfxMapXAsset
            {
                public long NamePointer { get; set; }
                public long NullPtr { get; set; }
                public int SomethingCount { get; set; }
                public int Something2Count { get; set; }
                public long Unk3 { get; set; }
                public long MapNamePointer { get; set; }
            }

            public override string Name => "Gfx Map";
            public override int Index => (int)XAssetPool.gfx_map;
            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }

            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameXAsset>();

                // Incomplete
                /*
                var poolInfo = instance.Reader.ReadStruct<XAssetPoolInfo>(instance.Game.BaseAddress + instance.Game.XAssetPoolsAddresses[instance.Game.ProcessIndex] + (Index * 24));

                StartAddress = poolInfo.PoolPtr;
                XAssetSize = poolInfo.XAssetSize;
                XAssetCount = poolInfo.PoolSize;

                for (int i = 0; i < XAssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<GfxMapXAsset>(StartAddress + (i * XAssetSize));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }

                    var rawData = instance.Reader.ReadBytes(StartAddress + (i * XAssetSize), (int)XAssetSize);
                    string exportName = Path.Combine("iw8_maps", instance.Reader.ReadNullTerminatedString(header.NamePointer));
                    Directory.CreateDirectory(Path.GetDirectoryName(exportName));
                    File.WriteAllBytes(exportName, rawData);

                    rawData = instance.Reader.ReadBytes(header.Unk3, 100000);
                    exportName = Path.Combine("iw8_maps", instance.Reader.ReadNullTerminatedString(header.NamePointer));
                    Directory.CreateDirectory(Path.GetDirectoryName(exportName));
                    File.WriteAllBytes(exportName, rawData);

                    Console.WriteLine(instance.Reader.ReadNullTerminatedString(header.MapNamePointer));
                }
                */

                return results;
            }

            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                Console.WriteLine($"Exported {Name} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}