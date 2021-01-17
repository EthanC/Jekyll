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

            public override int Index => (int)XAssetType.ASSET_TYPE_STRINGTABLE;

            /// <summary>
            /// Structure of a Modern Warfare StringTable XAsset.
            /// </summary>
            private struct StringTableXAsset
            {
                public long Name { get; set; }
                public int ColumnCount { get; set; }
                public int RowCount { get; set; }
                public int UniqueCellCount { get; set; }
                public long CellIndices { get; set; }
                public long Hashes { get; set; }
                public long Strings { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the StringTable XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of StringTable XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(instance.Game.BaseAddress + instance.Game.DBAssetPools + (Index * Marshal.SizeOf<DBAssetPool>()));

                Entries = pool.Entries;
                ElementSize = pool.ElementSize;
                PoolSize = pool.PoolSize;

                if (IsValidPool(Name, ElementSize, Marshal.SizeOf<StringTableXAsset>()) == false)
                {
                    return results;
                }

                for (int i = 0; i < PoolSize; i++)
                {
                    StringTableXAsset header = instance.Reader.ReadStruct<StringTableXAsset>(Entries + (i * ElementSize));

                    if (IsNullXAsset(header.Name))
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
            /// Exports the specified StringTable XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                StringTableXAsset header = instance.Reader.ReadStruct<StringTableXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.Name))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportPath, xasset.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                StringBuilder stringTable = new StringBuilder();

                int index = 0;
                for (int x = 0; x < header.RowCount; x++)
                {
                    for (int y = 0; y < header.ColumnCount; y++)
                    {
                        int cell = instance.Reader.ReadInt16(header.CellIndices + (2 * index));
                        string value = instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(header.Strings + (8 * cell)));

                        stringTable.Append(value);

                        if (y != (header.ColumnCount - 1))
                        {
                            stringTable.Append(",");
                        }

                        index++;
                    }

                    stringTable.AppendLine();
                }

                File.WriteAllText(path, stringTable.ToString());

                Console.WriteLine($"Exported {xasset.Type} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}