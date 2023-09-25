﻿using System;

namespace NetMudCore.Lexica.DeepLex
{
    [Serializable]
    public class VerbalIllustration
    {
        /// <summary>
        /// Text
        /// </summary>
        public string t { get; set; }

        /// <summary>
        /// Author Quotation
        /// </summary>
        public AttributionOfQuote aq { get; set; }
    }
}
