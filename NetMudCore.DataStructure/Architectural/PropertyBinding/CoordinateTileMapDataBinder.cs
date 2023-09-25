﻿namespace NetMudCore.DataStructure.Architectural.PropertyBinding
{
    public class CoordinateTileMapDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
            }

            long[,] coordinateGrouping = new long[100, 100];
            IEnumerable<string>? inputArray = input as IEnumerable<string>;
            int maxNodes = inputArray!.Count() - 1;
            short x = 0;
            short y = 99;

            for (int i = 0; i < maxNodes; i++)
            {
                coordinateGrouping[x, y] = long.Parse(inputArray!.ElementAt(i));

                x++;
                if (x > 99)
                {
                    y--;
                    x = 0;
                }

                if (y < 0)
                {
                    break;
                }
            }

            return coordinateGrouping;
        }
    }
}
