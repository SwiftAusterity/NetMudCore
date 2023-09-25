﻿namespace NetMudCore.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Entity can breathe (for mobiles) in specified medium
    /// </summary>
    /// <typeparam name="IMaterial">The specified medium the entity can breathe in</typeparam>
    public interface ICanBreathe
    {
        /// <summary>
        /// Types of respiration this can perform
        /// </summary>
        IEnumerable<RespiratoryType> RespirationTypes { get; set; }
    }
}
