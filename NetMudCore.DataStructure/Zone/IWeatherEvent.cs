﻿using NetMudCore.DataStructure.Architectural.EntityBase;

namespace NetMudCore.DataStructure.Zone
{
    /// <summary>
    /// Individual weather events
    /// </summary>
    public interface IWeatherEvent : ILookable, IVisible, ITouchable, ISmellable, IAudible, IRenderInLocation
    {
        /// <summary>
        /// The event type
        /// </summary>
        WeatherEventType Type { get; set; }

        /// <summary>
        /// The strength of the event (size of cloud, windspeed for cyclones)
        /// </summary>
        float Strength { get; set; }

        /// <summary>
        /// How much strength does this bleed per cycle, important for cyclones and typhoons and for earthquake aftershocks
        /// </summary>
        float Drain { get; set; }

        /// <summary>
        /// How high up is the event
        /// </summary>
        float Altitude { get; set; }

        /// <summary>
        /// How much does this obscure the sky, Percentage
        /// </summary>
        float Coverage { get; set; }

        /// <summary>
        /// How much precipitation does this produce per cycle?
        /// </summary>
        float PrecipitationAmount { get; set; }
    }
}
