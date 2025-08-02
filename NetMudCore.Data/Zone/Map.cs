using NetMudCore.DataStructure.Zone;
using System;

namespace NetMudCore.Data.Zone
{
    /// <summary>
    /// A 3d matrix map of rooms
    /// </summary>
    [Serializable]
    public class Map(long[,,] coordinateMap, bool isPartial) : IMap
    {
        /// <summary>
        /// The map of room IDs
        /// </summary>
        public long[,,] CoordinatePlane { get; set; } = coordinateMap;

        /// <summary>
        /// Is this a partial map
        /// </summary>
        public bool Partial { get; private set; } = isPartial;

        /// <summary>
        /// Get a single flat plane of the main map at a specific zIndex
        /// </summary>
        /// <param name="zIndex">the Z (up/down) level to retrieve</param>
        /// <returns></returns>
        public long[,] GetSinglePlane(int zIndex)
        {
            return Cartography.Cartographer.GetSinglePlane(CoordinatePlane, zIndex);
        }
    }
}
