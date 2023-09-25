﻿namespace NetMudCore.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Encapsulates position in the world
    /// </summary>
    public interface IExist : IHavePositioning, ILookable, IInspectable, IHaveInfo, IHaveQualities
    {
        /// <summary>
        /// Spawns a new instance of this entity in the live world into a default position
        /// </summary>
        void SpawnNewInWorld();

        /// <summary>
        /// Spawn a new instance of this entity into the live world in a set position
        /// </summary>
        /// <param name="position">container and zone to spawn to</param>
        void SpawnNewInWorld(IGlobalPosition position);

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        float GetCurrentLuminosity();
    }
}
