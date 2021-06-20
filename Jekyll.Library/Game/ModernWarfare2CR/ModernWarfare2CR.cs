using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Modern Warfare 2 Campaign Remastered
    /// Aliases: H1, Pheonix
    /// </summary>
    public partial class ModernWarfare2CR : IGame
    {
        /// <summary>
        /// Gets the name of Modern Warfare 2 Campaign Remastered.
        /// </summary>
        public string Name => "Modern Warfare 2 Campaign Remastered";

        /// <summary>
        /// Gets the process names of Modern Warfare 2 Campaign Remastered.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "MW2CR"
        };

        /// <summary>
        /// Gets or sets the process index of Modern Warfare 2 Campaign Remastered.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Modern Warfare 2 Campaign Remastered.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pools address of Modern Warfare 2 Campaign Remastered.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pool Sizes address of Modern Warfare 2 Campaign Remastered.
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Modern Warfare 2 Campaign Remastered.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of Modern Warfare 2 Campaign Remastered.
        /// </summary>
        private enum XAssetType : int
        {
            physpreset,
            phys_collmap,
            physwaterpreset,
            phys_worldmap,
            physconstraint,
            xanim,
            xmodelsurfs,
            xmodel,
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
            lpfcurve,
            reverbsendcurve,
            sndcontext,
            loaded_sound,
            col_map_mp,
            com_map,
            glass_map,
            aipaths,
            vehicle_track,
            map_ents,
            fx_map,
            gfx_map,
            lightdef,
            ui_map,
            menufile,
            menu,
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
            proto,
            tracer,
            vehicle,
            addon_map_ents,
            netconststrings,
            reverbpreset,
            luafile,
            scriptable,
            equipsndtable,
            vectorfield,
            dopplerpreset,
            particlesimanimation,
            laser,
            skeletonscript,
            clut,
            ttf,
        }

        /// <summary>
        /// Structure of a Modern Warfare 2 Campaign Remastered XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public int FreeHead { get; set; }
            public int Entries { get; set; }
        }

        /// <summary>
        /// Structure of a Modern Warfare 2 Campaign Remastered XAsset Pool Size.
        /// </summary>
        public struct DBAssetPoolSize
        {
            public uint PoolSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the DBAssetPools and DBAssetPoolSizes addresses of Modern Warfare 2 Campaign Remastered.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if addresses are valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();

            long[] scanDBAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x48, 0x8B, 0xD8, 0x48, 0x85, 0xC0, 0x75, null, 0xF0, 0xFF, 0x0D },
                BaseAddress,
                BaseAddress + instance.Reader.GetModuleMemorySize(),
                true);

            if (scanDBAssetPools.Length > 0)
            {
                DBAssetPools = instance.Reader.ReadInt32(scanDBAssetPools[0] - 0xC) + BaseAddress;
                DBAssetPoolSizes = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0x3C) + BaseAddress;

                // In Modern Warfare 2 Campaign Remastered, fx will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "fx")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of Modern Warfare 2 Campaign Remastered.
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
        /// Creates a shallow copy of the Modern Warfare 2 Campaign Remastered IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}