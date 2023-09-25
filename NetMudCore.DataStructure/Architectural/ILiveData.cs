﻿namespace NetMudCore.DataStructure.Architectural
{
    /// <summary>
    /// Signifies the object is cached in the livecache
    /// </summary>
    public interface ILiveData : ILiveInCache, IComparable<ILiveData>, IEquatable<ILiveData>, IEqualityComparer<ILiveData>, ICloneable
    {
        /// <summary>
        /// Indelible guid that helps the system figure out where stuff is, generated when the object is spawned into the world
        /// </summary>
        string BirthMark { get; set; }

        /// <summary>
        /// When this was first added to the live world
        /// </summary>
        DateTime Birthdate { get; set; }
    }
}
