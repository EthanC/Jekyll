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

            public override int Index => (int)XAssetType.rawfile;

            /// <summary>
            /// Structure of a Modern Warfare Remastered RawFile XAsset.
            /// </summary>
            private struct RawFileXAsset
            {
                public long Name { get; set; }
                public int CompressedLen { get; set; }
                public int Len { get; set; }
                public long Buffer { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the RawFile XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of RawFile XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                Entries = instance.Reader.ReadStruct<long>(instance.Game.DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * Index));
                PoolSize = instance.Reader.ReadStruct<uint>(instance.Game.DBAssetPoolSizes + (Marshal.SizeOf<DBAssetPoolSize>() * Index));

                for (int i = 0; i < PoolSize; i++)
                {
                    RawFileXAsset header = instance.Reader.ReadStruct<RawFileXAsset>(Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<RawFileXAsset>()));

                    if (IsNullXAsset(header.Name))
                    {
                        continue;
                    }
                    else if (header.Len == 0)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.Name),
                        Type = Name,
                        Size = ElementSize,
                        XAssetPool = this,
                        HeaderAddress = Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<RawFileXAsset>()),
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

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.Name))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportPath, xasset.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                MemoryStream DecodedCodeStream = Decode(instance.Reader.ReadBytes(header.Buffer + 2, header.CompressedLen - 2));

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