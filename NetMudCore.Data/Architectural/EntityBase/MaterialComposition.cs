using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Architectural.EntityBase
{
    [Serializable]
    public class MaterialComposition : IMaterialComposition
    {
        /// <summary>
        /// how much of the alloy is this material (1 to 100)
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Percentage", Description = "How much of the alloy is this material (1 to 100).")]
        public short PercentageOfComposition { get; set; }

        [JsonPropertyName("Material")]
        public TemplateCacheKey _material { get; set; }

        /// <summary>
        /// The material it's made of
        /// </summary>

        [JsonIgnore]
        [UIHint("MaterialList")]
        [Display(Name = "Material", Description = "The material it's made of.")]
        [MaterialDataBinder]
        public IMaterial Material
        {
            get
            {
                if (_material != null)
                {
                    return TemplateCache.Get<IMaterial>(_material);
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _material = new TemplateCacheKey(value);
            }
        }
    }
}
