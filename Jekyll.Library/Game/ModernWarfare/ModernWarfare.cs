using System.Collections.Generic;

namespace JekyllLibrary.Library
{
    public partial class ModernWarfare : IGame
    {
        /// <summary>
        /// Gets Modern Warfare's Game Name
        /// </summary>
        public string Name => "Modern Warfare";

        /// <summary>
        /// Gets Modern Warfare's Process Names
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "ModernWarfare"
        };

        /// <summary>
        /// Gets Modern Warfare's Asset Pools Addresses
        /// </summary>
        public long[] AssetPoolsAddresses { get; set; } = new long[]
        {
            // 0xB030060
        };

        /// <summary>
        /// Gets or Sets Modern Warfare's Base Address (ASLR)
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or Sets the current Process Index
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the list of Asset Pools
        /// </summary>
        public List<IAssetPool> AssetPools { get; set; }

        private enum AssetPool : int
        {
            physicslibrary,
            physicssfxeventasset,
            physicsvfxeventasset,
            physicsasset,
            physicsfxpipeline,
            physicsfxshape,
            physicsdebugdata,
            xanim,
            xmodelsurfs,
            xmodel = 9,
            mayhem,
            material,
            computeshader,
            libshader,
            vertexshader,
            hullshader,
            domainshader,
            pixelshader,
            techset,
            image = 19,
            soundglobals = 21,
            soundbank,
            soundbanktransient,
            col_map,
            com_map,
            glass_map,
            aipaths,
            navmesh,
            tacgraph,
            map_ents = 29,
            fx_map,
            gfx_map = 32,
            gfx_map_trzone = 32,
            iesprofile,
            lightdef = 34,
            gradingclut,
            ui_map,
            fogspline,
            animclass,
            playeranim,
            localize = 41,
            attachment,
            weapon,
            impactfx,
            surfacefx,
            aitype,
            mptype,
            character,
            xmodelalias,
            rawfile = 51,
            scriptfile = 52,
            scriptdebugdata,
            stringtable = 54,
            leaderboarddef,
            virtualleaderboarddef,
            tracer,
            vehicle,
            addon_map_ents,
            netconststrings,
            luafile = 62,
            scriptable,
            equipsndtable,
            vectorfield,
            particlesimanimation,
            streaminginfo,
            laser,
            ttf = 69,
            suit,
            suitanimpackage,
            camera,
            hudoutline,
            spaceshiptarget,
            rumble,
            rumblegraph,
            animpkg,
            sfxpkg,
            vfxpkg,
            footstepvfx,
            behaviortree,
            aianimset,
            aiasm,
            proceduralbones,
            dynamicbones,
            reticle,
            xanimcurve,
            coverselector,
            enemyselector,
            clientcharacter,
            clothasset,
            cinematicmotion,
            locdmgtable,
            bulletpenetration,
            scriptbundle,
            blendspace2d,
            xcam,
            camo,
            xcompositemodel,
            xmodeldetailcollision,
            streamkey,
            streamtreeoverride,
            keyvaluepairs,
            stterrain,
            nativescriptpatch,
            carryobject,
            soundbanklist,
            decalvolumematerial,
            decalvolumemask,
            fx_map_trzone,
            dlogschema,
            edgelist,
            defaultdummy,
            dummy
        }

        // Modern Warfare Pool Data Structure
        public struct AssetPoolInfo
        {
            // The beginning of the pool
            public long PoolPtr { get; set; }

            // A pointer to the closest free header
            public long PoolFreeHeadPtr { get; set; }

            // The maximum pool size
            public int PoolSize { get; set; }

            // The size of the asset header
            public int AssetSize { get; set; }
        }

        public bool ValidateAddresses(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();

            foreach (var assetPoolsAddress in AssetPoolsAddresses)
            {
                if (instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadStruct<AssetPoolInfo>(BaseAddress + assetPoolsAddress + 24 * 9).PoolPtr)) == "axis_guide_createfx")
                {
                    return true;
                }
            }

            // Scan for it
            var assetDBScan = instance.Reader.FindBytes(
                new byte?[] { 0x48, 0x8D, 0x04, 0x40, 0x4C, 0x8D, 0x8E, null, null, null, null, 0x4D, 0x8D, 0x0C, 0xC1, 0x8D, 0x42, 0xFF },
                instance.Reader.GetBaseAddress(),
                instance.Reader.GetBaseAddress() + instance.Reader.GetModuleMemorySize(),
                true);
            var stringDBScan = instance.Reader.FindBytes(
                new byte?[] { 0x4C, 0x8B, 0xC2, 0x48, 0x8D, 0x1D, null, null, null, null, 0x48, 0x2B, 0xCB },
                instance.Reader.GetBaseAddress(),
                instance.Reader.GetBaseAddress() + instance.Reader.GetModuleMemorySize(),
                true);

            // Validate results
            if (assetDBScan.Length > 0 && stringDBScan.Length > 0)
            {
                AssetPoolsAddresses = new long[] { instance.Reader.ReadInt32(assetDBScan[0] + 0x7) };

                if (instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadStruct<AssetPoolInfo>(BaseAddress + AssetPoolsAddresses[0] + 24 * 9).PoolPtr)) == "axis_guide_createfx")
                {
                    return true;
                }
            }

            return false;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}