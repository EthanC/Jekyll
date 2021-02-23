using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Modern Warfare 2
    /// Aliases: IW4
    /// </summary>
    public partial class ModernWarfare2 : IGame
    {
        /// <summary>
        /// Gets the name of Modern Warfare 2.
        /// </summary>
        public string Name => "Modern Warfare 2";

        /// <summary>
        /// Gets the process names of Modern Warfare 2.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "iw4sp",
            "iw4mp"
        };

        /// <summary>
        /// Gets or sets the process index of Modern Warfare 2.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Modern Warfare 2.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pools address of Modern Warfare 2.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pool Sizes address of Modern Warfare 2.
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Modern Warfare 2.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of Modern Warfare 2.
        /// </summary>
        private enum XAssetType : int
        {
            ASSET_TYPE_PHYSPRESET,
            ASSET_TYPE_PHYSCOLLMAP,
            ASSET_TYPE_XANIMPARTS,
            ASSET_TYPE_XMODEL_SURFS,
            ASSET_TYPE_XMODEL,
            ASSET_TYPE_MATERIAL,
            ASSET_TYPE_PIXELSHADER,
            ASSET_TYPE_VERTEXSHADER,
            ASSET_TYPE_VERTEXDECL,
            ASSET_TYPE_TECHNIQUE_SET,
            ASSET_TYPE_IMAGE,
            ASSET_TYPE_SOUND,
            ASSET_TYPE_SOUND_CURVE,
            ASSET_TYPE_LOADED_SOUND,
            ASSET_TYPE_CLIPMAP_SP,
            ASSET_TYPE_CLIPMAP_MP,
            ASSET_TYPE_COMWORLD,
            ASSET_TYPE_GAMEWORLD_SP,
            ASSET_TYPE_GAMEWORLD_MP,
            ASSET_TYPE_MAP_ENTS,
            ASSET_TYPE_FXWORLD,
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
            ASSET_TYPE_LEADERBOARD,
            ASSET_TYPE_STRUCTURED_DATA_DEF,
            ASSET_TYPE_TRACER,
            ASSET_TYPE_VEHICLE,
            ASSET_TYPE_ADDON_MAP_ENTS,
            ASSET_TYPE_COUNT,
            ASSET_TYPE_STRING,
            ASSET_TYPE_ASSETLIST
        }

        /// <summary>
        /// Structure of a Modern Warfare 2 XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public uint Entries { get; set; }
        }

        /// <summary>
        /// Structure of a Modern Warfare 2 XAsset Pool Size.
        /// </summary>
        public struct DBAssetPoolSize
        {
            public uint PoolSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the DBAssetPools and DBAssetPoolSizes addresses of Modern Warfare 2.
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
                DBAssetPools = instance.Reader.ReadUInt32(scanDBAssetPools[0] - 0xB);

                if (instance.Reader.ActiveProcess.ProcessName == "iw4sp")
                {
                    DBAssetPoolSizes = instance.Reader.ReadUInt32(scanDBAssetPools[0] + 0x30);
                }
                else if (instance.Reader.ActiveProcess.ProcessName == "iw4mp")
                {
                    DBAssetPoolSizes = instance.Reader.ReadUInt32(scanDBAssetPools[0] + 0x46);
                }

                // In Modern Warfare 2, void will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "void")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of Modern Warfare 2.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Name of the XModel.</returns>
        public string GetFirstXModel(JekyllInstance instance)
        {
            long address = DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * (int)XAssetType.ASSET_TYPE_XMODEL);
            long pool = instance.Reader.ReadUInt32(address) + Marshal.SizeOf<DBAssetPool>();
            long name = instance.Reader.ReadUInt32(pool);

            return instance.Reader.ReadNullTerminatedString(name);
        }

        /// <summary>
        /// Creates a shallow copy of the Modern Warfare 2 IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}