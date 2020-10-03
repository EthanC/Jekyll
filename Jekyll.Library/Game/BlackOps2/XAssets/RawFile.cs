using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class BlackOps2
    {
        public class RawFile : IXAssetPool
        {
            public override string Name => "Raw File";

            public override int Index => (int)XAssetPool.rawfile;

            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Structure of a Black Ops II RawFile XAsset.
            /// </summary>
            private struct RawFileXAsset
            {
                public int NamePointer { get; set; }
                public int Size { get; set; }
                public int DataPointer { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the RawFile XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of RawFile XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                StartAddress = instance.Reader.ReadStruct<int>(instance.Game.XAssetPoolsAddress + (Marshal.SizeOf<XAssetPoolData>() * Index));
                XAssetSize = instance.Reader.ReadStruct<int>(instance.Game.XAssetPoolSizesAddress + (Marshal.SizeOf<XAssetPoolSizesData>() * Index));

                for (int i = 0; i < XAssetSize; i++)
                {
                    RawFileXAsset header = instance.Reader.ReadStruct<RawFileXAsset>(StartAddress + Marshal.SizeOf<XAssetPoolData>() + (i * Marshal.SizeOf<RawFileXAsset>()));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }
                    else if (header.Size == 0)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        Type = Name,
                        Size = XAssetSize,
                        XAssetPool = this,
                        HeaderAddress = StartAddress + Marshal.SizeOf<XAssetPoolData>() + (i * Marshal.SizeOf<RawFileXAsset>()),
                    });
                }

                return results;
            }

            /// <summary>
            /// Exports the specified RawFile XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                RawFileXAsset header = instance.Reader.ReadStruct<RawFileXAsset>(xasset.HeaderAddress);

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