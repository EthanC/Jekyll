using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfareRemastered
    {
        public class TTF : IXAssetPool
        {
            public override string Name => "TrueType Font";

            public override int Index => (int)XAssetType.ttf;

            /// <summary>
            /// Structure of a Modern Warfare Remastered TTFDef.
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

                Entries = instance.Reader.ReadStruct<long>(instance.Game.DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * Index));
                PoolSize = instance.Reader.ReadStruct<uint>(instance.Game.DBAssetPoolSizes + (Marshal.SizeOf<DBAssetPoolSize>() * Index));

                for (int i = 0; i < PoolSize; i++)
                {
                    TTFDef header = instance.Reader.ReadStruct<TTFDef>(Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<TTFDef>()));

                    if (IsNullXAsset(header.Name))
                    {
                        continue;
                    }
                    else if (instance.Reader.ReadNullTerminatedString(header.Name).EndsWith(".ttf") is false)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.Name),
                        Type = Name,
                        Size = ElementSize,
                        XAssetPool = this,
                        HeaderAddress = Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<TTFDef>()),
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