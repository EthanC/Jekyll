using System;
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
        /// Gets or sets the header size of XAssets in the XAsset Pool.
        /// </summary>
        public uint ElementSize { get; set; }

        /// <summary>
        /// Gets or sets the number of slots in the XAsset Pool.
        /// </summary>
        public uint PoolSize { get; set; }

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

        /// <summary>
        /// Determines if the structure of the XAsset Pool is valid.
        /// </summary>
        public bool IsValidPool(string type, uint elementSize, int structSize)
        {
            if (elementSize == structSize)
            {
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to export {type} XAsset Type, Element Size ({ElementSize}) is not equal to Struct Size ({structSize})");
                Console.ResetColor();

                return false;
            }
        }
    }
}