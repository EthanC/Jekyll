using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfareRemastered
    {
        public class RawFile : IXAssetPool
        {
            public override string Name => "Raw File";

            public override int Index => (int)XAssetPool.rawfile;

            public override long EndAddress { get { return StartAddress + (XAssetCount * XAssetSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Structure of a Modern Warfare Remastered RawFile XAsset.
            /// </summary>
            private struct RawFileXAsset
            {
                public long NamePointer { get; set; }
                public int CompressedSize { get; set; }
                public int Size { get; set; }
                public long DataPointer { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the RawFile XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of RawFile XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                StartAddress = instance.Reader.ReadStruct<long>(instance.Game.XAssetPoolsAddress + (Marshal.SizeOf<XAssetPoolData>() * Index));
                XAssetSize = instance.Reader.ReadStruct<int>(instance.Game.XAssetPoolSizesAddress + (Marshal.SizeOf<XAssetPoolSizesData>() * Index));

                for (int i = 0; i < XAssetSize; i++)
                {
                    RawFileXAsset header = instance.Reader.ReadStruct<RawFileXAsset>(StartAddress + Marshal.SizeOf<XAssetPoolData>() + (i * Marshal.SizeOf<RawFileXAsset>()));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }
                    else if (header.Size == 0)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        Type = Name,
                        Size = XAssetSize,
                        XAssetPool = this,
                        HeaderAddress = StartAddress + Marshal.SizeOf<XAssetPoolData>() + (i * Marshal.SizeOf<RawFileXAsset>()),
                    });
                }

                return results;
            }

            /// <summary>
            /// Exports the specified RawFile XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                RawFileXAsset header = instance.Reader.ReadStruct<RawFileXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportPath, xasset.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                MemoryStream DecodedCodeStream = Decode(instance.Reader.ReadBytes(header.DataPointer + 2, header.CompressedSize - 2));

                try
                {
                    using FileStream outputStream = new FileStream(path, FileMode.Create);
                    DecodedCodeStream.CopyTo(outputStream);
                }
                catch
                {
                    return JekyllStatus.Exception;
                }

                Console.WriteLine($"Exported {xasset.Type} {xasset.Name}");

                return JekyllStatus.Success;
            }

            /// <summary>
            /// Decompress the specified array of bytes.
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public static MemoryStream Decode(byte[] data)
            {
                MemoryStream output = new MemoryStream();
                MemoryStream input = new MemoryStream(data);

                using (DeflateStream deflateStream = new DeflateStream(input, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(output);
                }

                output.Flush();
                output.Position = 0;

                return output;
            }
        }
    }
}