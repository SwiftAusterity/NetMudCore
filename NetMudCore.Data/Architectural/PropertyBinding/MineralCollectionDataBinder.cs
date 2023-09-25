using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.NaturalResource;
using System.Collections.Generic;
using System.Linq;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class MineralCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            IEnumerable<string> valueCollection = input as IEnumerable<string>;

            HashSet<IMineral> collective = new(valueCollection.Select(str => TemplateCache.Get<IMineral>(long.Parse(str))));

            return collective;
        }
    }
}
