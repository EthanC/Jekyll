using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Infinite Warfare
    /// Aliases: IW7, Genesis
    /// </summary>
    public partial class InfiniteWarfare : IGame
    {
        /// <summary>
        /// Gets the name of Infinite Warfare.
        /// </summary>
        public string Name => "Infinite Warfare";

        /// <summary>
        /// Gets the process names of Infinite Warfare.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "iw7_ship"
        };

        /// <summary>
        /// Gets or sets the process index of Infinite Warfare.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Infinite Warfare.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the DBAssetPools address of Infinite Warfare.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the DBAssetPoolSizes address of Infinite Warfare.
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Infinite Warfare.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of Infinite Warfare.
        /// </summary>
        private enum XAssetType : int
        {
            physicslibrary,
            physicssfxeventasset,
            physicsvfxeventasset,
            physicsasset,
            physicsfxpipeline,
            physicsfxshape,
            xanim,
            xmodelsurfs,
            xmodel,
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
            soundglobals,
            soundbank,
            soundpatch,
            col_map,
            com_map,
            glass_map,
            aipaths,
            navmesh,
            map_ents,
            fx_map,
            gfx_map,
            gfx_map_trzone,
            iesprofile,
            lightdef,
            ui_map,
            animclass,
            playeranim,
            gesture,
            localize,
            attachment,
            weapon,
            vfx,
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
            luafile,
            scriptable,
            equipsndtable,
            vectorfield,
            particlesimanimation,
            streaminginfo,
            laser,
            ttf,
            suit,
            suitanimpackage,
            spaceshiptarget,
            rumble,
            rumblegraph,
            animpkg,
            sfxpkg,
            vfxpkg,
            behaviortree,
            animarchetype,
            proceduralbones,
            reticle,
            gfxlightmap,
        }

        /// <summary>
        /// Structure of an Infinite Warfare XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public int FreeHead { get; set; }
            public int Entries { get; set; }
        }

        /// <summary>
        /// Structure of an Infinite Warfare XAsset Pool Size.
        /// </summary>
        public struct DBAssetPoolSize
        {
            public uint PoolSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the DBAssetPools and DBAssetPoolSizes addresses of Infinite Warfare.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if addresses are valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();
            long moduleSize = instance.Reader.GetModuleMemorySize();

            long[] scanDBAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x48, 0x63, 0xC1, 0x48, 0x8D, 0x15, null, null, null, null, 0x48, 0x8B, 0x8C, 0xC2 },
                BaseAddress,
                BaseAddress + moduleSize,
                true);
            long[] scanDBAssetPoolSizes = instance.Reader.FindBytes(
                new byte?[] { 0x72, null, 0x48, 0x63, 0xC1, 0x48, 0x8D, 0x0D, null, null, null, null, 0x83, 0x3C, 0x81 },
                BaseAddress,
                BaseAddress + moduleSize,
                true);

            if (scanDBAssetPools.Length > 0 && scanDBAssetPoolSizes.Length > 0)
            {
                DBAssetPools = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0xE) + BaseAddress;
                DBAssetPoolSizes = instance.Reader.ReadInt32(scanDBAssetPoolSizes[0] + 0x8) + (scanDBAssetPoolSizes[0] + 0xC);

                // In Infinite Warfare, viewmodel_default will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "viewmodel_default")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of Infinite Warfare.
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
        /// Creates a shallow copy of the Infinite Warfare IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}