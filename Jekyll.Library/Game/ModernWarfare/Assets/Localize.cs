using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class Localize : IAssetPool
        {
            #region AssetStructures
            /// <summary>
            /// TTF Asset Structure
            /// </summary>
            private struct LocalizeAsset
            {
                public long NamePointer;
                public long RawDataPtr;
            }
            #endregion

            public override string Name => "Localized String";
            public override int Index => (int)AssetPool.localize;
            public override long EndAddress { get { return StartAddress + (AssetCount * AssetSize); } set => throw new NotImplementedException(); }

            public override List<GameAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameAsset>();

                var poolInfo = instance.Reader.ReadStruct<AssetPoolInfo>(instance.Game.BaseAddress + instance.Game.AssetPoolsAddresses[instance.Game.ProcessIndex] + (Index * 24));

                StartAddress = poolInfo.PoolPtr;
                AssetSize = poolInfo.AssetSize;
                AssetCount = poolInfo.PoolSize;

                Dictionary<string, int> dictionary = new Dictionary<string, int>();

                for (int i = 0; i < AssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<LocalizeAsset>(StartAddress + (i * AssetSize));

                    if (IsNullAsset(header.NamePointer))
                        continue;

                    // Not optimized
                    string name = instance.Reader.ReadNullTerminatedString(header.NamePointer);
                    int idx = name.IndexOf("/");
                    string fileName = name.Substring(0, idx);

                    if (dictionary.TryGetValue(fileName, out int value))
                    {
                        dictionary.Remove(fileName);
                        dictionary.Add(fileName, value + 1);
                    }
                    else
                    {
                        dictionary.Add(fileName, 0);
                    }
                }

                foreach (var file in dictionary)
                {
                    results.Add(new GameAsset()
                    {
                        Name = file.Key,
                        HeaderAddress = StartAddress,
                        AssetPool = this,
                        Type = Name,
                        Information = string.Format("Strings: 0x{0:X}", file.Value)
                    });
                }

                return results;
            }

            public override JekyllStatus Export(GameAsset asset, JekyllInstance instance)
            {
                string path = Path.Combine("export", instance.Game.Name, "localizedstrings", asset.Name + ".json");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var result = new StringBuilder();

                result.AppendLine("{");
                for (int i = 0; i < AssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<LocalizeAsset>(StartAddress + (i * AssetSize));

                    if (IsNullAsset(header.NamePointer))
                        continue;

                    string name = instance.Reader.ReadNullTerminatedString(header.NamePointer);
                    int idx = name.IndexOf("/");
                    string fileName = name.Substring(0, idx);

                    if (asset.Name == fileName)
                    {
                        string key = $"{asset.Name}/{name.Substring(idx + 1)}";
                        string value = instance.Reader.ReadNullTerminatedString(header.RawDataPtr).Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"");

                        result.AppendLine($"    \"{key.ToUpper()}\": \"{value}\",");
                    }
                }
                result.Remove((result.Length - 3), 3);
                result.AppendLine();
                result.AppendLine("}");

                File.WriteAllText(path, result.ToString());

                Console.WriteLine($"Exported {Name} {asset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}