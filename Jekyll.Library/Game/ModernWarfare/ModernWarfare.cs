using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Modern Warfare
    /// Aliases: IW8, Kronos, Odin
    /// </summary>
    public partial class ModernWarfare : IGame
    {
        /// <summary>
        /// Gets the name of Modern Warfare.
        /// </summary>
        public string Name => "Modern Warfare";

        /// <summary>
        /// Gets the process names of Modern Warfare.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "ModernWarfare"
        };

        /// <summary>
        /// Gets or sets the process index of Modern Warfare.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Modern Warfare.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the DBAssetPools address of Modern Warfare.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the DBAssetPoolSizes address of Modern Warfare.
        /// Not used for this title, instead, it is stored in DBAssetPool.
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Modern Warfare.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of Modern Warfare.
        /// </summary>
        private enum XAssetType : int
        {
            ASSET_TYPE_PHYSICSLIBRARY,
            ASSET_TYPE_PHYSICS_SFX_EVENT_ASSET,
            ASSET_TYPE_PHYSICS_VFX_EVENT_ASSET,
            ASSET_TYPE_PHYSICSASSET,
            ASSET_TYPE_PHYSICS_FX_PIPELINE,
            ASSET_TYPE_PHYSICS_FX_SHAPE,
            ASSET_TYPE_PHYSICS_DEBUG_DATA,
            ASSET_TYPE_XANIMPARTS,
            ASSET_TYPE_XMODEL_SURFS,
            ASSET_TYPE_XMODEL,
            ASSET_TYPE_MAYHEM,
            ASSET_TYPE_MATERIAL,
            ASSET_TYPE_COMPUTESHADER,
            ASSET_TYPE_LIBSHADER,
            ASSET_TYPE_VERTEXSHADER,
            ASSET_TYPE_HULLSHADER,
            ASSET_TYPE_DOMAINSHADER,
            ASSET_TYPE_PIXELSHADER,
            ASSET_TYPE_TECHNIQUE_SET,
            ASSET_TYPE_IMAGE,
            ASSET_TYPE_SOUND_GLOBALS,
            ASSET_TYPE_SOUND_BANK,
            ASSET_TYPE_SOUND_BANK_TRANSIENT,
            ASSET_TYPE_CLIPMAP,
            ASSET_TYPE_COMWORLD,
            ASSET_TYPE_GLASSWORLD,
            ASSET_TYPE_PATHDATA,
            ASSET_TYPE_NAVMESH,
            ASSET_TYPE_TACGRAPH,
            ASSET_TYPE_MAP_ENTS,
            ASSET_TYPE_FXWORLD,
            ASSET_TYPE_GFXWORLD,
            ASSET_TYPE_GFXWORLD_TRANSIENT_ZONE,
            ASSET_TYPE_IESPROFILE,
            ASSET_TYPE_LIGHT_DEF,
            ASSET_TYPE_GRADING_CLUT,
            ASSET_TYPE_UI_MAP,
            ASSET_TYPE_FOG_SPLINE,
            ASSET_TYPE_ANIMCLASS,
            ASSET_TYPE_PLAYERANIM,
            ASSET_TYPE_GESTURE,
            ASSET_TYPE_LOCALIZE_ENTRY,
            ASSET_TYPE_ATTACHMENT,
            ASSET_TYPE_WEAPON,
            ASSET_TYPE_VFX,
            ASSET_TYPE_IMPACT_FX,
            ASSET_TYPE_SURFACE_FX,
            ASSET_TYPE_AITYPE,
            ASSET_TYPE_MPTYPE,
            ASSET_TYPE_CHARACTER,
            ASSET_TYPE_XMODELALIAS,
            ASSET_TYPE_RAWFILE,
            ASSET_TYPE_SCRIPTFILE,
            ASSET_TYPE_SCRIPT_DEBUG_DATA,
            ASSET_TYPE_STRINGTABLE,
            ASSET_TYPE_LEADERBOARD,
            ASSET_TYPE_VIRTUAL_LEADERBOARD,
            ASSET_TYPE_DDL,
            ASSET_TYPE_TRACER,
            ASSET_TYPE_VEHICLE,
            ASSET_TYPE_ADDON_MAP_ENTS,
            ASSET_TYPE_NET_CONST_STRINGS,
            ASSET_TYPE_LUA_FILE,
            ASSET_TYPE_SCRIPTABLE,
            ASSET_TYPE_EQUIPMENT_SND_TABLE,
            ASSET_TYPE_VECTORFIELD,
            ASSET_TYPE_PARTICLE_SIM_ANIMATION,
            ASSET_TYPE_STREAMING_INFO,
            ASSET_TYPE_LASER,
            ASSET_TYPE_TTF,
            ASSET_TYPE_SUIT,
            ASSET_TYPE_SUITANIMPACKAGE,
            ASSET_TYPE_CAMERA,
            ASSET_TYPE_HUDOUTLINE,
            ASSET_TYPE_SPACESHIPTARGET,
            ASSET_TYPE_RUMBLE,
            ASSET_TYPE_RUMBLE_GRAPH,
            ASSET_TYPE_ANIM_PACKAGE,
            ASSET_TYPE_SFX_PACKAGE,
            ASSET_TYPE_VFX_PACKAGE,
            ASSET_TYPE_FOOTSTEP_VFX,
            ASSET_TYPE_BEHAVIOR_TREE,
            ASSET_TYPE_ANIMSET,
            ASSET_TYPE_ASM,
            ASSET_TYPE_XANIM_PROCEDURALBONES,
            ASSET_TYPE_XANIM_DYNAMICBONES,
            ASSET_TYPE_RETICLE,
            ASSET_TYPE_XANIMCURVE,
            ASSET_TYPE_COVERSELECTOR,
            ASSET_TYPE_ENEMYSELECTOR,
            ASSET_TYPE_CLIENTCHARACTER,
            ASSET_TYPE_CLOTHASSET,
            ASSET_TYPE_CINEMATICMOTION,
            ASSET_TYPE_ACCESSORY,
            ASSET_TYPE_LOCDMGTABLE,
            ASSET_TYPE_BULLETPENETRATION,
            ASSET_TYPE_SCRIPTBUNDLE,
            ASSET_TYPE_BLENDSPACE2D,
            ASSET_TYPE_XCAM,
            ASSET_TYPE_CAMO,
            ASSET_TYPE_XCOMPOSITEMODEL,
            ASSET_TYPE_XMODEL_DETAIL_COLLISION,
            ASSET_TYPE_STREAM_KEY,
            ASSET_TYPE_STREAM_TREE_OVERRIDE,
            ASSET_TYPE_KEYVALUEPAIRS,
            ASSET_TYPE_SUPER_TERRAIN,
            ASSET_TYPE_NATIVE_SCRIPT_PATCH,
            ASSET_TYPE_COLLISION_TILE,
            ASSET_TYPE_EXECUTION,
            ASSET_TYPE_CARRYOBJECT,
            ASSET_TYPE_SOUNDBANKLIST,
            ASSET_TYPE_DECAL_VOLUME_MATERIAL,
            ASSET_TYPE_DECAL_VOLUME_MASK,
            ASSET_TYPE_DYNENTITY_LIST,
            ASSET_TYPE_FXWORLD_TRANSIENT_ZONE,
            ASSET_TYPE_DLOG_SCHEMA,
            ASSET_TYPE_EDGE_LIST,
            ASSET_TYPE_COUNT,
            ASSET_TYPE_STRING,
            ASSET_TYPE_ASSETLIST
        }

        /// <summary>
        /// Structure of a Modern Warfare XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public long Entries { get; set; }
            public long FreeHead { get; set; }
            public uint PoolSize { get; set; }
            public uint ElementSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the DBAssetPools address of Modern Warfare.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if address is valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();

            var scanDBAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x48, 0x8D, 0x04, 0x40, 0x4C, 0x8D, 0x8E, null, null, null, null, 0x4D, 0x8D, 0x0C, 0xC1, 0x8D, 0x42, 0xFF },
                BaseAddress,
                BaseAddress + instance.Reader.GetModuleMemorySize(),
                true);

            if (scanDBAssetPools.Length > 0)
            {
                DBAssetPools = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0x7);

                // In Modern Warfare, axis_guide_createfx will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "axis_guide_createfx")
                {
                    List<Dictionary<string, object>> pools = new List<Dictionary<string, object>>();

                    foreach (int index in Enum.GetValues(typeof(XAssetType)))
                    {
                        DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(instance.Game.BaseAddress + instance.Game.DBAssetPools + (index * Marshal.SizeOf<DBAssetPool>()));

                        pools.Add(new Dictionary<string, object>() {
                            { "Name", Enum.GetName(typeof(XAssetType), index) },
                            { "ElementSize", pool.ElementSize },
                        });
                    }

                    string path = Path.Combine(instance.ExportPath, "DBAssetPools.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (StreamWriter file = File.CreateText(path))
                    {
                        file.Write(JsonConvert.SerializeObject(pools, Formatting.Indented));
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of Modern Warfare.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Name of the XModel.</returns>
        public string GetFirstXModel(JekyllInstance instance)
        {
            long address = BaseAddress + DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * (int)XAssetType.ASSET_TYPE_XMODEL);
            DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(address);
            long name = instance.Reader.ReadInt64(pool.Entries);

            return instance.Reader.ReadNullTerminatedString(name);
        }

        /// <summary>
        /// Creates a shallow copy of the Modern Warfare IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}