﻿using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class AffectPatternDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            IEnumerable<string> coordinateGrouping = input as IEnumerable<string>;
            HashSet<Coordinate> returnList = new();

            for (int y = 10; y >= 0; y--)
            {
                for(int x = 0; x <= 10; x++)
                {
                    bool isChecked = coordinateGrouping.ElementAt(y).Split(",", StringSplitOptions.RemoveEmptyEntries)[x] == "1";

                    if (isChecked)
                    {
                        returnList.Add(new Coordinate(x, y, -1));
                    }
                }
            }

            return returnList;
        }
    }
}
