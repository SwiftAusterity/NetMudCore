using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural.PropertyBinding;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class HelpDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<IHelp>(long.Parse(stringInput));
        }
    }
}
