using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class RawFile : IXAssetPool
        {
            /// <summary>
            /// RawFile XAsset Structure
            /// </summary>
            private struct RawFileXAsset
            {
                public long NamePointer { get; set; }
                public int CompressedLength { get; set; }
                public int Len { get; set; }
                public long RawDataPtr { get; set; }
            }

            public override string Name => "Raw File";
            public override int Index => (int)XAssetPool.rawfile;
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
                    var header = instance.Reader.ReadStruct<RawFileXAsset>(StartAddress + (i * XAssetSize));

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
                        Information = $"Size: 0x{header.Len}"
                    });
                }

                return results;
            }

            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                var header = instance.Reader.ReadStruct<RawFileXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportFolder, xasset.Name);

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                MemoryStream DecodedCodeStream = Decode(instance.Reader.ReadBytes(header.RawDataPtr + 2, header.CompressedLength - 2));

                try
                {
                    using var outputStream = new FileStream(path, FileMode.Create);
                    DecodedCodeStream.CopyTo(outputStream);
                }
                catch
                {
                    return JekyllStatus.Exception;
                }

                Console.WriteLine($"Exported {Name} {xasset.Name}");

                return JekyllStatus.Success;
            }

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