using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class LuaFile : IXAssetPool
        {
            public override string Name => "Lua File";

            public override int Index => (int)XAssetType.ASSET_TYPE_LUA_FILE;

            /// <summary>
            /// Structure of a Modern Warfare LuaFile XAsset.
            /// </summary>
            private struct LuaFileXAsset
            {
                public long Name { get; set; }
                public int Len { get; set; }
                public int StrippingType { get; set; }
                public long Buffer { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the LuaFile XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of LuaFile XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(instance.Game.BaseAddress + instance.Game.DBAssetPools + (Index * Marshal.SizeOf<DBAssetPool>()));

                Entries = pool.Entries;
                ElementSize = pool.ElementSize;
                PoolSize = pool.PoolSize;

                if (IsValidPool(Name, ElementSize, Marshal.SizeOf<LuaFileXAsset>()) == false)
                {
                    return results;
                }

                for (int i = 0; i < PoolSize; i++)
                {
                    LuaFileXAsset header = instance.Reader.ReadStruct<LuaFileXAsset>(Entries + (i * ElementSize));

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
            /// Exports the specified LuaFile XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                LuaFileXAsset header = instance.Reader.ReadStruct<LuaFileXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.Name))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportPath, xasset.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                byte[] buffer = instance.Reader.ReadBytes(header.Buffer, header.Len);

                File.WriteAllBytes(path, buffer);

                Console.WriteLine($"Exported {xasset.Type} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}