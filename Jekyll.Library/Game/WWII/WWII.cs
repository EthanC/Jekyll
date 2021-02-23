using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: WWII
    /// Aliases: S2
    /// </summary>
    public partial class WWII : IGame
    {
        /// <summary>
        /// Gets the name of WWII.
        /// </summary>
        public string Name => "WWII";

        /// <summary>
        /// Gets the process names of WWII.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "s2_sp64_ship",
            "s2_mp64_ship"
        };

        /// <summary>
        /// Gets or sets the process index of WWII.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of WWII.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pools address of WWII.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pool Sizes address of WWII.
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of WWII.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of WWII.
        /// </summary>
        private enum XAssetType : int
        {
            physpreset,
            sndphyspreset,
            sndmusicset,
            phys_collmap,
            physwaterpreset,
            phys_worldmap,
            physconstraint,
            xanim,
            xsurfshared,
            xmodelsurfs,
            xmodel,
            xmodelbase,
            mayhem,
            material,
            computeshader,
            vertexshader,
            hullshader,
            domainshader,
            pixelshader,
            vertexdecl,
            techset,
            image,
            sound,
            soundsubmix,
            sndcurve,
            distcurve,
            reverbsendcurve,
            sndcontext,
            aliasparametermodifer,
            aliascombatcone,
            loaded_sound,
            col_map_mp,
            com_map,
            glass_map,
            aipaths,
            navmesh,
            vehicle_track,
            map_ents,
            fx_map,
            gfx_map,
            gfx_map_trzone,
            col_map_trzone,
            iesprofile,
            lightdef,
            ui_map,
            animclass,
            localize,
            attachment,
            weapon,
            snddriverglobals,
            fx,
            impactfx,
            surfacefx,
            aitype,
            mptype,
            character,
            xmodelalias,
            rawfile,
            scriptfile,
            stringtable,
            leaderboarddef,
            virtualleaderboarddef,
            structureddatadef,
            ddl,
            tracer,
            vehicle,
            addon_map_ents,
            netconststrings,
            reverbpreset,
            luafile,
            scripttable,
            equipsndtable,
            vectorfield,
            particlesimanimation,
            laser,
            beam,
            skeletonscript,
            clut,
            ttf,
            splines,
            physclothtuning,
            dlogschema,
            dlogroutes,
        }

        /// <summary>
        /// Structure of a WWII XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public int FreeHead { get; set; }
            public int Entries { get; set; }
        }

        /// <summary>
        /// Structure of a WWII XAsset Pool Size.
        /// </summary>
        public struct DBAssetPoolSize
        {
            public uint PoolSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the DBAssetPools and DBAssetPoolSizes addresses of WWII.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if addresses are valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();
            long moduleSize = instance.Reader.GetModuleMemorySize();

            var scanDBAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x4A, 0x8B, 0xAC, null, null, null, null, null, 0x48, 0x85, 0xED },
                BaseAddress,
                BaseAddress + moduleSize,
                true);
            var scanDBAssetPoolSizes = instance.Reader.FindBytes(
                new byte?[] { 0x83, 0xBC, null, null, null, null, null, 0x01, 0x7F, 0x48 },
                BaseAddress,
                BaseAddress + moduleSize,
                true);
            
            if (scanDBAssetPools.Length > 0 && scanDBAssetPoolSizes.Length > 0)
            {
                DBAssetPools = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0x4) + BaseAddress;
                DBAssetPoolSizes = instance.Reader.ReadInt32(scanDBAssetPoolSizes[0] + 0x3) + BaseAddress;

                // In WWII, empty_model will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "empty_model")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of WWII.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Name of the XModel.</returns>
        public string GetFirstXModel(JekyllInstance instance)
        {
            long address = DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * (int)XAssetType.xmodel);
            long pool = instance.Reader.ReadInt64(address) + Marshal.SizeOf<DBAssetPool>();
            long name = instance.Reader.ReadInt64(pool);

            return instance.Reader.ReadNullTerminatedString(name);
        }

        /// <summary>
        /// Creates a shallow copy of the WWII IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}