using System.Collections.Generic;

namespace JekyllLibrary.Library
{
    public interface IGame
    {
        /// <summary>
        /// Gets or Sets the Game's name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the Game's Process Names
        /// </summary>
        string[] ProcessNames { get; }

        /// <summary>
        /// Gets the Memory Addresses of the XAsset Pools
        /// </summary>
        long[] XAssetPoolsAddresses { get; set; }

        /// <summary>
        /// Gets or Sets the Base Address
        /// </summary>
        long BaseAddress { get; set; }

        /// <summary>
        /// Gets or Sets the Process Index (Matches the Address + Process Name Array)
        /// </summary>
        int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or Sets the List of XAsset Pools
        /// </summary>
        List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// Validates the games addresses
        /// </summary>
        /// <returns>True if the addresses are valid, otherwise false</returns>
        bool ValidateAddresses(JekyllInstance instance);

        /// <summary>
        /// Creates a shallow copy of the Game Object
        /// </summary>
        /// <returns>Copied Game Object</returns>
        object Clone();
    }
}