﻿using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Gaia;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Gaia
{
    /// <summary>
    /// Where the various celestial bodies are along their paths
    /// </summary>
    [Serializable]
    public class CelestialPosition : ICelestialPosition
    {
        [JsonPropertyName("CelestialObject")]
        public TemplateCacheKey _celestialObject { get; set; }

        /// <summary>
        /// The celestial object
        /// </summary>

        [JsonIgnore]
        [Display(Name = "CelestialObject", Description = "The celestial object.")]
        [UIHint("CelestialList")]
        public ICelestial CelestialObject
        {
            get
            {
                if (_celestialObject == null)
                {
                    return null;
                }

                return TemplateCache.Get<ICelestial>(_celestialObject);
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _celestialObject = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// Where the celestial object is in its orbit
        /// </summary>
        [Display(Name = "Position", Description = "Where the celestial object is in its orbit.")]
        [DataType(DataType.Text)]
        public float Position { get; set; }

        public CelestialPosition()
        {
            Position = 0;
        }

        public CelestialPosition(ICelestial celestialObject, float position)
        {
            CelestialObject = celestialObject;
            Position = position;
        }
    }
}
