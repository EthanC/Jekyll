using System.Collections.Generic;

namespace JekyllLibrary.Library
{
    public abstract class IXAssetPool
    {
        /// <summary>
        /// Gets the name of the XAsset Pool.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the index of the XAsset Pool.
        /// </summary>
        public abstract int Index { get; }

        /// <summary>
        /// Gets or sets the start address of the XAsset Pool.
        /// </summary>
        public long Entries { get; set; }

        /// <summary>
        /// Gets or sets the end address of the XAsset Pool.
        /// </summary>
        public abstract long EndAddress { get; set; }

        /// <summary>
        /// Gets or sets the header size of XAssets in the XAsset Pool.
        /// </summary>
        public int ElementSize { get; set; }

        /// <summary>
        /// Gets or sets the number of slots in the XAsset Pool.
        /// </summary>
        public int PoolSize { get; set; }

        /// <summary>
        /// Loads XAssets from the specified XAsset Pool.
        /// </summary>
        public abstract List<GameXAsset> Load(JekyllInstance instance);

        /// <summary>
        /// Exports the given XAsset from the current game.
        /// </summary>
        public abstract JekyllStatus Export(GameXAsset xasset, JekyllInstance instance);

        /// <summary>
        /// Determines if the specified XAsset is a null slot.
        /// </summary>
        public bool IsNullXAsset(long nameAddress)
        {
            return nameAddress >= Entries && nameAddress <= PoolSize * ElementSize + Entries || nameAddress == 0;
        }
    }
}