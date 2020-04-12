using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class Localize : IXAssetPool
        {
            /// <summary>
            /// Localize XAsset Structure
            /// </summary>
            private struct LocalizeXAsset
            {
                public long NamePointer { get; set; }
                public long RawDataPtr { get; set; }
            }

            public override string Name => "Localize";
            public override int Index => (int)XAssetPool.localize;
            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }

            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameXAsset>();

                var poolInfo = instance.Reader.ReadStruct<XAssetPoolInfo>(instance.Game.BaseAddress + instance.Game.XAssetPoolsAddresses[instance.Game.ProcessIndex] + (Index * 24));

                StartAddress = poolInfo.PoolPtr;
                XAssetSize = poolInfo.XAssetSize;
                XAssetCount = poolInfo.PoolSize;

                Dictionary<string, string> entries = new Dictionary<string, string>();

                for (int i = 0; i < XAssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<LocalizeXAsset>(StartAddress + (i * XAssetSize));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }

                    string key = instance.Reader.ReadNullTerminatedString(header.NamePointer).ToUpper();

                    if (entries.TryGetValue(key, out string _))
                    {
                        continue;
                    }
                    else
                    {
                        string value = instance.Reader.ReadNullTerminatedString(header.RawDataPtr);
                        entries.Add(key, value);

                        Console.WriteLine($"Exported {Name} {key}");
                    }
                }

                string path = Path.Combine(instance.ExportFolder, "localize.json");
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (StreamWriter file = File.CreateText(path))
                {
                    file.Write(JsonConvert.SerializeObject(entries, Formatting.Indented));
                }

                return results;
            }

            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                return JekyllStatus.Success;
            }
        }
    }
}