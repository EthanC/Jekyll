using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare
    {
        public class Weapon : IXAssetPool
        {
            /// <summary>
            /// Weapon XAsset Structure
            /// </summary>
            private struct WeaponXAsset
            {
                public long NamePointer { get; set; }
                public long Unk { get; set; }
                public long DisplayNamePointer { get; set; }
                public long VariantsFileNamePointer { get; set; }
                public long UnkPtr;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
                public WeaponAttachmentType[] WeaponAttachmentTypes;
                public int UnkInt1 { get; set; }
                public int UnkInt2 { get; set; }
                public long UnkPtr1 { get; set; }
                public long UnkPtr2 { get; set; }
                public long UnkNull { get; set; }
                public long WeaponIconImagePtr { get; set; }
                public long UnkNull2 { get; set; }
                public long WeaponIconImagePtr2 { get; set; }
            }

            private struct WeaponAttachmentType
            {
                public long AttachmentCount { get; set; }
                public long AttachmentPointer { get; set; }
            }

            private struct AttachmentXAsset
            {
                public long NamePointer;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
                public long[] UnkPadding;
                public long WorldModelPointer { get; set; }
                public long ViewModelPointer { get; set; }
            }

            public override string Name => "Weapon";
            public override int Index => (int)XAssetPool.weapon;
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
                    var header = instance.Reader.ReadStruct<WeaponXAsset>(StartAddress + (i * XAssetSize));

                    if (IsNullXAsset(header.NamePointer))
                    {
                        continue;
                    }
                    
                    var RawData = instance.Reader.ReadBytes(StartAddress + (i * XAssetSize), (int)XAssetSize);
                    string exportName = Path.Combine(instance.ExportFolder, "weapon", instance.Reader.ReadNullTerminatedString(header.NamePointer));

                    Directory.CreateDirectory(Path.GetDirectoryName(exportName));
                    File.WriteAllBytes(exportName, RawData);

                    // Console.WriteLine($"Exported {Name} {exportName.Split("\\")[^1]}");

                    // Testing
                    //if (instance.Reader.ReadNullTerminatedString(header.NamePointer) == "iw8_sm_augolf_mp")
                    //{
                    //    long pos = StartAddress + (i * XAssetSize);

                    //    for (int j = 0; j < XAssetSize; j += 8)
                    //    {
                    //        Console.WriteLine(instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64((pos + j))) + 8));
                    //    }

                    //    var raw = instance.Reader.ReadBytes(instance.Reader.ReadInt64(header.WeaponAttachmentTypes[1].AttachmentPointer), (int)968);

                    //    string name = Path.Combine("iw8_weapon", instance.Reader.ReadNullTerminatedString(header.NamePointer) + "att");
                    //    Directory.CreateDirectory(Path.GetDirectoryName(name));
                    //    File.WriteAllBytes(name, raw);

                    //    long position = instance.Reader.ReadInt64(header.WeaponAttachmentTypes[1].AttachmentPointer);

                    //    for (int j = 0; j < 968; j += 8)
                    //    {
                    //        Console.WriteLine(instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(pos + j)));
                    //    }

                    //    var Att = instance.Reader.ReadStruct<AttachmentXAsset>(instance.Reader.ReadInt64(header.WeaponAttachmentTypes[1].AttachmentPointer));

                    //    Console.WriteLine(instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64(Att.WorldModelPointer))));

                    //    for (int d = 0; d < header.WeaponAttachmentTypes[1].AttachmentCount; d++)
                    //    {
                    //        Console.WriteLine(instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64((header.WeaponAttachmentTypes[1].AttachmentPointer + d * 8)))));
                    //    }
                    //}

                    var result = new StringBuilder();
                    result.AppendLine($"Weapon: {instance.Reader.ReadNullTerminatedString(header.NamePointer)}");
                    result.AppendLine("Attachments:");

                    foreach (var AttackmentType in header.WeaponAttachmentTypes)
                    {
                        for (int d = 0; d < AttackmentType.AttachmentCount; d++)
                        {
                            var attachmentAsset = instance.Reader.ReadStruct<AttachmentXAsset>(instance.Reader.ReadInt64(AttackmentType.AttachmentPointer + d * 8));

                            result.AppendLine($"Attachment: {instance.Reader.ReadNullTerminatedString(attachmentAsset.NamePointer)}");

                            string worldModel = instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64(attachmentAsset.WorldModelPointer)));
                            string viewModel = instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64(attachmentAsset.ViewModelPointer)));

                            if (worldModel != "")
                            {
                                result.AppendLine($"World Model: {worldModel}");
                            }

                            if (viewModel != "")
                            {
                                result.AppendLine($"View Model: {viewModel}");
                            }

                            result.AppendLine();
                        }
                    }

                    string path = Path.Combine(instance.ExportFolder, "weapon", instance.Reader.ReadNullTerminatedString(header.NamePointer) + ".txt");
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    File.WriteAllText(path, result.ToString());

                    results.Add(new GameXAsset()
                    {
                        Name = instance.Reader.ReadNullTerminatedString(header.NamePointer),
                        HeaderAddress = StartAddress + (i * XAssetSize),
                        XAssetPool = this,
                        Type = Name,
                        Information = "Size: 0x5"
                    });
                }

                return results;
            }

            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                var header = instance.Reader.ReadStruct<WeaponXAsset>(xasset.HeaderAddress);

                if (xasset.Name != instance.Reader.ReadNullTerminatedString(header.NamePointer))
                {
                    return JekyllStatus.MemoryChanged;
                }

                string path = Path.Combine(instance.ExportFolder, xasset.Name);

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                Console.WriteLine($"Exported {Name} {xasset.Name}");

                return JekyllStatus.Success;
            }
        }
    }
}