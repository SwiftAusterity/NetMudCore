using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Gaia;
using System.Collections.Generic;
using System.Linq;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class CelestialCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            IEnumerable<string> valueCollection = input as IEnumerable<string>;

            HashSet<ICelestial> collective = new(valueCollection.Select(stringInput => TemplateCache.Get<ICelestial>(long.Parse(stringInput))));

            return collective;
        }
    }
}
