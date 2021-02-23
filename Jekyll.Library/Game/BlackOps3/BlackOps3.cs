using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Black Ops III
    /// Aliases: T7
    /// </summary>
    public partial class BlackOps3 : IGame
    {
        /// <summary>
        /// Gets the name of Black Ops III.
        /// </summary>
        public string Name => "Black Ops III";

        /// <summary>
        /// Gets the process names of Black Ops III.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "BlackOps3"
        };

        /// <summary>
        /// Gets or sets the process index of Black Ops III.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Black Ops III.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pools address of Black Ops III.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pool Sizes address of Black Ops III (unused, stored in pool data.)
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Black Ops III.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of Black Ops III.
        /// </summary>
        private enum XAssetType : int
        {
            ASSET_TYPE_PHYSPRESET,
            ASSET_TYPE_PHYSCONSTRAINTS,
            ASSET_TYPE_DESTRUCTIBLEDEF,
            ASSET_TYPE_XANIMPARTS,
            ASSET_TYPE_XMODEL,
            ASSET_TYPE_XMODELMESH,
            ASSET_TYPE_MATERIAL,
            ASSET_TYPE_COMPUTE_SHADER_SET,
            ASSET_TYPE_TECHNIQUE_SET,
            ASSET_TYPE_IMAGE,
            ASSET_TYPE_SOUND,
            ASSET_TYPE_SOUND_PATCH,
            ASSET_TYPE_CLIPMAP,
            ASSET_TYPE_COMWORLD,
            ASSET_TYPE_GAMEWORLD,
            ASSET_TYPE_MAP_ENTS,
            ASSET_TYPE_GFXWORLD,
            ASSET_TYPE_LIGHT_DEF,
            ASSET_TYPE_LENSFLARE_DEF,
            ASSET_TYPE_UI_MAP,
            ASSET_TYPE_FONT,
            ASSET_TYPE_FONTICON,
            ASSET_TYPE_LOCALIZE_ENTRY,
            ASSET_TYPE_WEAPON,
            ASSET_TYPE_WEAPONDEF,
            ASSET_TYPE_WEAPON_VARIANT,
            ASSET_TYPE_WEAPON_FULL,
            ASSET_TYPE_CGMEDIA,
            ASSET_TYPE_PLAYERSOUNDS,
            ASSET_TYPE_PLAYERFX,
            ASSET_TYPE_SHAREDWEAPONSOUNDS,
            ASSET_TYPE_ATTACHMENT,
            ASSET_TYPE_ATTACHMENT_UNIQUE,
            ASSET_TYPE_WEAPON_CAMO,
            ASSET_TYPE_CUSTOMIZATION_TABLE,
            ASSET_TYPE_CUSTOMIZATION_TABLE_FE_IMAGES,
            ASSET_TYPE_CUSTOMIZATION_TABLE_COLOR,
            ASSET_TYPE_SNDDRIVER_GLOBALS,
            ASSET_TYPE_FX,
            ASSET_TYPE_TAGFX,
            ASSET_TYPE_NEW_LENSFLARE_DEF,
            ASSET_TYPE_IMPACT_FX,
            ASSET_TYPE_IMPACT_SOUND,
            ASSET_TYPE_PLAYER_CHARACTER,
            ASSET_TYPE_AITYPE,
            ASSET_TYPE_CHARACTER,
            ASSET_TYPE_XMODELALIAS,
            ASSET_TYPE_RAWFILE,
            ASSET_TYPE_STRINGTABLE,
            ASSET_TYPE_STRUCTURED_TABLE,
            ASSET_TYPE_LEADERBOARD,
            ASSET_TYPE_DDL,
            ASSET_TYPE_GLASSES,
            ASSET_TYPE_TEXTURELIST,
            ASSET_TYPE_SCRIPTPARSETREE,
            ASSET_TYPE_KEYVALUEPAIRS,
            ASSET_TYPE_VEHICLEDEF,
            ASSET_TYPE_ADDON_MAP_ENTS,
            ASSET_TYPE_TRACER,
            ASSET_TYPE_SLUG,
            ASSET_TYPE_SURFACEFX_TABLE,
            ASSET_TYPE_SURFACESOUNDDEF,
            ASSET_TYPE_FOOTSTEP_TABLE,
            ASSET_TYPE_ENTITYFXIMPACTS,
            ASSET_TYPE_ENTITYSOUNDIMPACTS,
            ASSET_TYPE_ZBARRIER,
            ASSET_TYPE_VEHICLEFXDEF,
            ASSET_TYPE_VEHICLESOUNDDEF,
            ASSET_TYPE_TYPEINFO,
            ASSET_TYPE_SCRIPTBUNDLE,
            ASSET_TYPE_SCRIPTBUNDLELIST,
            ASSET_TYPE_RUMBLE,
            ASSET_TYPE_BULLETPENETRATION,
            ASSET_TYPE_LOCDMGTABLE,
            ASSET_TYPE_AIMTABLE,
            ASSET_TYPE_ANIMSELECTORTABLESET,
            ASSET_TYPE_ANIMMAPPINGTABLE,
            ASSET_TYPE_ANIMSTATEMACHINE,
            ASSET_TYPE_BEHAVIORTREE,
            ASSET_TYPE_BEHAVIORSTATEMACHINE,
            ASSET_TYPE_TTF,
            ASSET_TYPE_SANIM,
            ASSET_TYPE_LIGHT_DESCRIPTION,
            ASSET_TYPE_SHELLSHOCK,
            ASSET_TYPE_XCAM,
            ASSET_TYPE_BG_CACHE,
            ASSET_TYPE_TEXTURE_COMBO,
            ASSET_TYPE_FLAMETABLE,
            ASSET_TYPE_BITFIELD,
            ASSET_TYPE_ATTACHMENT_COSMETIC_VARIANT,
            ASSET_TYPE_MAPTABLE,
            ASSET_TYPE_MAPTABLE_LOADING_IMAGES,
            ASSET_TYPE_MEDAL,
            ASSET_TYPE_MEDALTABLE,
            ASSET_TYPE_OBJECTIVE,
            ASSET_TYPE_OBJECTIVE_LIST,
            ASSET_TYPE_UMBRA_TOME,
            ASSET_TYPE_NAVMESH,
            ASSET_TYPE_NAVVOLUME,
            ASSET_TYPE_BINARYHTML,
            ASSET_TYPE_LASER,
            ASSET_TYPE_BEAM,
            ASSET_TYPE_STREAMER_HINT,
            ASSET_TYPE_STRING,
            ASSET_TYPE_ASSETLIST,
            ASSET_TYPE_REPORT,
            ASSET_TYPE_DEPEND,
            ASSET_TYPE_FULL_COUNT
        }

        /// <summary>
        /// Structure of a Black Ops III XAsset Pool.
        /// </summary>
        public struct XAssetPool
        {
            public long Pool { get; set; }
            public uint ItemSize { get; set; }
            public int ItemCount { get; set; }
            public bool IsSingleton { get; set; }
            public int ItemAllocCount { get; set; }
            public long FreeHead { get; set; }
        }

        /// <summary>
        /// Validates and sets the XAsset Pools address of Black Ops III.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if address is valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();

            var scanDBAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x63, 0xC1, 0x48, 0x8D, 0x05, null, null, null, null, 0x49, 0xC1, 0xE0, null, 0x4C, 0x03, 0xC0 },
                BaseAddress,
                BaseAddress + instance.Reader.GetModuleMemorySize(),
                true);

            if (scanDBAssetPools.Length > 0)
            {
                DBAssetPools = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0x5) + (scanDBAssetPools[0] + 0x9);

                // In Black Ops III, void will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "void")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of Black Ops III.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Name of the XModel.</returns>
        public string GetFirstXModel(JekyllInstance instance)
        {
            long address = DBAssetPools + (Marshal.SizeOf<XAssetPool>() * (int)XAssetType.ASSET_TYPE_XMODEL);
            XAssetPool pool = instance.Reader.ReadStruct<XAssetPool>(address);
            long name = instance.Reader.ReadInt64(pool.Pool);

            return instance.Reader.ReadNullTerminatedString(name);
        }

        /// <summary>
        /// Creates a shallow copy of the Black Ops III IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}