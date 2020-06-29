using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class WWII
    {
        public class TTF : IXAssetPool
        {
            public override string Name => "TrueType Font";

            public override int Index => (int)XAssetPool.ttf;

            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Structure of a WWII TTF XAsset.
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

                StartAddress = instance.Reader.ReadStruct<long>(instance.Game.XAssetPoolsAddress + (Marshal.SizeOf<XAssetPoolData>() * Index));
                XAssetSize = instance.Reader.ReadStruct<int>(instance.Game.XAssetPoolSizesAddress + (Marshal.SizeOf<XAssetPoolSizesData>() * Index));

                for (int i = 0; i < XAssetSize; i++)
                {
                    TTFXAsset header = instance.Reader.ReadStruct<TTFXAsset>(StartAddress + Marshal.SizeOf<XAssetPoolData>() + (i * Marshal.SizeOf<TTFXAsset>()));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }
                    else if (header.Size == 1)
                    {
                        continue;
                    }
                    else if (instance.Reader.ReadNullTerminatedString(header.NamePointer).EndsWith(".ttf") is false)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        Type = Name,
                        Size = XAssetSize,
                        XAssetPool = this,
                        HeaderAddress = StartAddress + Marshal.SizeOf<XAssetPoolData>() + (i * Marshal.SizeOf<TTFXAsset>()),
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