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

            public override int Index => (int)XAssetType.stringtable;

            public override long EndAddress { get { return Entries + (PoolSize * ElementSize); } set => throw new NotImplementedException(); }

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

                StringBuilder result = new StringBuilder();

                int index = 0;
                for (int x = 0; x < header.RowCount; x++)
                {
                    for (int y = 0; y < header.ColumnCount; y++)
                    {
                        int stringIndex = instance.Reader.ReadInt16(header.CellIndices + (2 * index));
                        string str = instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(header.Strings + (8 * stringIndex)));

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