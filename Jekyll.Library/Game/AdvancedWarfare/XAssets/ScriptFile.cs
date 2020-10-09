using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class AdvancedWarfare
    {
        public class ScriptFile : IXAssetPool
        {
            public override string Name => "Script File";

            public override int Index => (int)XAssetType.scriptfile;

            public override long EndAddress { get { return Entries + (PoolSize * ElementSize); } set => throw new NotImplementedException(); }

            /// <summary>
            /// Structure of an Advanced Warfare ScriptFile XAsset.
            /// </summary>
            private struct ScriptFileXAsset
            {
                public long Name { get; set; }
                public int CompressedLen { get; set; }
                public int Len { get; set; }
                public int BytecodeLen { get; set; }
                public long Buffer { get; set; }
                public long Bytecode { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the ScriptFile XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of LuaFile XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                Entries = instance.Reader.ReadStruct<long>(instance.Game.DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * Index));
                PoolSize = instance.Reader.ReadStruct<int>(instance.Game.DBAssetPoolSizes + (Marshal.SizeOf<DBAssetPoolSize>() * Index));

                for (int i = 0; i < PoolSize; i++)
                {
                    ScriptFileXAsset header = instance.Reader.ReadStruct<ScriptFileXAsset>(Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<ScriptFileXAsset>()));

                    if (IsNullXAsset(header.Name))
                    {
                        continue;
                    }
                    else if (header.Buffer == 0)
                    {
                        continue;
                    }

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.Name),
                        Type = Name,
                        Size = ElementSize,
                        XAssetPool = this,
                        HeaderAddress = Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<ScriptFileXAsset>()),
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
                string path = Path.Combine(instance.ExportPath, addedScriptsFolder.Contains(".gsc") ? "" : addedScriptsFolder + ".gsc");
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                MemoryStream DecodedCodeStream = Decode(instance.Reader.ReadBytes(header.Buffer + 2, header.CompressedLen - 2));

                try
                {
                    using (var outputStream = new FileStream(path, FileMode.Create))
                    {
                        DecodedCodeStream.CopyTo(outputStream);
                    }
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

                try
                {
                    using (DeflateStream deflateStream = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(output);
                    }
                }
                catch
                {
                    return null;
                }

                output.Flush();
                output.Position = 0;

                return output;
            }
        }
    }
}