using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Linguistic;
using System.Linq;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class DictataDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            IDictata returnValue = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), stringInput, ConfigDataType.Dictionary))?.WordForms.FirstOrDefault();

            return returnValue;
        }
    }
}
