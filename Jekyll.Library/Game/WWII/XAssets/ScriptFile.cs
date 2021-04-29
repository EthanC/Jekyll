using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace JekyllLibrary.Library
{
    public partial class WWII
    {
        public class ScriptFile : IXAssetPool
        {
            public override string Name => "Script File";

            public override int Index => (int)XAssetType.scriptfile;

            /// <summary>
            /// Structure of a WWII ScriptFile XAsset.
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
            /// <returns>List of ScriptFile XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                List<GameXAsset> results = new List<GameXAsset>();

                Entries = instance.Reader.ReadStruct<long>(instance.Game.DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * Index));
                PoolSize = instance.Reader.ReadStruct<uint>(instance.Game.DBAssetPoolSizes + (Marshal.SizeOf<DBAssetPoolSize>() * Index));

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
                string path = Path.Combine(instance.ExportPath, addedScriptsFolder.Contains(".gsc") ? addedScriptsFolder + "bin" : addedScriptsFolder + ".gscbin");
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                byte[] filename = Encoding.UTF8.GetBytes(xasset.Name + char.MinValue);
                byte[] compressedLen = BitConverter.GetBytes(header.CompressedLen);
                byte[] len = BitConverter.GetBytes(header.Len);
                byte[] bytecodeLen = BitConverter.GetBytes(header.BytecodeLen);
                byte[] buffer = instance.Reader.ReadBytes(header.Buffer, header.CompressedLen);
                byte[] bytecode = instance.Reader.ReadBytes(header.Bytecode, header.BytecodeLen);

                byte[] file = new byte[filename.Length + compressedLen.Length + len.Length + bytecodeLen.Length + buffer.Length + bytecode.Length];

                Buffer.BlockCopy(filename, 0, file, 0, filename.Length);
                Buffer.BlockCopy(compressedLen, 0, file, filename.Length, compressedLen.Length);
                Buffer.BlockCopy(len, 0, file, filename.Length + compressedLen.Length, len.Length);
                Buffer.BlockCopy(bytecodeLen, 0, file, filename.Length + compressedLen.Length + len.Length, bytecodeLen.Length);
                Buffer.BlockCopy(buffer, 0, file, filename.Length + compressedLen.Length + len.Length + bytecodeLen.Length, buffer.Length);
                Buffer.BlockCopy(bytecode, 0, file, filename.Length + compressedLen.Length + len.Length + bytecodeLen.Length + buffer.Length, bytecode.Length);

                try
                {
                    File.WriteAllBytes(path, file);
                }
                catch
                {
                    return JekyllStatus.Exception;
                }

                Console.WriteLine($"Exported {xasset.Type} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}