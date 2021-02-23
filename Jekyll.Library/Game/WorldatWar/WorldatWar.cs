using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: World at War
    /// Aliases: T4
    /// </summary>
    public partial class WorldatWar : IGame
    {
        /// <summary>
        /// Gets the name of World at War.
        /// </summary>
        public string Name => "World at War";

        /// <summary>
        /// Gets the process names of World at War.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "CoDWaW",
            "CoDWaWmp"
        };

        /// <summary>
        /// Gets or sets the process index of World at War.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of World at War.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pools address of World at War.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pool Sizes address of World at War.
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of World at War.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of World at War.
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
            ASSET_TYPE_LOADED_SOUND,
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
            ASSET_TYPE_SNDDRIVER_GLOBALS,
            ASSET_TYPE_FX,
            ASSET_TYPE_IMPACT_FX,
            ASSET_TYPE_AITYPE,
            ASSET_TYPE_MPTYPE,
            ASSET_TYPE_CHARACTER,
            ASSET_TYPE_XMODELALIAS,
            ASSET_TYPE_RAWFILE,
            ASSET_TYPE_STRINGTABLE,
            ASSET_TYPE_PACK_INDEX,
            ASSET_TYPE_COUNT,
            ASSET_TYPE_STRING,
            ASSET_TYPE_ASSETLIST
        }

        /// <summary>
        /// Structure of a World at War XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public uint Entries { get; set; }
        }

        /// <summary>
        /// Structure of a World at War XAsset Pool Size.
        /// </summary>
        public struct DBAssetPoolSize
        {
            public uint PoolSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the DBAssetPools and DBAssetPoolSizes addresses of World at War.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if addresses are valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();
            long moduleSize = instance.Reader.GetModuleMemorySize();

            long[] scanDBAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0xFF, 0xD2, 0x8B, 0xF0, 0x83, 0xC4, 0x04, 0x85, 0xF6, 0x75 },
                BaseAddress,
                BaseAddress + moduleSize,
                true);

            if (scanDBAssetPools.Length > 0)
            {
                DBAssetPools = instance.Reader.ReadInt32(scanDBAssetPools[0] - 0xD);
                DBAssetPoolSizes = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0x25);

                // In World at War, void, defaultactor, or defaultweapon will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "void" || GetFirstXModel(instance) == "defaultactor" || GetFirstXModel(instance) == "defaultweapon")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of World at War.
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
        /// Creates a shallow copy of the World at War IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}