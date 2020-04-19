using System;
using System.Collections.Generic;
using System.IO;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class DataDefinition : IXAssetPool
        {
            /// <summary>
            /// Data Definition XAsset Structure
            /// </summary>
            private struct DDLXAsset
            {
                public long NamePointer { get; set; }
                public long DataPointer { get; set; }
            }

 //           struct ddlstructmember
 //           {
 //               public:
 //   const char* name; //0x0-0x8
 //               int index; //0x8-0xC
 //               private:
 //   int pad; //0xC-0x10
 //               public:
	//const char* parentName; //0x10-0x18
 //               int size; // 32= int, 1=bool etc. //0x18-0x1C data size total size of data
 //               int elementSize; //0x1C-0x20 size of nested elements
 //               int offset; //0x20-0x24
 //               int type; //0x24-0x28 10 = pad 3=int // this is strange. it seems to be something like a "category" more than a type
 //               int nestedStruct;
 //               char unknown_data[8]; //0x28-0x34
 //               int nestedItemCount; // number of nested elements (for arrays)
 //               char unknown_data1[8];
 //           };

 //           struct ddlcontext
 //           {
 //               int hash;
 //               int offset;
 //           };

 //           struct ddlstruct
 //           {
 //               const char* name; //0x0-0x8
 //               int size; //0x8-0xC
 //               int memberCount; //0xC-0x10
 //               ddlstructmember* members; //0x10-0x18
 //               ddlcontext* context; //0x18-0x20
 //               int contextSize;
 //               int maxSize;
 //               int* hashMap;
 //               int hashCount;
 //               int maxHashes;
 //           };

 //           struct ddlenumitem
 //           {

 //               const char* name;
 //           };

 //           struct ddlindex
 //           {
 //               unsigned int hash;
 //               int index;
 //           };

 //           struct ddlenum
 //           {
 //               const char* name;
 //               int itemsCount;
 //               int unk;
 //               ddlenumitem* items;
 //               ddlindex* indexes;
 //               int indexCount;
 //               int maxHashes;
 //           };

 //           struct ddldata
 //           {
 //               const char* name; //0x0-0x8
 //               int unk;
 //               int hash; //0xC-0x10
 //               char unknown_data[8];//0x10-0x18
 //               __int64 version; //0x18-0x20
 //               int unk1; // 0x20-0x24
 //               int size; //0x24-0x28
 //               ddlstruct* structures; //0x28-0x30
 //               int structurecount; //0x30-0x34
 //               int unk2; //0x34-0x38
 //               ddlenum* enums; //0x30-0x38
 //               int enumcount; //0x38-0x3C
 //               int unknown_data1; //0x3C-0x40
 //           };

            public override string Name => "Data Definition";
            public override int Index => (int)XAssetPool.ddl;
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
                    var header = instance.Reader.ReadStruct<DDLXAsset>(StartAddress + (i * XAssetSize));

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
                        Information = $"Size: 0x0"
                    });
                }

                return results;
            }

            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                var header = instance.Reader.ReadStruct<DDLXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportFolder, xasset.Name);

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                //byte[] buffer = instance.Reader.ReadBytes(header.RawDataPtr, (int)header.RawDataPtr.AssetSize);

                //File.WriteAllBytes(path, buffer);

                File.WriteAllText(path, null);

                Console.WriteLine($"Exported {Name} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}