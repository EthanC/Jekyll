using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class BlackOps3
    {
        public class Localize : IXAssetPool
        {
            public override string Name => "Localize Entry";

            public override int Index => (int)XAssetType.ASSET_TYPE_LOCALIZE_ENTRY;

            /// <summary>
            /// Structure of a Black Ops III LocalizeEntry.
            /// </summary>
            private struct LocalizeEntry
            {
                public long Value { get; set; }
                public long Name { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the Localize XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of Localize XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                XAssetPool pool = instance.Reader.ReadStruct<XAssetPool>(instance.Game.DBAssetPools + (Index * Marshal.SizeOf<XAssetPool>()));

                Entries = pool.Pool;
                ElementSize = pool.ItemSize;
                PoolSize = (uint)pool.ItemCount;

                if (IsValidPool(Name, ElementSize, Marshal.SizeOf<LocalizeEntry>()) == false)
                {
                    return results;
                }

                Dictionary<string, string> entries = new Dictionary<string, string>();

                for (int i = 0; i < PoolSize; i++)
                {
                    LocalizeEntry header = instance.Reader.ReadStruct<LocalizeEntry>(Entries + (i * ElementSize));

                    if (IsNullXAsset(header.Name))
                    {
                        continue;
                    }

                    string key = instance.Reader.ReadNullTerminatedString(header.Name).ToUpper();

                    if (entries.TryGetValue(key, out string _))
                    {
                        continue;
                    }

                    string value = instance.Reader.ReadNullTerminatedString(header.Value, nullCheck:true);
                    entries.Add(key, value);

                    Console.WriteLine($"Exported {Name} {key}");
                }

                string path = Path.Combine(instance.ExportPath, "localize.json");
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (StreamWriter file = File.CreateText(path))
                {
                    file.Write(JsonConvert.SerializeObject(entries, Formatting.Indented));
                }

                return results;
            }

            /// <summary>
            /// Exports the specified Localize XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                return JekyllStatus.Success;
            }
        }
    }
}