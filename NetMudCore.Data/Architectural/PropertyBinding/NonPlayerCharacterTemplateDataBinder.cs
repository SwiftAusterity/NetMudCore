using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.NPC;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class NonPlayerCharacterTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<INonPlayerCharacterTemplate>(long.Parse(stringInput));
        }
    }
}
