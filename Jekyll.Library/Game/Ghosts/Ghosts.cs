using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Ghosts
    /// Aliases: IW6
    /// </summary>
    public partial class Ghosts : IGame
    {
        /// <summary>
        /// Gets the name of Ghosts.
        /// </summary>
        public string Name => "Ghosts";

        /// <summary>
        /// Gets the process names of Ghosts.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "iw6sp64_ship",
            "iw6mp64_ship"
        };

        /// <summary>
        /// Gets or sets the process index of Ghosts.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Ghosts.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the DBAssetPools address of Ghosts.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the DBAssetPoolSizes address of Ghosts.
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Ghosts.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of Ghosts.
        /// </summary>
        private enum XAssetType : int
        {
            ASSET_TYPE_PHYSPRESET,
            ASSET_TYPE_PHYSCOLLMAP,
            ASSET_TYPE_XANIMPARTS,
            ASSET_TYPE_XMODEL_SURFS,
            ASSET_TYPE_XMODEL,
            ASSET_TYPE_MATERIAL,
            ASSET_TYPE_COMPUTESHADER,
            ASSET_TYPE_VERTEXSHADER,
            ASSET_TYPE_HULLSHADER,
            ASSET_TYPE_DOMAINSHADER,
            ASSET_TYPE_PIXELSHADER,
            ASSET_TYPE_VERTEXDECL,
            ASSET_TYPE_TECHNIQUE_SET,
            ASSET_TYPE_IMAGE,
            ASSET_TYPE_SOUND,
            ASSET_TYPE_SOUND_CURVE,
            ASSET_TYPE_LPF_CURVE,
            ASSET_TYPE_REVERB_CURVE,
            ASSET_TYPE_LOADED_SOUND,
            ASSET_TYPE_CLIPMAP,
            ASSET_TYPE_COMWORLD,
            ASSET_TYPE_GLASSWORLD,
            ASSET_TYPE_PATHDATA,
            ASSET_TYPE_VEHICLE_TRACK,
            ASSET_TYPE_MAP_ENTS,
            ASSET_TYPE_FXWORLD,
            ASSET_TYPE_GFXWORLD,
            ASSET_TYPE_LIGHT_DEF,
            ASSET_TYPE_UI_MAP,
            ASSET_TYPE_FONT,
            ASSET_TYPE_MENULIST,
            ASSET_TYPE_MENU,
            ASSET_TYPE_ANIMCLASS,
            ASSET_TYPE_LOCALIZE_ENTRY,
            ASSET_TYPE_ATTACHMENT,
            ASSET_TYPE_WEAPON,
            ASSET_TYPE_SNDDRIVER_GLOBALS,
            ASSET_TYPE_FX,
            ASSET_TYPE_IMPACT_FX,
            ASSET_TYPE_SURFACE_FX,
            ASSET_TYPE_AITYPE,
            ASSET_TYPE_MPTYPE,
            ASSET_TYPE_CHARACTER,
            ASSET_TYPE_XMODELALIAS,
            ASSET_TYPE_RAWFILE,
            ASSET_TYPE_SCRIPTFILE,
            ASSET_TYPE_STRINGTABLE,
            ASSET_TYPE_LEADERBOARD,
            ASSET_TYPE_STRUCTURED_DATA_DEF,
            ASSET_TYPE_TRACER,
            ASSET_TYPE_VEHICLE,
            ASSET_TYPE_ADDON_MAP_ENTS,
            ASSET_TYPE_NET_CONST_STRINGS,
            ASSET_TYPE_REVERB_PRESET,
            ASSET_TYPE_LUA_FILE,
            ASSET_TYPE_SCRIPTABLE,
            ASSET_TYPE_COLORIZATION,
            ASSET_TYPE_COLORIZATIONSET,
            ASSET_TYPE_TONEMAPPING,
            ASSET_TYPE_EQUIPMENT_SND_TABLE,
            ASSET_TYPE_VECTORFIELD,
            ASSET_TYPE_DOPPLER_PRESET,
            ASSET_TYPE_PARTICLE_SIM_ANIMATION,
            ASSET_TYPE_COUNT,
            ASSET_TYPE_STRING,
            ASSET_TYPE_ASSETLIST
        }

        /// <summary>
        /// Structure of a Ghosts XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public uint FreeHead { get; set; }
            public uint Entries { get; set; }
        }

        /// <summary>
        /// Structure of a Ghosts XAsset Pool Size.
        /// </summary>
        public struct DBAssetPoolSize
        {
            public uint PoolSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the DBAssetPools and DBAssetPoolSizes addresses of Ghosts.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if addresses are valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();
            long moduleSize = instance.Reader.GetModuleMemorySize();

            long[] scanDBAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x48, 0x8B, 0xD8, 0x48, 0x85, 0xC0, 0x75, null, 0xF0, 0xFF, 0x0D },
                BaseAddress,
                BaseAddress + moduleSize,
                true);

            if (scanDBAssetPools.Length > 0)
            {
                DBAssetPools = instance.Reader.ReadUInt32(scanDBAssetPools[0] - 0xB) + BaseAddress;

                if (instance.Reader.ActiveProcess.ProcessName == "iw6sp64_ship")
                {
                    DBAssetPoolSizes = instance.Reader.ReadUInt32(scanDBAssetPools[0] + 0x26) + BaseAddress;
                }
                else if (instance.Reader.ActiveProcess.ProcessName == "iw6mp64_ship")
                {
                    DBAssetPoolSizes = instance.Reader.ReadUInt32(scanDBAssetPools[0] + 0x1E) + BaseAddress;
                }

                // In Ghosts, void or empty_model will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "void" || GetFirstXModel(instance) == "empty_model")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of Ghosts.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Name of the XModel.</returns>
        public string GetFirstXModel(JekyllInstance instance)
        {
            long address = DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * (int)XAssetType.ASSET_TYPE_XMODEL);
            long pool = instance.Reader.ReadInt64(address) + Marshal.SizeOf<DBAssetPool>();
            long name = instance.Reader.ReadInt64(pool);

            return instance.Reader.ReadNullTerminatedString(name);
        }

        /// <summary>
        /// Creates a shallow copy of the Ghosts IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}