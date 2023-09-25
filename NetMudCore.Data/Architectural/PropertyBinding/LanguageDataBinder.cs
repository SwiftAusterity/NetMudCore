using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Linguistic;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class LanguageDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            ILanguage returnValue = ConfigDataCache.Get<ILanguage>(new ConfigDataCacheKey(typeof(ILanguage), stringInput, ConfigDataType.Language));

            return returnValue;
        }
    }
}
