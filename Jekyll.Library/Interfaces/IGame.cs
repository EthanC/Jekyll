using System.Collections.Generic;

namespace JekyllLibrary.Library
{
    public interface IGame
    {
        /// <summary>
        /// Gets the name of the game.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the process names of the game.
        /// </summary>
        string[] ProcessNames { get; }

        /// <summary>
        /// Gets or sets the process index for the game.
        /// </summary>
        int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address for the game.
        /// </summary>
        long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the memory address for the DBAssetPools of the game.
        /// </summary>
        long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the memory address for the DBAssetPoolSizes of the game (optional).
        /// </summary>
        long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools for the game.
        /// </summary>
        List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// Initializes the game and validates its associated addresses.
        /// </summary>
        /// <returns>True if valid addresses are found, otherwise false.</returns>
        bool InitializeGame(JekyllInstance instance);

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of the game.
        /// </summary>
        /// <returns>True if valid addresses are found, otherwise false.</returns>
        string GetFirstXModel(JekyllInstance instance);

        /// <summary>
        /// Creates a shallow copy of the IGame object.
        /// </summary>
        /// <returns>Shallow copy of the IGame object.</returns>
        object Clone();
    }
}