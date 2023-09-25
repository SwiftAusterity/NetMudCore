﻿using System;
using System.Collections.Generic;

namespace NetMudCore.Lexica.DeepLex
{
    [Serializable]
    public class UndefinedRunOnText
    {
        public VerbalIllustration vis { get; set; }

        public List<UsageNotes> uns { get; set; }

        public UndefinedRunOnText()
        {
            uns = new List<UsageNotes>();
        }
    }
}
