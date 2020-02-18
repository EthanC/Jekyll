using System.Collections.Generic;

namespace JekyllLibrary.Library
{
    public abstract class IAssetPool
    {
        /// <summary>
        /// Gets the Asset Pool Name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the Setting Group for this Asset Pool
        /// </summary>
        //string SettingGroup { get; }

        /// <summary>
        /// Gets the Asset Pool Index
        /// </summary>
        public abstract int Index { get; }

        /// <summary>
        /// Gets the Asset Header Size
        /// </summary>
        public int AssetSize { get; set; }

        /// <summary>
        /// Gets or Sets the number of Asset slots in this pool
        /// </summary>
        public int AssetCount { get; set; }

        /// <summary>
        /// Gets or Sets the start Address of this pool
        /// </summary>
        public long StartAddress { get; set; }

        /// <summary>
        /// Gets or Sets the end Address of this pool
        /// </summary>
        public abstract long EndAddress { get; set; }

        /// <summary>
        /// Loads Assets from the given Asset Pool
        /// </summary>
        public abstract List<GameAsset> Load(JekyllInstance instance);

        /// <summary>
        /// Exports the given asset from the game
        /// </summary>
        public abstract JekyllStatus Export(GameAsset asset, JekyllInstance instance);

        /// <summary>
        /// Checks if the given asset is null
        /// </summary>
        public bool IsNullAsset(GameAsset asset)
        {
            return IsNullAsset(asset.NameLocation);
        }

        /// <summary>
        /// Checks if the given pointer points to a null slot
        /// </summary>
        public bool IsNullAsset(long nameAddress)
        {
            return nameAddress >= StartAddress && nameAddress <= AssetCount * AssetSize + StartAddress || nameAddress == 0;
        }
    }
}