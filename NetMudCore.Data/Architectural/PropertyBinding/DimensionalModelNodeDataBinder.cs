using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using System;
using System.Collections.Generic;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class DimensionalModelNodeDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            HashSet<IDimensionalModelNode> nodes = new();
            IEnumerable<string> nodeValues = input as IEnumerable<string>;

            short i = 0;
            foreach(string nodeValue in nodeValues)
            {
                nodes.Add(new DimensionalModelNode() { XAxis = i, Style = (DamageType)Enum.Parse(typeof(DamageType), nodeValue) });
                i++;
            }

            return nodes;
        }
    }
}
