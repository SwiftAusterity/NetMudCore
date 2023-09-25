﻿using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Architectural.PropertyBinding;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class MaterialDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<IMaterial>(long.Parse(stringInput));
        }
    }
}
