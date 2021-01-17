using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace JekyllLibrary.Library
{
    public partial class BlackOps3
    {
        public class StringTable : IXAssetPool
        {
            public override string Name => "String Table";

            public override int Index => (int)XAssetType.ASSET_TYPE_STRINGTABLE;

            /// <summary>
            /// Structure of a Black Ops III StringTable XAsset.
            /// </summary>
            private struct StringTableXAsset
            {
                public long Name { get; set; }
                public int ColumnCount { get; set; }
                public int RowCount { get; set; }
                public long Values { get; set; }
                public long CellIndex { get; set; }
            }

            /// <summary>
            /// Structure of a Black Ops III StringTable Cell.
            /// </summary>
            private struct StringTableCell
            {
                public long String { get; set; }
                public int Hash { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the StringTable XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of StringTable XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                XAssetPool pool = instance.Reader.ReadStruct<XAssetPool>(instance.Game.DBAssetPools + (Index * Marshal.SizeOf<XAssetPool>()));

                Entries = pool.Pool;
                ElementSize = pool.ItemSize;
                PoolSize = (uint)pool.ItemCount;

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

                for (int y = 0; y < header.RowCount; y++)
                {
                    for (int x = 0; x < header.ColumnCount; x++)
                    {
                        StringTableCell cell = instance.Reader.ReadStruct<StringTableCell>(header.Values);
                        string value = instance.Reader.ReadNullTerminatedString(cell.String);

                        stringTable.Append(value);

                        if (y != (header.ColumnCount - 1))
                        {
                            stringTable.Append(",");
                        }

                        header.Values += Marshal.SizeOf<StringTableCell>();
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