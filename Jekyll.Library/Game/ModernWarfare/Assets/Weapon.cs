using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class Weapon : IAssetPool
        {
            #region AssetStructures
            /// <summary>
            /// TTF Asset Structure
            /// </summary>
            private struct WeaponAsset
            {
                public long NamePointer;
                public long Unk;
                public long DisplayNamePointer;
                public long VariantsFileNamePointer;
                public long UnkPtr;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
                public WeaponAttachmentType[] WeaponAttachmentTypes;
                public int UnkInt1;
                public int UnkInt2;
                public long UnkPtr1;
                public long UnkPtr2;
                public long UnkNull;
                public long WeaponIconImagePtr;
                public long UnkNull2;
                public long WeaponIconImagePtr2;
            }

            private struct WeaponAttachmentType
            {
                public long AttachmentCount;
                public long AttachmentPointer;
            }

            private struct AttachmentAsset
            {
                public long NamePointer;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
                public long[] UnkPadding;
                public long WorldModelPointer;
                public long ViewModelPointer;
            }
            #endregion

            public override string Name => "Weapon";
            public override int Index => (int)AssetPool.weapon;
            public override long EndAddress { get { return StartAddress + (AssetCount * AssetSize); } set => throw new NotImplementedException(); }

            public override List<GameAsset> Load(JekyllInstance instance)
            {
                // Incomplete
                return new List<GameAsset>();
            }
            public List<GameAsset> Load(JekyllInstance instance, int i)
            {
                var results = new List<GameAsset>();

                var poolInfo = instance.Reader.ReadStruct<AssetPoolInfo>(instance.Game.BaseAddress + instance.Game.AssetPoolsAddresses[instance.Game.ProcessIndex] + (Index * 24));

                StartAddress = poolInfo.PoolPtr;
                AssetSize = poolInfo.AssetSize;
                AssetCount = poolInfo.PoolSize;

                for (i = 0; i < AssetCount; i++)
                {
                    var header = instance.Reader.ReadStruct<WeaponAsset>(StartAddress + (i * AssetSize));

                    if (IsNullAsset(header.NamePointer))
                        continue;

                    /*
                    var RawData = instance.Reader.ReadBytes(StartAddress + (i * AssetSize), (int)AssetSize);
                    string exportName = Path.Combine("iw8_weapon", instance.Reader.ReadNullTerminatedString(header.NamePointer));
                    Directory.CreateDirectory(Path.GetDirectoryName(exportName));
                    File.WriteAllBytes(exportName, RawData);
                    */

                    if (instance.Reader.ReadNullTerminatedString(header.NamePointer) == "iw8_ar_akilo47_mp")
                    {
                        /*
                        long pos = StartAddress + (i * AssetSize);
                        for (int j = 0; j < AssetSize; j += 8)
                        {
                            Console.WriteLine(instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64((pos + j))) + 8));
                        }
                        */

                        var RawData = instance.Reader.ReadBytes(instance.Reader.ReadInt64(header.WeaponAttachmentTypes[1].AttachmentPointer), (int)968);
                        string exportName = Path.Combine("iw8_weapon", instance.Reader.ReadNullTerminatedString(header.NamePointer) + "att");
                        Directory.CreateDirectory(Path.GetDirectoryName(exportName));
                        File.WriteAllBytes(exportName, RawData);
                        long pos = instance.Reader.ReadInt64(header.WeaponAttachmentTypes[1].AttachmentPointer);
                        for (int j = 0; j < 968; j += 8)
                        {
                            Console.WriteLine(instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(pos + j)));
                        }

                        var Att = instance.Reader.ReadStruct<AttachmentAsset>(
                            instance.Reader.ReadInt64(header.WeaponAttachmentTypes[1].AttachmentPointer));
                        Console.WriteLine(instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64(Att.WorldModelPointer))));

                        /*
                        for (int d = 0; d < header.WeaponAttachmentTypes[1].AttachmentCount; d++)
                        {
                            Console.WriteLine(instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64((header.WeaponAttachmentTypes[1].AttachmentPointer + d * 8)))));
                        }
                        */
                    }

                    var result = new StringBuilder();
                    result.AppendLine("Weapon: " + instance.Reader.ReadNullTerminatedString(header.NamePointer));

                    result.AppendLine("\nAttachments:");
                    foreach (var AttackmentType in header.WeaponAttachmentTypes)
                    {
                        for (int d = 0; d < AttackmentType.AttachmentCount; d++)
                        {
                            var attachmentAsset = instance.Reader.ReadStruct<AttachmentAsset>(instance.Reader.ReadInt64(AttackmentType.AttachmentPointer + d * 8));
                            result.AppendLine("Attachment: " +
                                              instance.Reader.ReadNullTerminatedString(attachmentAsset.NamePointer));
                            string worldModel = instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64(attachmentAsset.WorldModelPointer)));
                            string viewModel = instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64(attachmentAsset.ViewModelPointer)));
                            if (worldModel != "")
                                result.AppendLine("Worldmodel: " + worldModel);
                            if (viewModel != "")
                                result.AppendLine("Viewmodel: " + viewModel);
                            result.AppendLine();
                        }
                    }
                    string path = Path.Combine("export", instance.Game.Name, "weapon", instance.Reader.ReadNullTerminatedString(header.NamePointer) + ".txt");
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    File.WriteAllText(path, result.ToString());

                    results.Add(new GameAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        HeaderAddress = StartAddress + (i * AssetSize),
                        AssetPool = this,
                        Type = Name,
                        Information = string.Format("Size: 0x{0:X}", 5)
                    });
                }

                return results;
            }

            public override JekyllStatus Export(GameAsset asset, JekyllInstance instance)
            {
                var header = instance.Reader.ReadStruct<WeaponAsset>(asset.HeaderAddress);

                if (asset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                    return JekyllStatus.MemoryChanged;

                string path = Path.Combine("export", instance.Game.Name, asset.Name);

                // Create path
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                Console.WriteLine($"Exported {Name} {asset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}