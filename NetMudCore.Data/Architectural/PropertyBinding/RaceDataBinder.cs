using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.PropertyBinding;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class RaceDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<IRace>(long.Parse(stringInput));
        }
    }
}
