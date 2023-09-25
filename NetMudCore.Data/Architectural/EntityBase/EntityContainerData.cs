﻿using NetMudCore.DataStructure.Architectural.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Data.Architectural.EntityBase
{
    /// <summary>
    /// Framework for storage/retrieval/management of entity containers in backing data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class EntityContainerData<T> : IEntityContainerData<T> where T : IEntity
    {
        /// <summary>
        /// How large is this container
        /// </summary>
        [Display(Name = "Volume", Description = "The size of the container.")]
        [DataType(DataType.Text)]
        public long CapacityVolume { get; set; }

        /// <summary>
        /// How much weight can it carry before taking damage
        /// </summary>
        [Display(Name = "Weight", Description = "How much weight the container can hold.")]
        [DataType(DataType.Text)]
        public long CapacityWeight { get; set; }

        /// <summary>
        /// The name of the container; can be string empty without issue
        /// </summary>
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The descriptive name used to refer to this.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// Instansiate this empty
        /// </summary>
        public EntityContainerData()
        {
            CapacityVolume = -1;
            CapacityWeight = -1;
            Name = "NotImpl";
        }

        /// <summary>
        /// Instansiate this with parameters
        /// </summary>
        /// <param name="capacityVolume">How large is this container</param>
        /// <param name="capacityWeight">How much weight can it carry before taking damage</param>
        /// <param name="name"> The name of the container; can be string empty without issue</param>
        public EntityContainerData(long capacityVolume, long capacityWeight, string name)
        {
            CapacityVolume = capacityVolume;
            CapacityWeight = capacityWeight;
            Name = name;
        }

        /// <summary>
        /// Will an entity fit inside
        /// </summary>
        /// <param name="entity">the entity you want to cram in</param>
        /// <returns>does it fit (true) or not (false)</returns>
        public bool WillItFit(T entity)
        {
            //-1 volume means infinite
            if (CapacityVolume < 0)
            {
                return true;
            }

            //TODO: Entity dimensions
            return true;
        }
    }
}
