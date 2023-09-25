using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.System;
using System;
using System.Linq;

namespace NetMudCore.Cartography
{

    /// <summary>
    /// General utilities for map rendering and parsing
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Gets the opposite room from the origin based on direction
        /// </summary>
        /// <param name="origin">The room we're looking to oppose</param>
        /// <param name="direction">The direction the room would be in (this method will reverse the direction itself)</param>
        /// <returns>The room that is in the direction from our room</returns>
        public static IRoomTemplate GetOpposingRoom(IRoomTemplate origin, MovementDirectionType direction)
        {
            //There is no opposite of none directionals
            if (origin == null || direction == MovementDirectionType.None)
            {
                return null;
            }

            MovementDirectionType oppositeDirection = ReverseDirection(direction);

            System.Collections.Generic.IEnumerable<IPathwayTemplate> paths = TemplateCache.GetAll<IPathwayTemplate>();

            IPathwayTemplate ourPath = paths.FirstOrDefault(pt => origin.Equals(pt.Destination)
                                            && pt.DirectionType == oppositeDirection);

            if (ourPath != null)
            {
                return (IRoomTemplate)ourPath.Destination;
            }

            return null;
        }

        /// <summary>
        /// Is this coordinate out of bounds of the map
        /// </summary>
        /// <param name="boundings">a 3d coordinate x,y,z</param>
        /// <param name="map">the 3d map in question</param>
        /// <returns>whether it is out of bounds of the map</returns>
        public static bool IsOutOfBounds(Coordinate boundings, long[,,] map)
        {
            return map.GetUpperBound(0) < boundings.X || map.GetLowerBound(0) > boundings.X
                || map.GetUpperBound(1) < boundings.Y || map.GetLowerBound(1) > boundings.Y
                || map.GetUpperBound(2) < boundings.Z || map.GetLowerBound(2) > boundings.Z;

        }

        /// <summary>
        /// Is this coordinate out of bounds of the map
        /// </summary>
        /// <param name="boundings">a 2d coordinate x,y,z</param>
        /// <param name="map">the 2d map in question</param>
        /// <returns>whether it is out of bounds of the map</returns>
        public static bool IsOutOfBounds(Tuple<int, int> boundings, long[,] map)
        {
            return map.GetUpperBound(0) < boundings.Item1 || map.GetLowerBound(0) > boundings.Item1
                || map.GetUpperBound(1) < boundings.Item2 || map.GetLowerBound(1) > boundings.Item2;
        }

        /// <summary>
        /// Translates degreesFromNorth into direction words for pathways
        /// </summary>
        /// <param name="degreesFromNorth">the value to translate</param>
        /// <param name="inclineGrade">the value to translate</param>
        /// <param name="reverse">reverse the direction or not</param>
        /// <returns>translated text</returns>
        public static MovementDirectionType TranslateToDirection(int degreesFromNorth, int inclineGrade = 0, bool reverse = false)
        {
            int trueDegrees = degreesFromNorth;

            if (trueDegrees < 0)
            {
                if (inclineGrade > 0)
                {
                    return MovementDirectionType.Up;
                }
                else if (inclineGrade < 0)
                {
                    return MovementDirectionType.Down;
                }

                return MovementDirectionType.None;
            }

            if (reverse)
            {
                trueDegrees = degreesFromNorth < 180 ? degreesFromNorth + 180 : degreesFromNorth - 180;
                inclineGrade *= -1;
            }

            if (trueDegrees > 22 && trueDegrees < 67)
            {
                if (inclineGrade > 0)
                {
                    return MovementDirectionType.UpNorthEast;
                }
                else if (inclineGrade < 0)
                {
                    return MovementDirectionType.DownNorthEast;
                }

                return MovementDirectionType.NorthEast;
            }

            if (trueDegrees > 66 && trueDegrees < 111)
            {
                if (inclineGrade > 0)
                {
                    return MovementDirectionType.UpEast;
                }
                else if (inclineGrade < 0)
                {
                    return MovementDirectionType.DownEast;
                }

                return MovementDirectionType.East;
            }

            if (trueDegrees > 110 && trueDegrees < 155)
            {
                if (inclineGrade > 0)
                {
                    return MovementDirectionType.UpSouthEast;
                }
                else if (inclineGrade < 0)
                {
                    return MovementDirectionType.DownSouthEast;
                }

                return MovementDirectionType.SouthEast;
            }

            if (trueDegrees > 154 && trueDegrees < 199)
            {
                if (inclineGrade > 0)
                {
                    return MovementDirectionType.UpSouth;
                }
                else if (inclineGrade < 0)
                {
                    return MovementDirectionType.DownSouth;
                }

                return MovementDirectionType.South;
            }

            if (trueDegrees > 198 && trueDegrees < 243)
            {
                if (inclineGrade > 0)
                {
                    return MovementDirectionType.UpSouthWest;
                }
                else if (inclineGrade < 0)
                {
                    return MovementDirectionType.DownSouthWest;
                }

                return MovementDirectionType.SouthWest;
            }

            if (trueDegrees > 242 && trueDegrees < 287)
            {
                if (inclineGrade > 0)
                {
                    return MovementDirectionType.UpWest;
                }
                else if (inclineGrade < 0)
                {
                    return MovementDirectionType.DownWest;
                }

                return MovementDirectionType.West;
            }

            if (trueDegrees > 286 && trueDegrees < 331)
            {
                if (inclineGrade > 0)
                {
                    return MovementDirectionType.UpNorthWest;
                }
                else if (inclineGrade < 0)
                {
                    return MovementDirectionType.DownNorthWest;
                }

                return MovementDirectionType.NorthWest;
            }

            if (inclineGrade > 0)
            {
                return MovementDirectionType.UpNorth;
            }
            else if (inclineGrade < 0)
            {
                return MovementDirectionType.DownNorth;
            }

            return MovementDirectionType.North;
        }

        public static double GetBaseInclineGrade(MovementDirectionType direction)
        {
            return direction switch
            {
                MovementDirectionType.Up => 1,
                MovementDirectionType.Down => -1,
                MovementDirectionType.UpEast or MovementDirectionType.UpNorth or MovementDirectionType.UpNorthEast or MovementDirectionType.UpNorthWest or MovementDirectionType.UpSouth or MovementDirectionType.UpSouthEast or MovementDirectionType.UpSouthWest or MovementDirectionType.UpWest => 0.5,
                MovementDirectionType.DownEast or MovementDirectionType.DownNorth or MovementDirectionType.DownNorthEast or MovementDirectionType.DownNorthWest or MovementDirectionType.DownSouth or MovementDirectionType.DownSouthEast or MovementDirectionType.DownSouthWest or MovementDirectionType.DownWest => -0.5,
                _ => 0,
            };
        }

        /// <summary>
        /// Translates direction words into degreesFromNorth and inclineGrade for pathways, returns the absolute default value
        /// </summary>
        /// <param name="direction">the value to translate</param>
        /// <returns>degrees from north, incline grade</returns>
        public static Tuple<int, int> TranslateDirectionToDegrees(MovementDirectionType direction)
        {
            return direction switch
            {
                MovementDirectionType.East => new Tuple<int, int>(90, 0),
                MovementDirectionType.North => new Tuple<int, int>(0, 0),
                MovementDirectionType.NorthEast => new Tuple<int, int>(45, 0),
                MovementDirectionType.NorthWest => new Tuple<int, int>(315, 0),
                MovementDirectionType.South => new Tuple<int, int>(180, 0),
                MovementDirectionType.SouthEast => new Tuple<int, int>(135, 0),
                MovementDirectionType.SouthWest => new Tuple<int, int>(225, 0),
                MovementDirectionType.West => new Tuple<int, int>(270, 0),
                MovementDirectionType.Up => new Tuple<int, int>(-1, 25),
                MovementDirectionType.Down => new Tuple<int, int>(-1, -25),
                MovementDirectionType.UpEast => new Tuple<int, int>(90, 23),
                MovementDirectionType.UpNorth => new Tuple<int, int>(0, 25),
                MovementDirectionType.UpNorthEast => new Tuple<int, int>(45, 25),
                MovementDirectionType.UpNorthWest => new Tuple<int, int>(315, 25),
                MovementDirectionType.UpSouth => new Tuple<int, int>(180, 25),
                MovementDirectionType.UpSouthEast => new Tuple<int, int>(135, 25),
                MovementDirectionType.UpSouthWest => new Tuple<int, int>(225, 25),
                MovementDirectionType.UpWest => new Tuple<int, int>(270, 25),
                MovementDirectionType.DownEast => new Tuple<int, int>(90, -25),
                MovementDirectionType.DownNorth => new Tuple<int, int>(0, -25),
                MovementDirectionType.DownNorthEast => new Tuple<int, int>(45, -25),
                MovementDirectionType.DownNorthWest => new Tuple<int, int>(315, -25),
                MovementDirectionType.DownSouth => new Tuple<int, int>(180, -25),
                MovementDirectionType.DownSouthEast => new Tuple<int, int>(135, -25),
                MovementDirectionType.DownSouthWest => new Tuple<int, int>(225, -25),
                MovementDirectionType.DownWest => new Tuple<int, int>(270, -25),
                //return none, neutral for anything not counted
                _ => new Tuple<int, int>(-1, 0),
            };
        }

        public static int ReverseDirection(int degrees)
        {
            if (degrees < 0)
            {
                return -1;
            }

            return degrees < 180 ? degrees + 180 : degrees - 180;
        }

        public static MovementDirectionType ReverseDirection(MovementDirectionType direction)
        {
            return direction switch
            {
                MovementDirectionType.East => MovementDirectionType.West,
                MovementDirectionType.North => MovementDirectionType.South,
                MovementDirectionType.NorthEast => MovementDirectionType.SouthWest,
                MovementDirectionType.NorthWest => MovementDirectionType.SouthEast,
                MovementDirectionType.South => MovementDirectionType.North,
                MovementDirectionType.SouthEast => MovementDirectionType.NorthWest,
                MovementDirectionType.SouthWest => MovementDirectionType.NorthEast,
                MovementDirectionType.West => MovementDirectionType.East,
                MovementDirectionType.Up => MovementDirectionType.Down,
                MovementDirectionType.Down => MovementDirectionType.Up,
                MovementDirectionType.UpEast => MovementDirectionType.DownWest,
                MovementDirectionType.UpNorth => MovementDirectionType.DownSouth,
                MovementDirectionType.UpNorthEast => MovementDirectionType.DownSouthWest,
                MovementDirectionType.UpNorthWest => MovementDirectionType.DownSouthEast,
                MovementDirectionType.UpSouth => MovementDirectionType.DownNorth,
                MovementDirectionType.UpSouthEast => MovementDirectionType.DownNorthWest,
                MovementDirectionType.UpSouthWest => MovementDirectionType.DownNorthEast,
                MovementDirectionType.UpWest => MovementDirectionType.DownEast,
                MovementDirectionType.DownEast => MovementDirectionType.UpWest,
                MovementDirectionType.DownNorth => MovementDirectionType.UpSouth,
                MovementDirectionType.DownNorthEast => MovementDirectionType.UpSouthWest,
                MovementDirectionType.DownNorthWest => MovementDirectionType.UpSouthEast,
                MovementDirectionType.DownSouth => MovementDirectionType.UpNorth,
                MovementDirectionType.DownSouthEast => MovementDirectionType.UpNorthWest,
                MovementDirectionType.DownSouthWest => MovementDirectionType.UpNorthEast,
                MovementDirectionType.DownWest => MovementDirectionType.UpEast,
                //return none, neutral for anything not counted
                _ => MovementDirectionType.None,
            };
        }

        /// <summary>
        /// Translates hard directions to ascii characters. UP inclines are always brackets open to the left, DOWN is always bracket open to the right
        /// </summary>
        /// <param name="direction">the hard direction to turn into a character</param>
        /// <returns>a single ascii character in a string</returns>
        public static string TranslateDirectionToAsciiCharacter(MovementDirectionType direction)
        {
            return direction switch
            {
                MovementDirectionType.West or MovementDirectionType.East => "-",
                MovementDirectionType.South or MovementDirectionType.North => "|",
                MovementDirectionType.SouthWest or MovementDirectionType.NorthEast => "/",
                MovementDirectionType.SouthEast or MovementDirectionType.NorthWest => @"\",
                MovementDirectionType.Up => "^",
                MovementDirectionType.UpEast or MovementDirectionType.UpWest => "3",
                MovementDirectionType.UpSouth or MovementDirectionType.UpNorth => "}",
                MovementDirectionType.UpSouthWest or MovementDirectionType.UpNorthEast => ">",
                MovementDirectionType.UpSouthEast or MovementDirectionType.UpNorthWest => "]",
                MovementDirectionType.Down => "v",
                MovementDirectionType.DownEast or MovementDirectionType.DownWest => "E",
                MovementDirectionType.DownSouth or MovementDirectionType.DownNorth => "{",
                MovementDirectionType.DownSouthWest or MovementDirectionType.DownNorthEast => "<",
                MovementDirectionType.DownSouthEast or MovementDirectionType.DownNorthWest => "[",
                _ => "#",
            };
        }

        /// <summary>
        /// X, Y, Z
        /// </summary>
        /// <param name="transversalDirection">The direction being faced</param>
        /// <returns>the coordinates for the direction needed to move one unit "forward"</returns>
        public static Tuple<int, int, int> GetDirectionStep(MovementDirectionType transversalDirection)
        {
            switch (transversalDirection)
            {
                default: //We already defaulted to 0,0,0
                    break;
                case MovementDirectionType.East:
                    return new Tuple<int, int, int>(1, 0, 0);
                case MovementDirectionType.North:
                    return new Tuple<int, int, int>(0, 1, 0);
                case MovementDirectionType.NorthEast:
                    return new Tuple<int, int, int>(1, 1, 0);
                case MovementDirectionType.NorthWest:
                    return new Tuple<int, int, int>(-1, 1, 0);
                case MovementDirectionType.South:
                    return new Tuple<int, int, int>(0, -1, 0);
                case MovementDirectionType.SouthEast:
                    return new Tuple<int, int, int>(1, -1, 0);
                case MovementDirectionType.SouthWest:
                    return new Tuple<int, int, int>(-1, -1, 0);
                case MovementDirectionType.West:
                    return new Tuple<int, int, int>(-1, 0, 0);
                case MovementDirectionType.Up:
                    return new Tuple<int, int, int>(0, 0, 1);
                case MovementDirectionType.Down:
                    return new Tuple<int, int, int>(0, 0, -1);
                case MovementDirectionType.UpEast:
                    return new Tuple<int, int, int>(1, 0, 1);
                case MovementDirectionType.UpNorth:
                    return new Tuple<int, int, int>(0, 1, 1);
                case MovementDirectionType.UpNorthEast:
                    return new Tuple<int, int, int>(1, 1, 1);
                case MovementDirectionType.UpNorthWest:
                    return new Tuple<int, int, int>(-1, 1, 1);
                case MovementDirectionType.UpSouth:
                    return new Tuple<int, int, int>(0, -1, 1);
                case MovementDirectionType.UpSouthEast:
                    return new Tuple<int, int, int>(1, -1, 1);
                case MovementDirectionType.UpSouthWest:
                    return new Tuple<int, int, int>(-1, -1, 1);
                case MovementDirectionType.UpWest:
                    return new Tuple<int, int, int>(-1, 0, 1);
                case MovementDirectionType.DownEast:
                    return new Tuple<int, int, int>(1, 0, -1);
                case MovementDirectionType.DownNorth:
                    return new Tuple<int, int, int>(0, 1, -1);
                case MovementDirectionType.DownNorthEast:
                    return new Tuple<int, int, int>(1, 1, -1);
                case MovementDirectionType.DownNorthWest:
                    return new Tuple<int, int, int>(-1, 1, -1);
                case MovementDirectionType.DownSouth:
                    return new Tuple<int, int, int>(0, -1, -1);
                case MovementDirectionType.DownSouthEast:
                    return new Tuple<int, int, int>(1, -1, -1);
                case MovementDirectionType.DownSouthWest:
                    return new Tuple<int, int, int>(-1, -1, -1);
                case MovementDirectionType.DownWest:
                    return new Tuple<int, int, int>(-1, 0, -1);
            }

            return new Tuple<int, int, int>(0, 0, 0);
        }
    }
}
