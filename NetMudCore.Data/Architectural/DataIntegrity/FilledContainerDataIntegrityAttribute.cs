﻿using System;
using System.Collections;

namespace NetMudCore.Data.Architectural.DataIntegrity
{
    /// <summary>
    /// Field must not be null and also have stuff in it
    /// </summary>
    /// <remarks>
    /// Creates an attribute
    /// </remarks>
    /// <param name="errorMessage">error to display when this fails the integrity check</param>
    /// <param name="warning">Not a required field but will display on the editor itself</param>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FilledContainerDataIntegrityAttribute(string errorMessage, int minimumCapacity = 1) : BaseDataIntegrity(errorMessage, true)
    {
        /// <summary>
        /// Lower value for range. Is a greater than not a greater or equals
        /// </summary>
        public int MinimumCapacity { get; private set; } = minimumCapacity;

        /// <summary>
        /// How to check against this result; returns true if it passes Longegrity
        /// </summary>
        internal override bool Verify(object val)
        {
            Type valueType = val.GetType();

            //return true on non-collections unless they're null
            if (!valueType.IsArray && (typeof(string).Equals(valueType) || !typeof(IEnumerable).IsAssignableFrom(valueType)))
            {
                return val != null;
            }

            try
            {
                //Check this has at least N elements, awkward way of doing this but we don't care what <type> the enumeration is containing
                int iterator = 1;
                IEnumerator valueContainer = ((IEnumerable)val).GetEnumerator();

                while (iterator <= MinimumCapacity)
                {
                    valueContainer.MoveNext();

                    if (valueContainer.Current == null)
                    {
                        return false;
                    }

                    iterator++;
                }
            }
            catch
            {
                //obvs failed, it was null or had nothing in it
                return false;
            }

            return true;
        }
    }
}
