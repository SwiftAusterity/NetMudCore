using System;

namespace NetMudCore.Lexica.DeepLex
{
    [Serializable]
    public class Artwork
    {
        /// <summary>
        /// filename of target image
        /// </summary>
        public string artid { get; set; }

        /// <summary>
        ///  image caption text
        /// </summary>
        public string capt { get; set; }
    }
}
