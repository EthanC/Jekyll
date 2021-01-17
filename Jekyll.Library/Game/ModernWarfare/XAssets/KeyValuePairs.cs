using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class KeyValuePairs : IXAssetPool
        {
            public override string Name => "Key/Value Pair";

            public override int Index => (int)XAssetType.ASSET_TYPE_KEYVALUEPAIRS;

            /// <summary>
            /// Structure of a Modern Warfare KeyValuePairs XAsset.
            /// </summary>
            private struct KeyValuePairsXAsset
            {
                public long Name { get; set; }
                public int NumVariables { get; set; }
                public long KeyValuePairs { get; set; }
            }

            private struct KeyValuePair
            {
                public int KeyHash { get; set; }
                public long Value { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the Key/Value Pairs XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of Key/Value Pairs XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(instance.Game.BaseAddress + instance.Game.DBAssetPools + (Index * Marshal.SizeOf<DBAssetPool>()));

                Entries = pool.Entries;
                ElementSize = pool.ElementSize;
                PoolSize = pool.PoolSize;

                if (IsValidPool(Name, ElementSize, Marshal.SizeOf<KeyValuePairsXAsset>()) == false)
                {
                    return results;
                }

                Dictionary<string, string> entries = new Dictionary<string, string>();

                for (int i = 0; i < PoolSize; i++)
                {
                    KeyValuePairsXAsset header = instance.Reader.ReadStruct<KeyValuePairsXAsset>(Entries + (i * ElementSize));

                    if (IsNullXAsset(header.Name))
                    {
                        continue;
                    }

                    string key = instance.Reader.ReadNullTerminatedString(header.Name);

                    if (entries.TryGetValue(key, out string _))
                    {
                        continue;
                    }

                    KeyValuePair data = instance.Reader.ReadStruct<KeyValuePair>(header.KeyValuePairs);

                    string value = instance.Reader.ReadNullTerminatedString(data.Value);
                    entries.Add(key, value);

                    Console.WriteLine($"Exported {Name} {key}");
                }

                string path = Path.Combine(instance.ExportPath, "keyValuePairs.json");
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (StreamWriter file = File.CreateText(path))
                {
                    file.Write(JsonConvert.SerializeObject(entries, Formatting.Indented));
                }

                return results;
            }

            /// <summary>
            /// Exports the specified Key/Value Pair XAsset.
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