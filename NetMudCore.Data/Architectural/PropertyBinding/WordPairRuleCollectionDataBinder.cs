using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Linguistic;
using System.Collections.Generic;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class WordPairRuleCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            HashSet<IWordPairRule> collective = new();

            return collective;
        }
    }
}
