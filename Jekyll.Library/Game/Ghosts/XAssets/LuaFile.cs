using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class Ghosts
    {
        public class LuaFile : IXAssetPool
        {
            public override string Name => "Lua File";

            public override int Index => (int)XAssetType.ASSET_TYPE_LUA_FILE;

            /// <summary>
            /// Structure of a Ghosts LuaFile XAsset.
            /// </summary>
            private struct LuaFileXAsset
            {
                public long Name { get; set; }
                public int Len { get; set; }
                public byte StrippingType { get; set; }
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

                Entries = instance.Reader.ReadStruct<long>(instance.Game.DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * Index));
                PoolSize = instance.Reader.ReadStruct<uint>(instance.Game.DBAssetPoolSizes + (Marshal.SizeOf<DBAssetPoolSize>() * Index));

                for (int i = 0; i < PoolSize; i++)
                {
                    LuaFileXAsset header = instance.Reader.ReadStruct<LuaFileXAsset>(Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<LuaFileXAsset>()));

                    if (IsNullXAsset(header.Name))
                    {
                        continue;
                    }
                    else if (header.Len == 0)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.Name),
                        Type = Name,
                        Size = ElementSize,
                        XAssetPool = this,
                        HeaderAddress = Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<LuaFileXAsset>()),
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