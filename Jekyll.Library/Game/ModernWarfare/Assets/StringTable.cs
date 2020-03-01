using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class StringTable : IXAssetPool
        {
            /// <summary>
            /// String Table XAsset Structure
            /// </summary>
            private struct StringTableXAsset
            {
                /// <summary>
                /// String Table Cell Structure
                /// </summary>
                public long NamePointer { get; set; }
                public int ColumnCount { get; set; }
                public int RowCount { get; set; }
                public int Unk { get; set; }
                public long CellsPointer { get; set; }
                public long IndicesPointer { get; set; }
                public long StringsPtr { get; set; }
            }

            public override string Name => "String Table";
            public override int Index => (int)XAssetPool.stringtable;
            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }

            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameXAsset>();

                var poolInfo = instance.Reader.ReadStruct<XAssetPoolInfo>(instance.Game.BaseAddress + instance.Game.XAssetPoolsAddresses[instance.Game.ProcessIndex] + (Index * 24));

                StartAddress = poolInfo.PoolPtr;
                XAssetSize = poolInfo.XAssetSize;
                XAssetCount = poolInfo.PoolSize;

                for (int i = 0; i < XAssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<StringTableXAsset>(StartAddress + (i * XAssetSize));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        HeaderAddress = StartAddress + (i * XAssetSize),
                        XAssetPool = this,
                        Type = Name,
                        Information = $"Rows: {header.RowCount}, Columns: {header.ColumnCount}"
                    });
                }

                return results;
            }

            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                var header = instance.Reader.ReadStruct<StringTableXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportFolder, xasset.Name);

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                var result = new StringBuilder();

                // Rows for the table
                int index = 0;
                for (int x = 0; x < header.RowCount; x++)
                {
                    // Columns for the row
                    for (int y = 0; y < header.ColumnCount; y++)
                    {
                        int stringIndex = instance.Reader.ReadInt16(header.CellsPointer + (2 * index));
                        string str = instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(header.StringsPtr + (8 * stringIndex)));
                        result.Append(str + ",");
                        index++;
                    }

                    result.AppendLine();
                }

                File.WriteAllText(path, result.ToString());

                Console.WriteLine($"Exported {Name} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}