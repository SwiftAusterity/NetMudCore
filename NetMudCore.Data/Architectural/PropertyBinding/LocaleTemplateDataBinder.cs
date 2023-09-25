using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Locale;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class LocaleTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<ILocaleTemplate>(long.Parse(stringInput));
        }
    }
}
