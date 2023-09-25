﻿using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Locale;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.Zone;
using System;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Architectural
{
    /// <summary>
    /// An entity's position in the world
    /// </summary>
    [Serializable]
    public class GlobalPosition : IGlobalPosition
    {
        /// <summary>
        /// Birthmark for current live location of this
        /// </summary>
        [JsonPropertyName("CurrentZone")]
        private LiveCacheKey _currentZone { get; set; }

        /// <summary>
        /// The actual container that the current location is
        /// </summary>
        [JsonIgnore]

        public IZone CurrentZone
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_currentZone?.BirthMark))
                {
                    return LiveCache.Get<IZone>(_currentZone);
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _currentZone = new LiveCacheKey(value);
            }
        }

        /// <summary>
        /// Birthmark for current live location of this
        /// </summary>
        [JsonPropertyName("CurrentLocale")]
        private LiveCacheKey _currentLocale { get; set; }

        /// <summary>
        /// The actual container that the current location is
        /// </summary>
        [JsonIgnore]

        public ILocale CurrentLocale
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_currentLocale?.BirthMark))
                {
                    return LiveCache.Get<ILocale>(_currentLocale);
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _currentLocale = new LiveCacheKey(value);
            }
        }

        /// <summary>
        /// Birthmark for current live location of this
        /// </summary>
        [JsonPropertyName("CurrentRoom")]
        private LiveCacheKey _currentRoom { get; set; }

        /// <summary>
        /// The actual container that the current location is
        /// </summary>
        [JsonIgnore]

        public IRoom CurrentRoom
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_currentRoom?.BirthMark))
                {
                    return LiveCache.Get<IRoom>(_currentRoom);
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _currentRoom = new LiveCacheKey(value);
            }
        }

        /// <summary>
        /// Birthmark for current live location of this
        /// </summary>
        [JsonPropertyName("CurrentContainer")]
        private LiveCacheKey _currentContainer { get; set; }

        /// <summary>
        /// The actual container that the current location is
        /// </summary>
        [JsonIgnore]

        public IContains CurrentContainer
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_currentContainer?.BirthMark))
                {
                    return (IContains)LiveCache.Get(_currentContainer);
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _currentContainer = new LiveCacheKey(value);
            }
        }

        /// <summary>
        /// This constructor is required for the JSON deserializer to be able
        /// to identify concrete classes to use when deserializing the interface properties.
        /// </summary>
        [JsonConstructor]
        public GlobalPosition(LiveCacheKey currentZone, LiveCacheKey currentContainer)
        {
            _currentZone = currentZone;
            _currentContainer = currentContainer;
        }

        /// <summary>
        /// Construct with the zone to set as the location
        /// </summary>
        /// <param name="currentLocation">the container</param>
        public GlobalPosition(IZone currentLocation)
        {
            CurrentZone = currentLocation;
        }

        /// <summary>
        /// Construct with the zone, locale, room to set as the location
        /// </summary>
        /// <param name="currentZone"></param>
        /// <param name="currentLocale"></param>
        public GlobalPosition(IZone currentZone, ILocale currentLocale)
        {
            CurrentZone = currentZone;
            CurrentLocale = currentLocale;
        }

        /// <summary>
        /// Construct with the zone, locale, room to set as the location
        /// </summary>
        /// <param name="currentZone"></param>
        /// <param name="currentLocale"></param>
        /// <param name="currentRoom"></param>
        public GlobalPosition(IZone currentZone, ILocale currentLocale, IRoom currentRoom)
        {
            CurrentZone = currentZone;
            CurrentLocale = currentLocale;
            CurrentRoom = currentRoom;
        }

        /// <summary>
        /// Construct with the container to set as the location
        /// </summary>
        /// <param name="currentContainer">the container</param>
        public GlobalPosition(IContains currentContainer)
        {
            CurrentContainer = currentContainer;
            CurrentZone = currentContainer.CurrentLocation?.CurrentZone;
            CurrentLocale = currentContainer.CurrentLocation?.CurrentLocale;
            CurrentRoom = currentContainer.CurrentLocation?.CurrentRoom;
        }

        /// <summary>
        /// Get the absolute lowest level thing this other thing is in
        /// </summary>
        /// <returns></returns>
        public ILocation CurrentLocation()
        {
            if (CurrentContainer == null)
            {
                if(CurrentRoom == null)
                {
                    return CurrentZone;
                }

                return CurrentRoom;
            }

            return (ILocation)CurrentContainer;
        }

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        public string MoveFrom(IEntity thing)
        {
            if (CurrentContainer != null)
            {
                return CurrentContainer.MoveFrom(thing);
            }
            else if (CurrentZone != null)
            {
                thing.CurrentLocation.CurrentZone = null;
                thing.CurrentLocation.CurrentContainer = null;
                thing.CurrentLocation.CurrentLocale = null;
                thing.CurrentLocation.CurrentRoom = null;
            }

            return string.Empty;
        }


        /// <summary>
        /// Move an entity into of this
        /// </summary>
        /// <typeparam name="T">the type of entity to move</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        public string MoveInto(IEntity thing)
        {
            if (CurrentContainer != null)
            {
                return CurrentContainer.MoveInto(thing);
            }
            else if (CurrentZone != null)
            {
                thing.TryMoveTo(this);
            }

            return string.Empty;
        }

        /// <summary>
        /// make a copy of this
        /// </summary>
        /// <returns>a clone of this</returns>
        public object Clone()
        {
            return new GlobalPosition(CurrentZone, CurrentLocale, CurrentRoom) { CurrentContainer = CurrentContainer };
        }
    }
}
