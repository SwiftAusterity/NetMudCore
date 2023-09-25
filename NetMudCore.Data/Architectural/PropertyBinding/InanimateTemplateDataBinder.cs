using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Inanimate;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class InanimateTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<IInanimateTemplate>(long.Parse(stringInput));
        }
    }
}
