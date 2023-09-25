﻿namespace NetMudCore.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// entity can recieve visible type notification messages and triggers
    /// </summary>
    public interface ICanSee
    {
        /// <summary>
        /// Gets the actual vision modifier taking into account blindness and other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        ValueRange<float> GetVisualRange();
    }
}
