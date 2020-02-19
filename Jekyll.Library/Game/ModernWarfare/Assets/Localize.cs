using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class Localize : IAssetPool
        {
            #region AssetStructures
            /// <summary>
            /// Localize Asset Structure
            /// </summary>
            private struct LocalizeAsset
            {
                public long NamePointer;
                public long RawDataPtr;
            }
            #endregion

            public override string Name => "Localize";
            public override int Index => (int)AssetPool.localize;
            public override long EndAddress { get { return StartAddress + (AssetCount * AssetSize); } set => throw new NotImplementedException(); }

            public override List<GameAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameAsset>();

                var poolInfo = instance.Reader.ReadStruct<AssetPoolInfo>(instance.Game.BaseAddress + instance.Game.AssetPoolsAddresses[instance.Game.ProcessIndex] + (Index * 24));

                StartAddress = poolInfo.PoolPtr;
                AssetSize = poolInfo.AssetSize;
                AssetCount = poolInfo.PoolSize;

                Dictionary<string, string> entries = new Dictionary<string, string>();

                for (int i = 0; i < AssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<LocalizeAsset>(StartAddress + (i * AssetSize));

                    if (IsNullAsset(header.NamePointer))
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
                        Console.WriteLine($"Exported {Name} {key}");

                        string value = instance.Reader.ReadNullTerminatedString(header.RawDataPtr);
                        entries.Add(key, value);
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

            public override JekyllStatus Export(GameAsset asset, JekyllInstance instance)
            {
                return JekyllStatus.Success;
            }
        }
    }
}