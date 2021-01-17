using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class BlackOps3
    {
        public class ScriptFile : IXAssetPool
        {
            public override string Name => "Script File";

            public override int Index => (int)XAssetType.ASSET_TYPE_SCRIPTPARSETREE;

            /// <summary>
            /// Structure of a Black Ops III ScriptParseTree.
            /// </summary>
            private struct ScriptParseTree
            {
                public long Name { get; set; }
                public int Len { get; set; }
                public long Buffer { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the ScriptParseTree XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of ScriptParseTree XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                XAssetPool pool = instance.Reader.ReadStruct<XAssetPool>(instance.Game.DBAssetPools + (Index * Marshal.SizeOf<XAssetPool>()));

                Entries = pool.Pool;
                ElementSize = pool.ItemSize;
                PoolSize = (uint)pool.ItemCount;

                if (IsValidPool(Name, ElementSize, Marshal.SizeOf<ScriptParseTree>()) == false)
                {
                    return results;
                }

                for (int i = 0; i < PoolSize; i++)
                {
                    ScriptParseTree header = instance.Reader.ReadStruct<ScriptParseTree>(Entries + (i * ElementSize));

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
                        HeaderAddress = Entries + (i * ElementSize),
                    });
                }

                return results;
            }

            /// <summary>
            /// Exports the specified ScriptParseTree XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                ScriptParseTree header = instance.Reader.ReadStruct<ScriptParseTree>(xasset.HeaderAddress);

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