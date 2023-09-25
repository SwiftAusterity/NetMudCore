using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Combat;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class FightingArtDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<IFightingArt>(long.Parse(stringInput));
        }
    }
}
