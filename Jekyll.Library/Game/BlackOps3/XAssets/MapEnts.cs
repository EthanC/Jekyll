using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class BlackOps3
    {
        public class MapEnts : IXAssetPool
        {
            public override string Name => "Map Entities";

            public override int Index => (int)XAssetType.map_ents;

            public override long EndAddress { get { return Entries + (PoolSize * ElementSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Structure of a Black Ops III MapEnts XAsset.
            /// </summary>
            private struct MapEntsXAsset
            {
                public long Name { get; set; }
                public long EntityString { get; set; }
                // TODO: Fill remaining unknown.
            }

            /// <summary>
            /// Load the valid XAssets for the MapEnts XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of MapEnts XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(instance.Game.DBAssetPools + (Index * Marshal.SizeOf<DBAssetPool>()));

                Entries = pool.Entries;
                ElementSize = pool.ElementSize;
                PoolSize = pool.PoolSize;

                for (int i = 0; i < PoolSize; i++)
                {
                    MapEntsXAsset header = instance.Reader.ReadStruct<MapEntsXAsset>(Entries + (i * ElementSize));

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
            /// Exports the specified MapEnts XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                MapEntsXAsset header = instance.Reader.ReadStruct<MapEntsXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.Name))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportPath, xasset.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                File.WriteAllText(path, instance.Reader.ReadNullTerminatedString(header.EntityString));

                Console.WriteLine($"Exported {xasset.Type} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}