using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class BlackOps4
    {
        public class RawFile : IXAssetPool
        {
            public override string Name => "Raw File";

            public override int Index => (int)XAssetType.rawfile;

            /// <summary>
            /// Structure of a Black Ops 4 RawFile XAsset.
            /// </summary>
            private struct RawFileXAsset
            {
                public long Name { get; set; }
                public long NullPadding1 { get; set; }
                public int Len { get; set; }
                public int CompressedLen { get; set; }
                public long Buffer { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the RawFile XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of RawFile XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(instance.Game.DBAssetPools + (Index * Marshal.SizeOf<DBAssetPool>()));

                Entries = pool.Entries;
                ElementSize = pool.ElementSize;
                PoolSize = pool.PoolSize;

                if (IsValidPool(Name, ElementSize, Marshal.SizeOf<RawFileXAsset>()) == false)
                {
                    return results;
                }

                for (int i = 0; i < PoolSize; i++)
                {
                    RawFileXAsset header = instance.Reader.ReadStruct<RawFileXAsset>(Entries + (i * ElementSize));

                    if (header.Len == 0)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadBytesToString(Entries + (i * ElementSize)).ToLower(),
                        Type = Name,
                        Size = ElementSize,
                        XAssetPool = this,
                        HeaderAddress = Entries + (i * Marshal.SizeOf<RawFileXAsset>()),
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

                string path = Path.Combine(instance.ExportPath, "rawfile/" + xasset.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                byte[] buffer = instance.Reader.ReadBytes(header.Buffer, header.Len);

                File.WriteAllBytes(path, buffer);

                Console.WriteLine($"Exported {xasset.Type} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}