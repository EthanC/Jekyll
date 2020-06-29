using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Modern Warfare Remastered (H1)
    /// </summary>
    public partial class ModernWarfareRemastered : IGame
    {
        /// <summary>
        /// Gets the name of Modern Warfare Remastered.
        /// </summary>
        public string Name => "Modern Warfare Remastered";

        /// <summary>
        /// Gets the process names of Modern Warfare Remastered.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "h1_sp64_ship",
            "h1_mp64_ship"
        };

        /// <summary>
        /// Gets or sets the process index of Modern Warfare Remastered.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Modern Warfare Remastered.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pools address of Modern Warfare Remastered.
        /// </summary>
        public long XAssetPoolsAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pool Sizes address of Modern Warfare Remastered.
        /// </summary>
        public long XAssetPoolSizesAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Modern Warfare Remastered.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Pools of Modern Warfare Remastered.
        /// </summary>
        private enum XAssetPool : int
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
        /// Structure of a Modern Warfare Remastered XAsset Pool.
        /// </summary>
        public struct XAssetPoolData
        {
            public int FreeHeaderPointer { get; set; }
            public int PoolEntries { get; set; }
        }

        /// <summary>
        /// Structure of a Modern Warfare Remastered XAsset Pool Size.
        /// </summary>
        public struct XAssetPoolSizesData
        {
            public int PoolSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the XAsset Pools and XAsset Pool Sizes addresses of Modern Warfare Remastered.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if addresses are valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();

            long[] scanXAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x48, 0x8B, 0xD8, 0x48, 0x85, 0xC0, 0x75, null, 0xF0, 0xFF, 0x0D },
                BaseAddress,
                BaseAddress + instance.Reader.GetModuleMemorySize(),
                true);

            if (scanXAssetPools.Length > 0)
            {
                XAssetPoolsAddress = instance.Reader.ReadInt32(scanXAssetPools[0] - 0xC) + BaseAddress;
                XAssetPoolSizesAddress = instance.Reader.ReadInt32(scanXAssetPools[0] + 0x3C) + BaseAddress;

                // In Modern Warfare Remastered, fx will always be the first entry in the XModel XAsset Pool.
                if (instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadInt64(XAssetPoolsAddress + (Marshal.SizeOf<XAssetPoolData>() * (int)XAssetPool.xmodel)) + Marshal.SizeOf<XAssetPoolData>())) == "fx")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a shallow copy of the Modern Warfare Remastered IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}