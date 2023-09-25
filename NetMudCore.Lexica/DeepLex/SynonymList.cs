using System;
using System.Collections.Generic;

namespace NetMudCore.Lexica.DeepLex
{
    [Serializable]
    public class SynonymList
    {
        public List<string> item { get; set; }

        public SynonymList()
        {
            item = new List<string>();
        }
    }
}
