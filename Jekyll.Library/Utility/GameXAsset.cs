namespace JekyllLibrary.Library
{
    /// <summary>
    /// Generic class that represents a Call of Duty XAsset.
    /// </summary>
    public class GameXAsset
    {
        /// <summary>
        /// Gets or sets the name of the XAsset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the XAsset.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the size of the XAsset.
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pool of the XAsset.
        /// </summary>
        public IXAssetPool XAssetPool { get; set; }

        /// <summary>
        /// Gets or sets the header address of the XAsset.
        /// </summary>
        public long HeaderAddress { get; set; }
    }
}