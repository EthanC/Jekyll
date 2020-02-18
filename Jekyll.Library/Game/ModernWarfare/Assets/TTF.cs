using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class TTF : IAssetPool
        {
            #region AssetStructures
            /// <summary>
            /// TTF Asset Structure
            /// </summary>
            private struct TTFAsset
            {
                public long NamePointer;
                public int AssetSize;
                public long RawDataPtr;
                public int Unk;
            }
            #endregion

            public override string Name => "TypeType Font";
            public override int Index => (int)AssetPool.ttf;
            public override long EndAddress { get { return StartAddress + (AssetCount * AssetSize); } set => throw new NotImplementedException(); }
            public override List<GameAsset> Load(JekyllInstance instance)
            {
                var results = new List<GameAsset>();

                var poolInfo = instance.Reader.ReadStruct<AssetPoolInfo>(instance.Game.BaseAddress + instance.Game.AssetPoolsAddresses[instance.Game.ProcessIndex] + (Index * 24));

                StartAddress = poolInfo.PoolPtr;
                AssetSize = poolInfo.AssetSize;
                AssetCount = poolInfo.PoolSize;

                for (int i = 0; i < AssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<TTFAsset>(StartAddress + (i * AssetSize));

                    if (IsNullAsset(header.NamePointer))
                        continue;

                    results.Add(new GameAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        HeaderAddress = StartAddress + (i * AssetSize),
                        AssetPool = this,
                        Type = Name,
                        Information = string.Format("Size: 0x{0:X}", header.AssetSize)
                    });
                }

                return results;
            }

            public override JekyllStatus Export(GameAsset asset, JekyllInstance instance)
            {
                var header = instance.Reader.ReadStruct<TTFAsset>(asset.HeaderAddress);

                if (asset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                    return JekyllStatus.MemoryChanged;

                string path = Path.Combine("export", instance.Game.Name, asset.Name);

                // Create path
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                byte[] buffer = instance.Reader.ReadBytes(header.RawDataPtr, (int)header.AssetSize);

                File.WriteAllBytes(path, buffer);

                Console.WriteLine($"Exported {Name} {asset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}