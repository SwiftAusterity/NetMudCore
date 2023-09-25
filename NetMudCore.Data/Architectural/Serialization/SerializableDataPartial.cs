using NetMudCore.DataStructure.Architectural;
using NetMudCore.Utility;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Architectural.Serialization
{
    public abstract class SerializableDataPartial : IFileStored
    {
        /// <summary>
        /// Serialize this live entity to a json string
        /// </summary>
        /// <returns>json string</returns>
        public virtual string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// Deserialize a json string into this entity
        /// </summary>
        /// <param name="jsonData">string to deserialize</param>
        /// <returns>the entity</returns>
        public virtual IFileStored DeSerialize(string jsonData)
        {
            return JsonSerializer.Deserialize(jsonData, GetType()) as IFileStored;
        }

        /// <summary>
        /// Serialize this live entity to a binary stream
        /// </summary>
        /// <returns>binary stream</returns>
        public virtual byte[] ToBytes()
        {
            return Encoding.Unicode.GetBytes(Serialize());
        }

        private static string GetStringFromBytes(byte[] bytes)
        {
            string returnString = Encoding.ASCII.GetString(bytes);

            if (string.IsNullOrWhiteSpace(returnString) || returnString.Substring(1, 1) == "\0")
            {
                return Encoding.Unicode.GetString(bytes);
            }

            return returnString;
        }

        /// <summary>
        /// Deserialize a binary stream into this entity
        /// </summary>
        /// <param name="bytes">binary to deserialize</param>
        /// <returns>the entity</returns>
        public virtual IFileStored FromBytes(byte[] bytes)
        {
            string strData = GetStringFromBytes(bytes);

            IFileStored obj = DeSerialize(strData);

            //Finds containers and inits them to empty after this thing is deserialized
            foreach (PropertyInfo container in obj.GetType().GetProperties())
            {
                if (!container.PropertyType.GetCustomAttributes<JsonIgnoreAttribute>().Any()
                    && (container.PropertyType.IsArray || (!typeof(string).Equals(container.PropertyType) && typeof(IEnumerable).IsAssignableFrom(container.PropertyType)))
                    && container.GetValue(obj) == null)
                {
                    try
                    {
                        container.SetValue(obj, Activator.CreateInstance(container.PropertyType, Array.Empty<object>()));
                    }
                    catch
                    {
                        //Oh well, can't init this on deserialization that's ok mostly
                    }
                }
            }

            return obj;
        }
    }
}
