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
            public override string Name => "Localize";

            public override int Index => (int)XAssetPool.localize;

            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Structure of a Black Ops III Localize XAsset.
            /// </summary>
            private struct LocalizeXAsset
            {
                public long StringPointer { get; set; }
                public long NamePointer { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the Localize XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of Localize XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                XAssetPoolData poolInfo = instance.Reader.ReadStruct<XAssetPoolData>(instance.Game.XAssetPoolsAddress + (Index * Marshal.SizeOf<XAssetPoolData>()));

                StartAddress = poolInfo.PoolPointer;
                XAssetSize = poolInfo.XAssetSize;
                XAssetCount = poolInfo.PoolSize;

                Dictionary<string, string> entries = new Dictionary<string, string>();

                for (int i = 0; i < XAssetCount; i++)
                {
                    LocalizeXAsset header = instance.Reader.ReadStruct<LocalizeXAsset>(StartAddress + (i * XAssetSize));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }

                    string key = instance.Reader.ReadNullTerminatedString(header.NamePointer).ToUpper();

                    if (entries.TryGetValue(key, out string _))
                    {
                        continue;
                    }

                    string value = instance.Reader.ReadNullTerminatedString(header.StringPointer);
                    entries.Add(key, value);

                    Console.WriteLine($"Exported {Name} {key}");
                }

                string path = Path.Combine(instance.ExportPath, "localize.json");
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (StreamWriter file = File.CreateText(path))
                {
                    file.Write(JsonConvert.SerializeObject(entries, Formatting.Indented));
                }

                return new List<GameXAsset>();
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