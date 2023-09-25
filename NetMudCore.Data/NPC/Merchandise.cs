using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.PropertyValidation;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.NPC;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.NPC
{
    /// <summary>
    /// Criteria for buying and selling merchandise
    /// </summary>
    [Serializable]
    public class Merchandise : IMerchandise
    {
        [JsonPropertyName("Item")]
        private TemplateCacheKey _item { get; set; }

        /// <summary>
        /// Item type
        /// </summary>

        [JsonIgnore]
        [InanimateTemplateDataBinder]
        [Display(Name = "Item", Description = "The item in question.")]
        [UIHint("InanimateTemplateList")]
        [Required]
        public IInanimateTemplate Item
        {
            get
            {
                if (_item == null)
                {
                    return null;
                }

                return TemplateCache.Get<IInanimateTemplate>(_item);
            }
            set
            {
                if (value != null)
                {
                    _item = new TemplateCacheKey(value);
                }
            }
        }

        /// <summary>
        /// Required quality
        /// </summary>
        [Display(Name = "Quality", Description = "The required quality for the item type.")]
        [DataType(DataType.Text)]
        public string Quality { get; set; }

        /// <summary>
        /// Range for the quality
        /// </summary>
        [Display(Name = "Quality Range", Description = "The value for the required quality.")]
        [UIHint("ValueRangeInt")]
        [IntValueRangeValidator(Optional = true)]
        public ValueRange<int> QualityRange { get; set; }

        /// <summary>
        /// Markup or discount for buying/selling. 1 would be no markup/discount, below 1 would be discount
        /// </summary>
        [Display(Name = "Mark-rate", Description = "The markup (above 1) or discount (below 1) to apply to buying or selling this item.")]
        [DataType(DataType.Text)]
        public decimal MarkRate { get; set; }
    }
}
