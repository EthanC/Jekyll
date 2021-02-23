using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Black Ops
    /// Aliases: T5
    /// </summary>
    public partial class BlackOps : IGame
    {
        /// <summary>
        /// Gets the name of Black Ops.
        /// </summary>
        public string Name => "Black Ops";

        /// <summary>
        /// Gets the process names of Black Ops.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "BlackOps",
            "BlackOpsMP"
        };

        /// <summary>
        /// Gets or sets the process index of Black Ops.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Black Ops.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pools address of Black Ops.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pool Sizes address of Black Ops.
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Black Ops.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of Black Ops.
        /// </summary>
        private enum XAssetType : int
        {
            ASSET_TYPE_XMODELPIECES,
            ASSET_TYPE_PHYSPRESET,
            ASSET_TYPE_PHYSCONSTRAINTS,
            ASSET_TYPE_DESTRUCTIBLEDEF,
            ASSET_TYPE_XANIMPARTS,
            ASSET_TYPE_XMODEL,
            ASSET_TYPE_MATERIAL,
            ASSET_TYPE_TECHNIQUE_SET,
            ASSET_TYPE_IMAGE,
            ASSET_TYPE_SOUND,
            ASSET_TYPE_SOUND_PATCH,
            ASSET_TYPE_CLIPMAP,
            ASSET_TYPE_CLIPMAP_PVS,
            ASSET_TYPE_COMWORLD,
            ASSET_TYPE_GAMEWORLD_SP,
            ASSET_TYPE_GAMEWORLD_MP,
            ASSET_TYPE_MAP_ENTS,
            ASSET_TYPE_GFXWORLD,
            ASSET_TYPE_LIGHT_DEF,
            ASSET_TYPE_UI_MAP,
            ASSET_TYPE_FONT,
            ASSET_TYPE_MENULIST,
            ASSET_TYPE_MENU,
            ASSET_TYPE_LOCALIZE_ENTRY,
            ASSET_TYPE_WEAPON,
            ASSET_TYPE_WEAPONDEF,
            ASSET_TYPE_WEAPON_VARIANT,
            ASSET_TYPE_SNDDRIVER_GLOBALS,
            ASSET_TYPE_FX,
            ASSET_TYPE_IMPACT_FX,
            ASSET_TYPE_AITYPE,
            ASSET_TYPE_MPTYPE,
            ASSET_TYPE_MPBODY,
            ASSET_TYPE_MPHEAD,
            ASSET_TYPE_CHARACTER,
            ASSET_TYPE_XMODELALIAS,
            ASSET_TYPE_RAWFILE,
            ASSET_TYPE_STRINGTABLE,
            ASSET_TYPE_PACK_INDEX,
            ASSET_TYPE_XGLOBALS,
            ASSET_TYPE_DDL,
            ASSET_TYPE_GLASSES,
            ASSET_TYPE_EMBLEMSET,
            ASSET_TYPE_COUNT,
            ASSET_TYPE_STRING,
            ASSET_TYPE_ASSETLIST
        }

        /// <summary>
        /// Structure of an Black Ops XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public uint Entries { get; set; }
        }

        /// <summary>
        /// Structure of a Black Ops XAsset Pool Size.
        /// </summary>
        public struct DBAssetPoolSize
        {
            public uint PoolSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the DBAssetPools and DBAssetPoolSizes addresses of Black Ops.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if addresses are valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();
            long moduleSize = instance.Reader.GetModuleMemorySize();

            long[] scanDBAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x56, 0x51, 0xFF, 0xD2, 0x8B, 0xF0, 0x83, 0xC4, 0x04, 0x85, 0xF6 },
                BaseAddress,
                BaseAddress + moduleSize,
                true);

            if (scanDBAssetPools.Length > 0)
            {
                DBAssetPools = instance.Reader.ReadInt32(scanDBAssetPools[0] - 0xB);
                DBAssetPoolSizes = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0x26);

                // In Black Ops, void will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "void")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of Black Ops.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Name of the XModel.</returns>
        public string GetFirstXModel(JekyllInstance instance)
        {
            long address = DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * (int)XAssetType.ASSET_TYPE_XMODEL);
            long pool = instance.Reader.ReadInt32(address) + Marshal.SizeOf<DBAssetPool>();
            long name = instance.Reader.ReadInt32(pool);

            return instance.Reader.ReadNullTerminatedString(name);
        }

        /// <summary>
        /// Creates a shallow copy of the Black Ops IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}