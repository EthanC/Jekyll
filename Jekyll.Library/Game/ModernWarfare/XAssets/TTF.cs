using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class TTF : IXAssetPool
        {
            public override string Name => "TrueType Font";

            public override int Index => (int)XAssetPool.ttf;

            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Structure of a Modern Warfare TTF XAsset.
            /// </summary>
            private struct TTFXAsset
            {
                public long NamePointer { get; set; }
                public int Size { get; set; }
                public long DataPointer { get; set; }
                public int NullPadding { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the TTF XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of TTF XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                XAssetPoolData poolInfo = instance.Reader.ReadStruct<XAssetPoolData>(instance.Game.BaseAddress + instance.Game.XAssetPoolsAddress + (Index * Marshal.SizeOf<XAssetPoolData>()));

                StartAddress = poolInfo.PoolPointer;
                XAssetSize = poolInfo.XAssetSize;
                XAssetCount = poolInfo.PoolSize;

                for (int i = 0; i < XAssetCount; i++)
                {
                    TTFXAsset header = instance.Reader.ReadStruct<TTFXAsset>(StartAddress + (i * XAssetSize));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        Type = Name,
                        Size = XAssetSize,
                        XAssetPool = this,
                        HeaderAddress = StartAddress + (i * XAssetSize),
                    });
                }

                return results;
            }

            /// <summary>
            /// Exports the specified TTF XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                TTFXAsset header = instance.Reader.ReadStruct<TTFXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportPath, xasset.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                byte[] buffer = instance.Reader.ReadBytes(header.DataPointer, header.Size);

                File.WriteAllBytes(path, buffer);

                Console.WriteLine($"Exported {xasset.Type} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}