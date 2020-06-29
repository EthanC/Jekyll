using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Black Ops III (T7)
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
        public long XAssetPoolsAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pool Sizes address of Black Ops III (unused, stored in pool data.)
        /// </summary>
        public long XAssetPoolSizesAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Black Ops III.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Pools of Black Ops III.
        /// </summary>
        private enum XAssetPool : int
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
            sound_patch,
            col_map,
            com_map,
            game_map,
            map_ents,
            gfx_map,
            lightdef,
            lensflaredef,
            ui_map,
            font,
            fonticon,
            localize,
            weapon,
            weapondef,
            weaponvariant,
            weaponfull,
            cgmediatable,
            playersoundstable,
            playerfxtable,
            sharedweaponsounds,
            attachment,
            attachmentunique,
            weaponcamo,
            customizationtable,
            customizationtable_feimages,
            customizationtablecolor,
            snddriverglobals,
            fx,
            tagfx,
            klf,
            impactsfxtable,
            impactsoundstable,
            player_character,
            aitype,
            character,
            xmodelalias,
            rawfile,
            stringtable,
            structuredtable,
            leaderboarddef,
            ddl,
            glasses,
            texturelist,
            scriptparsetree,
            keyvaluepairs,
            vehicle,
            addon_map_ents,
            tracer,
            slug,
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
            animselectortable,
            animmappingtable,
            animstatemachine,
            behaviortree,
            behaviorstatemachine,
            ttf,
            sanim,
            lightdescription,
            shellshock,
            xcam,
            bgcache,
            texturecombo,
            flametable,
            bitfield,
            attachmentcosmeticvariant,
            maptable,
            maptableloadingimages,
            medal,
            medaltable,
            objective,
            objectivelist,
            umbra_tome,
            navmesh,
            navvolume,
            binaryhtml,
            laser,
            beam,
            streamerhint,
            _string,
            assetlist,
            report,
            depend,
        }

        /// <summary>
        /// Structure of a Black Ops III XAsset Pool.
        /// </summary>
        public struct XAssetPoolData
        {
            public long PoolPointer { get; set; }
            public int XAssetSize { get; set; }
            public int PoolSize { get; set; }
            public int NullPadding { get; set; }
            public int XAssetCount { get; set; }
            public long FreeHeaderPointer { get; set; }
        }

        /// <summary>
        /// Validates and sets the XAsset Pools address of Black Ops III.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if address is valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();

            var scanXAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x63, 0xC1, 0x48, 0x8D, 0x05, null, null, null, null, 0x49, 0xC1, 0xE0, null, 0x4C, 0x03, 0xC0 },
                BaseAddress,
                BaseAddress + instance.Reader.GetModuleMemorySize(),
                true);

            if (scanXAssetPools.Length > 0)
            {
                XAssetPoolsAddress = instance.Reader.ReadInt32(scanXAssetPools[0] + 0x5) + (scanXAssetPools[0] + 0x9);

                // In Black Ops III, void will always be the first entry in the XModel XAsset Pool.
                if (instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt64(instance.Reader.ReadStruct<XAssetPoolData>(XAssetPoolsAddress + (Marshal.SizeOf<XAssetPoolData>() * (int)XAssetPool.xmodel)).PoolPointer)) == "void")
                {
                    return true;
                }
            }

            return false;
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