using System.Collections.Generic;

namespace JekyllLibrary.Library
{
    public abstract class IXAssetPool
    {
        /// <summary>
        /// Gets the XAsset Pool Name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the Setting Group for this XAsset Pool
        /// </summary>
        // string SettingGroup { get; }

        /// <summary>
        /// Gets the XAsset Pool Index
        /// </summary>
        public abstract int Index { get; }

        /// <summary>
        /// Gets the XAsset Header Size
        /// </summary>
        public int XAssetSize { get; set; }

        /// <summary>
        /// Gets or Sets the number of XAsset slots in this pool
        /// </summary>
        public int XAssetCount { get; set; }

        /// <summary>
        /// Gets or Sets the start Address of this pool
        /// </summary>
        public long StartAddress { get; set; }

        /// <summary>
        /// Gets or Sets the end Address of this pool
        /// </summary>
        public abstract long EndAddress { get; set; }

        /// <summary>
        /// Loads XAssets from the given XAsset Pool
        /// </summary>
        public abstract List<GameXAsset> Load(JekyllInstance instance);

        /// <summary>
        /// Exports the given XAsset from the game
        /// </summary>
        public abstract JekyllStatus Export(GameXAsset xasset, JekyllInstance instance);

        /// <summary>
        /// Checks if the given XAsset is null
        /// </summary>
        public bool IsNullXAsset(GameXAsset xasset)
        {
            return IsNullXAsset(xasset.NameLocation);
        }

        /// <summary>
        /// Checks if the given pointer points to a null slot
        /// </summary>
        public bool IsNullXAsset(long nameAddress)
        {
            return nameAddress >= StartAddress && nameAddress <= XAssetCount * XAssetSize + StartAddress || nameAddress == 0;
        }
    }
}