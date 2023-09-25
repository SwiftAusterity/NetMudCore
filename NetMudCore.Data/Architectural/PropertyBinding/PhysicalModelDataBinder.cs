using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using System.Collections.Generic;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class PhysicalModelDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            HashSet<Coordinate> returnList = new();

            for (int y = 21; y >= 0; y--)
            {
            }

            return returnList;
        }
    }
}
