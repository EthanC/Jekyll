using System.IO;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// A generic class to hold a Call of Duty XAsset
    /// </summary>
    public class GameXAsset
    {
        /// <summary>
        /// Gets or Sets the Name of this XAsset
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the Display Name for this XAsset
        /// </summary>
        public string DisplayName { get { return Path.GetFileName(Name); } }

        /// <summary>
        /// Gets or Sets this XAsset's Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets Information for this XAsset
        /// </summary>
        public string Information { get; set; }

        /// <summary>
        /// Gets or Sets the Pointer to the XAsset's name
        /// </summary>
        public long NameLocation { get; set; }

        /// <summary>
        /// Gets or Sets the Header Address for this XAsset
        /// </summary>
        public long HeaderAddress { get; set; }

        /// <summary>
        /// Gets or Sets the size of this XAsset
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or Sets the Pool for this XAsset
        /// </summary>
        public IXAssetPool XAssetPool { get; set; }

        /// <summary>
        /// Returns the Name of this XAsset
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}