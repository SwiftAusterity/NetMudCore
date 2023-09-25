﻿using System;
using System.Collections.Generic;

namespace NetMudCore.Lexica.DeepLex
{
    [Serializable]
    public class AntonymList
    {
        public List<string> item { get; set; }

        public AntonymList()
        {
            item = new List<string>();
        }
    }
}
