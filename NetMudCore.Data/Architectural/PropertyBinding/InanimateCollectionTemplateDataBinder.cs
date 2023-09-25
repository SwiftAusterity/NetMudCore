using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Inanimate;
using System.Collections.Generic;
using System.Linq;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class InanimateCollectionTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            IEnumerable<string> valueCollection = input as IEnumerable<string>;

            HashSet<IInanimateTemplate> collective = new(valueCollection.Select(str => TemplateCache.Get<IInanimateTemplate>(long.Parse(str))));

            return collective;
        }
    }
}
