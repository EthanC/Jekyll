using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class TTF : IXAssetPool
        {
            /// <summary>
            /// TTF XAsset Structure
            /// </summary>
            private struct TTFXAsset
            {
                public long NamePointer { get; set; }
                public int AssetSize { get; set; }
                public long RawDataPtr { get; set; }
                public int Unk { get; set; }
            }

            public override string Name => "TrueType Font";
            public override int Index => (int)XAssetPool.ttf;
            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameXAsset>();

                var poolInfo = instance.Reader.ReadStruct<XAssetPoolInfo>(instance.Game.BaseAddress + instance.Game.XAssetPoolsAddresses[instance.Game.ProcessIndex] + (Index * 24));

                StartAddress = poolInfo.PoolPtr;
                XAssetSize = poolInfo.XAssetSize;
                XAssetCount = poolInfo.PoolSize;

                for (int i = 0; i < XAssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<TTFXAsset>(StartAddress + (i * XAssetSize));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        HeaderAddress = StartAddress + (i * XAssetSize),
                        XAssetPool = this,
                        Type = Name,
                        Information = $"Size: 0x{header.AssetSize}"
                    });
                }

                return results;
            }

            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                var header = instance.Reader.ReadStruct<TTFXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportFolder, xasset.Name);

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                byte[] buffer = instance.Reader.ReadBytes(header.RawDataPtr, (int)header.AssetSize);

                File.WriteAllBytes(path, buffer);

                Console.WriteLine($"Exported {Name} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}