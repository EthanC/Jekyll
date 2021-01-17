using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class BlackOps3
    {
        public class TTF : IXAssetPool
        {
            public override string Name => "TrueType Font";

            public override int Index => (int)XAssetType.ASSET_TYPE_TTF;

            /// <summary>
            /// Structure of a Black Ops III TTFDef.
            /// </summary>
            private struct TTFDef
            {
                public long Name { get; set; }
                public int FileLen { get; set; }
                public long File { get; set; }
                public long FtFace { get; set; }
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 131072)]
                public byte[] KerningCache;
            }

            /// <summary>
            /// Load the valid XAssets for the TTF XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of TTF XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                XAssetPool pool = instance.Reader.ReadStruct<XAssetPool>(instance.Game.DBAssetPools + (Index * Marshal.SizeOf<XAssetPool>()));

                Entries = pool.Pool;
                ElementSize = pool.ItemSize;
                PoolSize = (uint)pool.ItemCount;

                if (IsValidPool(Name, ElementSize, Marshal.SizeOf<TTFDef>()) == false)
                {
                    return results;
                }

                for (int i = 0; i < PoolSize; i++)
                {
                    TTFDef header = instance.Reader.ReadStruct<TTFDef>(Entries + (i * ElementSize));

                    if (IsNullXAsset(header.Name))
                    {
                        continue;
                    }
                    else if (header.FileLen == 0)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.Name),
                        Type = Name,
                        Size = ElementSize,
                        XAssetPool = this,
                        HeaderAddress = Entries + (i * ElementSize),
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
                TTFDef header = instance.Reader.ReadStruct<TTFDef>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.Name))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportPath, xasset.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                byte[] buffer = instance.Reader.ReadBytes(header.File, header.FileLen);

                File.WriteAllBytes(path, buffer);

                Console.WriteLine($"Exported {xasset.Type} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}