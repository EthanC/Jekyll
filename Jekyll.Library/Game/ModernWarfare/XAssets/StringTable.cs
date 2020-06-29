using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class StringTable : IXAssetPool
        {
            public override string Name => "String Table";

            public override int Index => (int)XAssetPool.stringtable;

            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Structure of a Modern Warfare StringTable XAsset.
            /// </summary>
            private struct StringTableXAsset
            {
                public long NamePointer { get; set; }
                public int ColumnCount { get; set; }
                public int RowCount { get; set; }
                public int Unknown1 { get; set; }
                public long CellsPointer { get; set; }
                public long IndicesPointer { get; set; }
                public long StringsPtr { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the StringTable XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of StringTable XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                XAssetPoolData poolInfo = instance.Reader.ReadStruct<XAssetPoolData>(instance.Game.BaseAddress + instance.Game.XAssetPoolsAddress + (Index * Marshal.SizeOf<XAssetPoolData>()));

                StartAddress = poolInfo.PoolPointer;
                XAssetSize = poolInfo.XAssetSize;
                XAssetCount = poolInfo.PoolSize;

                for (int i = 0; i < XAssetCount; i++)
                {
                    StringTableXAsset header = instance.Reader.ReadStruct<StringTableXAsset>(StartAddress + (i * XAssetSize));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        Type = Name,
                        Size = XAssetSize,
                        XAssetPool = this,
                        HeaderAddress = StartAddress + (i * XAssetSize),
                    });
                }

                return results;
            }

            /// <summary>
            /// Exports the specified StringTable XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                StringTableXAsset header = instance.Reader.ReadStruct<StringTableXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportPath, xasset.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                StringBuilder result = new StringBuilder();

                int index = 0;
                for (int x = 0; x < header.RowCount; x++)
                {
                    for (int y = 0; y < header.ColumnCount; y++)
                    {
                        int stringIndex = instance.Reader.ReadInt16(header.CellsPointer + (2 * index));
                        string str = instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(header.StringsPtr + (8 * stringIndex)));

                        result.Append($"{str},");

                        index++;
                    }

                    result.AppendLine();
                }

                File.WriteAllText(path, result.ToString());

                Console.WriteLine($"Exported {xasset.Type} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}