using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class BlackOps2
    {
        public class MapEnts : IXAssetPool
        {
            public override string Name => "Map Entities";

            public override int Index => (int)XAssetType.ASSET_TYPE_MAP_ENTS;

            /// <summary>
            /// Structure of a Black Ops II MapEnts XAsset.
            /// </summary>
            private struct MapEntsXAsset
            {
                public uint Name { get; set; }
                public uint EntityString { get; set; }
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
                public byte[] Unused;
            }

            /// <summary>
            /// Load the valid XAssets for the MapEnts XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of MapEnts XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                Entries = instance.Reader.ReadStruct<uint>(instance.Game.DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * Index));
                PoolSize = instance.Reader.ReadStruct<uint>(instance.Game.DBAssetPoolSizes + (Marshal.SizeOf<DBAssetPoolSize>() * Index));

                for (int i = 0; i < PoolSize; i++)
                {
                    MapEntsXAsset header = instance.Reader.ReadStruct<MapEntsXAsset>(Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<MapEntsXAsset>()));

                    if (IsNullXAsset(header.Name))
                    {
                        continue;
                    }
                    else if (instance.Reader.ReadNullTerminatedString(header.Name).EndsWith(".d3dbsp") is false)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.Name),
                        Type = Name,
                        Size = ElementSize,
                        XAssetPool = this,
                        HeaderAddress = Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<MapEntsXAsset>()),
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