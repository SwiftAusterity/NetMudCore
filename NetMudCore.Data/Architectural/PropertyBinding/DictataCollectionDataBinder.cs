using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Linguistic;
using System.Collections.Generic;
using System.Linq;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class DictataCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            IEnumerable<string> valueCollection = input as IEnumerable<string>;

            HashSet<IDictata> collective = new(valueCollection.Select(str => ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), str, ConfigDataType.Dictionary))?.WordForms.FirstOrDefault()));

            return collective;
        }
    }
}
