using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.NaturalResource;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class MineralResourceBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            long id = long.Parse(stringInput);

            return TemplateCache.Get<IMineral>(id) as INaturalResource;
        }
    }
}
