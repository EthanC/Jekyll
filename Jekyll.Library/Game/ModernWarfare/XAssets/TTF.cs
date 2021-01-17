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

            public override int Index => (int)XAssetType.ASSET_TYPE_TTF;

            /// <summary>
            /// Structure of a Modern Warfare TTFDef.
            /// </summary>
            private struct TTFDef
            {
                public long Name { get; set; }
                public int FileLen { get; set; }
                public long File { get; set; }
                public int FtFace { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the TTF XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of TTF XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(instance.Game.BaseAddress + instance.Game.DBAssetPools + (Index * Marshal.SizeOf<DBAssetPool>()));

                Entries = pool.Entries;
                ElementSize = pool.ElementSize;
                PoolSize = pool.PoolSize;

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