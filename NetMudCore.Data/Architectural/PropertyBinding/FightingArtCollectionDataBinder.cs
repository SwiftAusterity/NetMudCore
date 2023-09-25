using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Combat;
using System.Collections.Generic;
using System.Linq;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class FightingArtCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
                return null;

            var valueCollection = input as IEnumerable<string>;

            var collective = new HashSet<IFightingArt>(valueCollection.Select(str => TemplateCache.Get<IFightingArt>(long.Parse(str))));

            return collective;
        }
    }
}
