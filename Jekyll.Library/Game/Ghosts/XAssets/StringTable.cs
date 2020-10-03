using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace JekyllLibrary.Library
{
    public partial class Ghosts
    {
        public class StringTable : IXAssetPool
        {
            public override string Name => "String Table";

            public override int Index => (int)XAssetPool.stringtable;

            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Structure of an Ghosts StringTable XAsset.
            /// </summary>
            private struct StringTableXAsset
            {
                public long NamePointer { get; set; }
                public int ColumnCount { get; set; }
                public int RowCount { get; set; }
                public long DataPointer { get; set; }
            }

            /// <summary>
            /// Structure of an Ghosts StringTable XAsset's data.
            /// </summary>
            private struct StringTableData
            {
                public long StringPointer { get; set; }
                public int Hash { get; set; }
                public int NullPadding { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the StringTable XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of StringTable XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                StartAddress = instance.Reader.ReadStruct<long>(instance.Game.XAssetPoolsAddress + (Marshal.SizeOf<XAssetPoolData>() * Index));
                XAssetSize = instance.Reader.ReadStruct<int>(instance.Game.XAssetPoolSizesAddress + (Marshal.SizeOf<XAssetPoolSizesData>() * Index));

                for (int i = 0; i < XAssetSize; i++)
                {
                    StringTableXAsset header = instance.Reader.ReadStruct<StringTableXAsset>(StartAddress + Marshal.SizeOf<XAssetPoolData>() + (i * Marshal.SizeOf<StringTableXAsset>()));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }
                    else if (instance.Reader.ReadNullTerminatedString(header.NamePointer).EndsWith(".csv") is false)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        Type = Name,
                        Size = XAssetSize,
                        XAssetPool = this,
                        HeaderAddress = StartAddress + Marshal.SizeOf<XAssetPoolData>() + (i * Marshal.SizeOf<StringTableXAsset>()),
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

                StringBuilder stringTable = new StringBuilder();

                for (int x = 0; x < header.RowCount; x++)
                {
                    for (int y = 0; y < header.ColumnCount; y++)
                    {
                        StringTableData data = instance.Reader.ReadStruct<StringTableData>(header.DataPointer);
                        string cell = instance.Reader.ReadNullTerminatedString(data.StringPointer);

                        stringTable.Append($"{cell},");

                        header.DataPointer += Marshal.SizeOf<StringTableData>();
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