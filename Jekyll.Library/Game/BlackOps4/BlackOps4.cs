using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Black Ops 4
    /// Aliases: T8, Python, Viper
    /// </summary>
    public partial class BlackOps4 : IGame
    {
        /// <summary>
        /// Gets the name of Black Ops 4.
        /// </summary>
        public string Name => "Black Ops 4";

        /// <summary>
        /// Gets the process names of Black Ops 4.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "BlackOps4"
        };

        /// <summary>
        /// Gets or sets the process index of Black Ops 4.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Black Ops 4.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the DBAssetPools address of Black Ops 4.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the DBAssetPoolSizes address of Black Ops 4.
        /// Not used for this title, instead, it is stored in DBAssetPool.
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Black Ops 4.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of Black Ops 4.
        /// </summary>
        private enum XAssetType : int
        {
            physpreset,
            physconstraints,
            destructibledef,
            xanim,
            xmodel,
            xmodelmesh,
            material,
            computeshaderset,
            techset,
            image,
            sound,
            col_map,
            com_map,
            game_map,
            gfx_map,
            fonticon,
            localizeentry,
            localize,
            gesture,
            gesturetable,
            weapon,
            weaponfull,
            weapontunables,
            cgmediatable,
            playersoundstable,
            playerfxtable,
            sharedweaponsounds,
            attachment,
            attachmentunique,
            weaponcamo,
            customizationtable,
            customizationtable_feimages,
            snddriverglobals,
            fx,
            tagfx,
            klf,
            impactsfxtable,
            impactsoundstable,
            aitype,
            character,
            xmodelalias,
            rawfile,
            animtree,
            stringtable,
            structuredtable,
            leaderboarddef,
            ddl,
            glasses,
            scriptparsetree,
            scriptparsetreedbg,
            scriptparsetreeforced,
            keyvaluepairs,
            vehicle,
            tracer,
            surfacefxtable,
            surfacesounddef,
            footsteptable,
            entityfximpacts,
            entitysoundimpacts,
            zbarrier,
            vehiclefxdef,
            vehiclesounddef,
            typeinfo,
            scriptbundle,
            scriptbundlelist,
            rumble,
            bulletpenetration,
            locdmgtable,
            aimtable,
            shoottable,
            playerglobaltunables,
            animselectortable,
            animmappingtable,
            animstatemachine,
            behaviortree,
            behaviorstatemachine,
            ttf,
            sanim,
            lightdescription,
            shellshock,
            statuseffect,
            cinematic_camera,
            cinematic_sequence,
            spectate_camera,
            xcam,
            bgcache,
            texturecombo,
            flametable,
            bitfield,
            maptable,
            maptablelist,
            maptableloadingimages,
            maptablepreviewimages,
            maptableentrylevelassets,
            objective,
            objectivelist,
            navmesh,
            navvolume,
            laser,
            beam,
            streamerhint,
            flowgraph,
            postfxbundle,
            luafile,
            luafiledebug,
            renderoverridebundle,
            staticlevelfxlist,
            triggerlist,
            playerroletemplate,
            playerrolecategorytable,
            playerrolecategory,
            characterbodytype,
            gametypetable,
            feature,
            featuretable,
            unlockableitem,
            unlockableitemtable,
            entitylist,
            playlists,
            playlistglobalsettings,
            playlistevent,
            playlisteventschedule,
            motionmatchinginput,
            blackboard,
            tacticalquery,
            playermovementtunables,
            hierarchicaltasknetwork,
            ragdoll,
            storagefile,
            storagefilelist,
            charmixer,
            storeproduct,
            storecategory,
            storecategorylist,
            rank,
            ranktable,
            prestige,
            prestigetable,
            firstpartyentitlement,
            firstpartyentitlementlist,
            entitlement,
            entitlementlist,
            sku,
            labelstore,
            labelstorelist,
            cpu_occlusion_data,
            lighting,
            streamerworld,
            talent,
            playertalenttemplate,
            playeranimation,
            playeranimscript,
            terraingfx,
            highlightreelinfodefines,
            highlightreelprofileweighting,
            highlightreelstarlevels,
            dlogevent,
            rawstring,
            ballisticdesc,
            streamkey,
            rendertargets,
            drawnodes,
            grouplodmodel,
            fxlibraryvolume,
            assetlist,
            report
        }

        /// <summary>
        /// Structure of a Black Ops 4 XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public long Entries { get; set; }
            public uint ElementSize { get; set; }
            public uint PoolSize { get; set; }
            public int NullPadding { get; set; }
            public int UsedCount { get; set; }
            public long FreeHead { get; set; }
        }

        /// <summary>
        /// Validates and sets the DBAssetPools address of Black Ops 4.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if address is valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();

            var scanDBAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x48, 0x89, 0x5C, 0x24, null, 0x57, 0x48, 0x83, 0xEC, null, 0x0F, 0xB6, 0xF9, 0x48, 0x8D, 0x05 },
                BaseAddress,
                BaseAddress + instance.Reader.GetModuleMemorySize(),
                true);

            if (scanDBAssetPools.Length > 0)
            {
                DBAssetPools = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0x10) + (scanDBAssetPools[0] + 0x14);

                // In Black Ops 4, 10c968e933756404 will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "10c968e933756404")
                {
                    List<Dictionary<string, object>> pools = new List<Dictionary<string, object>>();

                    foreach (int index in Enum.GetValues(typeof(XAssetType)))
                    {
                        DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(instance.Game.DBAssetPools + (index * Marshal.SizeOf<DBAssetPool>()));

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
        /// Gets the first entry in the XModel XAsset Pool of Black Ops 4.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Name of the XModel.</returns>
        public string GetFirstXModel(JekyllInstance instance)
        {
            long address = instance.Game.DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * (int)XAssetType.xmodel);
            DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(address);

            return instance.Reader.ReadBytesToString(pool.Entries).ToLower();
        }

        /// <summary>
        /// Creates a shallow copy of the Black Ops 4 IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}