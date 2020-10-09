using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Ghosts (IW6)
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
            physpreset,
            phys_collmap,
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
            sndcurve,
            lpfcurve,
            reverbsendcurve,
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
            font,
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
            structureddatadef,
            tracer,
            vehicle,
            addon_map_ents,
            netconststrings,
            reverbpreset,
            luafile,
            scriptable,
            colorization,
            colorizationset,
            tonemapping,
            equipsndtable,
            vectorfield,
            dopplerpreset,
            particlesimanimation,
        }

        /// <summary>
        /// Structure of a Ghosts XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public int FreeHead { get; set; }
            public int Entries { get; set; }
        }

        /// <summary>
        /// Structure of an Ghosts XAsset Pool Size.
        /// </summary>
        public struct DBAssetPoolSize
        {
            public int PoolSize { get; set; }
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
                DBAssetPools = instance.Reader.ReadInt32(scanDBAssetPools[0] - 0xB) + BaseAddress;

                if (instance.Reader.ActiveProcess.ProcessName == "iw6sp64_ship")
                {
                    DBAssetPoolSizes = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0x26) + BaseAddress;
                }
                else if (instance.Reader.ActiveProcess.ProcessName == "iw6mp64_ship")
                {
                    DBAssetPoolSizes = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0x1E) + BaseAddress;
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
            long address = DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * (int)XAssetType.xmodel);
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