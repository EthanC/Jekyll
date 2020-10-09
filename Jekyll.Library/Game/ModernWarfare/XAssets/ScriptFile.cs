using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class ScriptFile : IXAssetPool
        {
            public override string Name => "Script File";

            public override int Index => (int)XAssetType.scriptfile;

            public override long EndAddress { get { return Entries + (PoolSize * ElementSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Structure of a Modern Warfare ScriptFile XAsset.
            /// </summary>
            private struct ScriptFileXAsset
            {
                public long Name { get; set; }
                public int CompressedLen { get; set; }
                public int Len { get; set; }
                public long BytecodeLen { get; set; }
                public long Buffer { get; set; }
                public long Bytecode { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the ScriptFile XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of ScriptFile XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                DBAssetPool poolInfo = instance.Reader.ReadStruct<DBAssetPool>(instance.Game.BaseAddress + instance.Game.DBAssetPools + (Index * Marshal.SizeOf<DBAssetPool>()));

                Entries = poolInfo.Entries;
                ElementSize = poolInfo.ElementSize;
                PoolSize = poolInfo.PoolSize;

                for (int i = 0; i < PoolSize; i++)
                {
                    ScriptFileXAsset header = instance.Reader.ReadStruct<ScriptFileXAsset>(Entries + (i * ElementSize));

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
                        HeaderAddress = Entries + (i * ElementSize),
                    });
                }

                return results;
            }

            /// <summary>
            /// Exports the specified ScriptFile XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                ScriptFileXAsset header = instance.Reader.ReadStruct<ScriptFileXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.Name))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string addedScriptsFolder = Path.Combine(xasset.Name.Contains("scripts") ? "" : "scripts", xasset.Name);
                string path = Path.Combine(instance.ExportPath, addedScriptsFolder);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                MemoryStream DecodedCodeStream = Decode(instance.Reader.ReadBytes(header.Buffer + 2, header.CompressedLen - 2));
                using (FileStream outputStream = new FileStream(path, FileMode.Create))
                {
                    DecodedCodeStream.CopyTo(outputStream);
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