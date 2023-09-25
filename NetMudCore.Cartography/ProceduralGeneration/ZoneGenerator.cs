﻿using NetMudCore.DataStructure.Locale;
using System;
using System.Linq;

namespace NetMudCore.Cartography.ProceduralGeneration
{
    /// <summary>
    /// Generate some zones procedurally
    /// </summary>
    public class LocaleGenerator
    {
        private readonly Random _randomizer;
        private const string roomSymbol = "*";

        /// <summary>
        /// The rand seed
        /// </summary>
        public int Seed { get; private set; }

        /// <summary>
        /// Width of the zone (X axis)
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Length of the zone (Y axis)
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Height of zone above center line
        /// </summary>
        public int Elevation { get; private set; }

        /// <summary>
        /// Height of zone under center line
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Diameter of the zone array
        /// </summary>
        public int Diameter { get; private set; }

        /// <summary>
        /// Radius of the zone array
        /// </summary>
        public int Radius { get; private set; }

        /// <summary>
        /// How hilly the zone should be
        /// </summary>
        public int HillFrequency { get; set; }

        /// <summary>
        /// Variance on cave generation
        /// </summary>
        public int CaveFrequency { get; set; }

        /// <summary>
        /// How smooth should the system make the hills and caves (lower = more sheet cliffs)
        /// </summary>
        public int VariantSmoothness { get; set; }

        /// <summary>
        /// Should the zone have a final pass and preset structures be created
        /// </summary>
        public bool CreateStructures { get; set; }

        /// <summary>
        /// Should a pass be made to create degredated structures
        /// </summary>
        public bool CreateRuins { get; set; }

        /// <summary>
        /// Center room x,y,z of the array
        /// </summary>
        public Tuple<int, int, int> Center { get; private set; }

        /// <summary>
        /// The zone we're filling
        /// </summary>
        public ILocale Locale { get; private set; }

        /// <summary>
        /// The room map array
        /// </summary>
        public long[, ,] RoomMap { get; private set; }

        /// <summary>
        /// Is this zone ready to generate rooms for
        /// </summary>
        public bool Primed { get; private set; }

        public LocaleGenerator(int seed, ILocale locale, int width, int length, int elevation, int depth)
        {
            VerifyLocale(locale);
            VerifyDimensions(width, length, elevation, depth);

            _randomizer = new Random(Seed);

            Seed = seed;
            Locale = locale;

            Width = width;
            Length = length;
            Elevation = elevation;
            Depth = depth;

            RoomMap = new long[width * 3 + 1, length * 3 + 1, (elevation + depth) * 3 + 1];
        }

        public LocaleGenerator(ILocale locale, int width, int length, int elevation, int depth)
        {
            VerifyLocale(locale);
            VerifyDimensions(width, length, elevation, depth);

            Random rand = new();
            _randomizer = new Random(Seed);

            Seed = rand.Next(10000);
            Locale = locale;
            Width = width;
            Length = length;
            Elevation = elevation;
            Depth = depth;

            RoomMap = new long[width * 3 + 1, length * 3 + 1, (elevation + depth) * 3 + 1];
        }

        /// <summary>
        /// Creates the map, but only the array, does not create the rooms
        /// </summary>
        public void FillMap()
        {
            //We'll build the potential rooms first just using strings
            string[,,] prototypeMap = new string[Width * 3 + 1, Length * 3 + 1, (Elevation + Depth) * 3 + 1];
            Tuple<int, int, int> center = new(Width * 3 / 2 + 1, Length * 3 / 2 + 1, (Elevation + Depth) * 3 / 2 + 1);

            //Find the absolute max boundings
            int maxX = prototypeMap.GetUpperBound(0);
            int maxY = prototypeMap.GetUpperBound(1);
            _ = prototypeMap.GetUpperBound(2);

            //Set up center room
            prototypeMap[center.Item1, center.Item2, center.Item3] = roomSymbol;

            int currentX = center.Item1;
            int currentY = center.Item2;
            int currentZ = center.Item3;

            //Do 4 point cardinal directions for the initial layer
            for (int variance = 1; currentX < maxX || currentY < maxY; variance++)
            {
                //Room or pathway?
                bool isRoom = variance % 3 == 0;

                //Do X, don't do it if we're at or over max bounding for X specifically
                if(currentX + 1 < maxX)
                {
                    int roll = _randomizer.Next(1, 100);

                    if(isRoom && roll >= 25)
                    {
                        prototypeMap[center.Item1 + variance, currentY, currentZ] = roomSymbol;

                        if(roll >= 50)
                        {
                            prototypeMap[center.Item1 - variance, currentY, currentZ] = roomSymbol;
                        }
                    }
                    else if(roll >= 50)
                    {
                        prototypeMap[center.Item1 + variance, currentY, currentZ] = "-";

                        if (roll >= 75)
                        {
                            prototypeMap[center.Item1 - variance, currentY, currentZ] = "-";
                        }
                    }

                    currentX++;
                }

                //Do Y
                if (currentY + 1 < maxY)
                {
                    int roll = _randomizer.Next(1, 100);

                    if (isRoom && roll >= 25)
                    {
                        prototypeMap[center.Item1, currentY + variance, currentZ] = roomSymbol;

                        if (roll >= 50)
                        {
                            prototypeMap[center.Item1, currentY - variance, currentZ] = roomSymbol;
                        }
                    }
                    else if (roll >= 50)
                    {
                        prototypeMap[center.Item1, currentY + variance, currentZ] = "|";

                        if (roll >= 75)
                        {
                            prototypeMap[center.Item1, currentY - variance, currentZ] = "|";
                        }
                    }

                    currentY++;
                }
            }

            //Do "cave" entrances (sloped down) and hills (sloped up)

            //Verify grid

            //Prime it after verification
            Primed = true;
        }

        public void ExecuteMap()
        {
            if (!Primed)
            {
                throw new AccessViolationException("Map is not primed yet.");
            }

            //Create rooms and pathways
        }

        /// <summary>
        /// We just throw errors, no need for a return value
        /// </summary>
        private static void VerifyLocale(ILocale locale)
        {
            if (locale == null)
            {
                throw new ArgumentNullException("Locale must not be null.");
            }

            if (locale.Rooms().Any())
            {
                throw new ArgumentOutOfRangeException("Locale must be devoid of rooms.");
            }

            if (locale.Template<ILocaleTemplate>().FitnessProblems)
            {
                throw new ArgumentOutOfRangeException("Zone must have data integrity.");
            }
        }

        private static void VerifyDimensions(int width, int length, int elevation, int depth)
        {
            if (width < 1 || length < 1 || elevation + depth < 0)
            {
                throw new ArgumentOutOfRangeException("Width and length must be at least 1 and elevation + depth must be at least zero.");
            }

            if (width > 100 || length > 100 || elevation > 100 || depth > 100)
            {
                throw new ArgumentOutOfRangeException("None of the dimensions can be greater than 100.");
            }
        }
    }
}
