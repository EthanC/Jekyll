using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Modern Warfare (IW8)
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
            physicslibrary,
            physicssfxeventasset,
            physicsvfxeventasset,
            physicsasset,
            physicsfxpipeline,
            physicsfxshape,
            physicsdebugdata,
            xanim,
            xmodelsurfs,
            xmodel,
            mayhem,
            material,
            computeshader,
            libshader,
            vertexshader,
            hullshader,
            domainshader,
            pixelshader,
            techset,
            image,
            soundglobals,
            soundbank,
            soundbanktransient,
            col_map,
            com_map,
            glass_map,
            aipaths,
            navmesh,
            tacgraph,
            map_ents,
            fx_map,
            gfx_map,
            gfx_map_trzone,
            iesprofile,
            lightdef,
            gradingclut,
            ui_map,
            fogspline,
            animclass,
            playeranim,
            gesture,
            localize,
            attachment,
            weapon,
            vfx,
            impactfx,
            surfacefx,
            aitype,
            mptype,
            character,
            xmodelalias,
            rawfile,
            scriptfile,
            scriptdebugdata,
            stringtable,
            leaderboarddef,
            virtualleaderboarddef,
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
            accessory,
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
            collisiontile,
            execution,
            carryobject,
            soundbanklist,
            decalvolumematerial,
            decalvolumemask,
            dynentitylist,
            fx_map_trzone,
            dlogschema,
            edgelist,
        }

        /// <summary>
        /// Structure of a Modern Warfare XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public long Entries { get; set; }
            public long FreeHead { get; set; }
            public int PoolSize { get; set; }
            public int ElementSize { get; set; }
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
            long address = BaseAddress + DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * (int)XAssetType.xmodel);
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