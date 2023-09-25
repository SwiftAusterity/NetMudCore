﻿using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Architectural.EntityBase
{
    /// <summary>
    /// Object that handles any and all "this contains entities" for the system
    /// </summary>
    /// <typeparam name="T">the type of entities it can contain</typeparam>
    public class EntityContainer<T> : IEntityContainer<T> where T : IEntity
    {
        private const string genericCollectionLabel = "*VoidContainer*";
        /// <summary>
        /// What this actually contains, yeah it's a hashtable of hashtables but whatever
        /// </summary>
        private Dictionary<string, HashSet<LiveCacheKey>> Birthmarks { get; set; }

        /// <summary>
        /// What named containers are attached to this
        /// </summary>
        public IEnumerable<IEntityContainerData<T>> NamedContainers { get; set; }

        /// <summary>
        /// New up an empty container
        /// </summary>
        public EntityContainer()
        {
            NamedContainers = Enumerable.Empty<IEntityContainerData<T>>();
            Birthmarks = new Dictionary<string, HashSet<LiveCacheKey>>
            {
                { genericCollectionLabel, new HashSet<LiveCacheKey>() }
            };
        }

        /// <summary>
        /// New up an empty container with the named containers it should have
        /// </summary>
        public EntityContainer(IEnumerable<IEntityContainerData<T>> namedContainers)
        {
            NamedContainers = namedContainers;
            Birthmarks = new Dictionary<string, HashSet<LiveCacheKey>>
            {
                { genericCollectionLabel, new HashSet<LiveCacheKey>() }
            };

            foreach (IEntityContainerData<T> container in namedContainers)
            {
                Birthmarks.Add(container.Name, new HashSet<LiveCacheKey>());
            }
        }


        /// <summary>
        /// New up an empty container
        /// </summary>
        [JsonConstructor]
        public EntityContainer(IEnumerable<EntityContainerData<T>> namedContainers)
        {
            NamedContainers = namedContainers;
            Birthmarks = new Dictionary<string, HashSet<LiveCacheKey>>
            {
                { genericCollectionLabel, new HashSet<LiveCacheKey>() }
            };

            foreach (EntityContainerData<T> container in namedContainers)
            {
                Birthmarks.Add(container.Name, new HashSet<LiveCacheKey>());
            }
        }

        #region Universal Accessors
        /// <summary>
        /// List of entities contained sent back with which container they are in
        /// </summary>
        /// <returns>entities paired with their container names</returns>
        public IEnumerable<Tuple<string, T>> EntitiesContainedByName()
        {
            if (Count() > 0)
            {
                List<Tuple<string, T>> returnList = new();

                foreach (KeyValuePair<string, HashSet<LiveCacheKey>> hashKeySet in Birthmarks)
                {
                    foreach (T value in LiveCache.GetMany<T>(hashKeySet.Value))
                    {
                        returnList.Add(new Tuple<string, T>(hashKeySet.Key, value));
                    }
                }

                return returnList;
            }

            return Enumerable.Empty<Tuple<string, T>>();
        }

        /// <summary>
        /// Restful list of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        public IEnumerable<T> EntitiesContained()
        {
            if (Count() > 0)
            {
                return LiveCache.GetMany<T>(Birthmarks.Values.SelectMany(hs => hs));
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Add an entity to this
        /// </summary>
        /// <param name="entity">the entity to add</param>
        /// <returns>success status</returns>
        public bool Add(T entity)
        {
            LiveCacheKey newKey = new(entity);
            if (Birthmarks[genericCollectionLabel].Any(mark => mark.KeyHash().Equals(newKey.KeyHash())))
            {
                return false;
            }

            return Birthmarks[genericCollectionLabel].Add(new LiveCacheKey(entity));
        }

        /// <summary>
        /// Does this contain the specified entity
        /// </summary>
        /// <param name="entity">the entity in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        public bool Contains(T entity)
        {
            LiveCacheKey newKey = new(entity);
            return Birthmarks.Values.Any(hs => hs.Any(mark => mark.KeyHash().Equals(newKey.KeyHash())));
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <returns>success status</returns>
        public bool Remove(T entity)
        {
            LiveCacheKey newKey = new(entity);
            if (!Birthmarks[genericCollectionLabel].Any(mark => mark.KeyHash().Equals(newKey.KeyHash())))
            {
                return false;
            }

            return Birthmarks[genericCollectionLabel].RemoveWhere(key => key.BirthMark == newKey.BirthMark) > 0;
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="birthMark">the entity's birthmark to remove</param>
        /// <returns>success status</returns>
        public bool Remove(ICacheKey cacheKey)
        {
            LiveCacheKey newKey = (LiveCacheKey)cacheKey;

            if (!Birthmarks[genericCollectionLabel].Any(mark => mark.KeyHash().Equals(newKey.KeyHash())))
            {
                return false;
            }

            return Birthmarks[genericCollectionLabel].RemoveWhere(key => key.BirthMark == newKey.BirthMark) > 0;
        }

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        public int Count()
        {
            return Birthmarks.Values.Sum(hs => hs.Count);
        }
        #endregion

        #region Named Containers
        /// <summary>
        /// Restful list of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        public IEnumerable<T> EntitiesContained(string namedContainer)
        {
            if (string.IsNullOrWhiteSpace(namedContainer))
            {
                return EntitiesContained();
            }

            if (Count(namedContainer) > 0)
            {
                return LiveCache.GetMany<T>(Birthmarks[namedContainer]);
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Add an entity to this
        /// </summary>
        /// <param name="entity">the entity to add</param>
        /// <returns>success status</returns>
        public bool Add(T entity, string namedContainer)
        {
            if (string.IsNullOrWhiteSpace(namedContainer))
            {
                return Add(entity);
            }

            LiveCacheKey key = new(entity);

            if (Birthmarks[namedContainer].Any(mark => mark.KeyHash().Equals(key.KeyHash())))
            {
                return false;
            }

            return Birthmarks[namedContainer].Add(key);
        }

        /// <summary>
        /// Does this contain the specified entity
        /// </summary>
        /// <param name="entity">the entity in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        public bool Contains(T entity, string namedContainer)
        {
            if (string.IsNullOrWhiteSpace(namedContainer))
            {
                return Contains(entity);
            }

            LiveCacheKey key = new(entity);

            return Birthmarks[namedContainer].Any(mark => mark.KeyHash().Equals(key.KeyHash()));
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <returns>success status</returns>
        public bool Remove(T entity, string namedContainer)
        {
            if (string.IsNullOrWhiteSpace(namedContainer))
            {
                return Remove(entity);
            }

            LiveCacheKey key = new(entity);

            if (!Birthmarks[namedContainer].Any(mark => mark.KeyHash().Equals(key.KeyHash())))
            {
                return false;
            }

            return Birthmarks[namedContainer].RemoveWhere(keys => keys == key) > 0;
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="birthMark">the entity's birthmark to remove</param>
        /// <returns>success status</returns>
        public bool Remove(ICacheKey cacheKey, string namedContainer)
        {
            if (string.IsNullOrWhiteSpace(namedContainer))
            {
                return Remove(cacheKey);
            }

            LiveCacheKey key = (LiveCacheKey)cacheKey;

            if (!Birthmarks[namedContainer].Any(mark => mark.KeyHash().Equals(key.KeyHash())))
            {
                return false;
            }

            return Birthmarks[namedContainer].RemoveWhere(keys => keys == key) > 0;
        }

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        public int Count(string namedContainer)
        {
            if (string.IsNullOrWhiteSpace(namedContainer))
            {
                return Count();
            }

            return Birthmarks[namedContainer].Count;
        }
        #endregion
    }
}
