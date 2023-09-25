﻿using System.ComponentModel.DataAnnotations;

namespace NetMudCore.DataStructure.Architectural
{
    public class Coordinate
    {
        [DataType(DataType.Text)]
        public int X { get; set; }

        [DataType(DataType.Text)]
        public int Y { get; set; }

        [DataType(DataType.Text)]
        public int Z { get; set; }

        public Coordinate()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Coordinate(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }
    }
}
