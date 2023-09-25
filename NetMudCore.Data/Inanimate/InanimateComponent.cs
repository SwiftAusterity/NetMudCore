using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Inanimate;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Inanimate
{
    [Serializable]
    public class InanimateComponent : IInanimateComponent
    {
        [JsonPropertyName("Item")]
        public TemplateCacheKey _item { get; set; }

        [JsonIgnore]

        [Display(Name = "Component", Description = "The object of the collection.")]
        [UIHint("InanimateTemplateList")]
        [InanimateTemplateDataBinder]
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
                _item = new TemplateCacheKey(value);
            }
        }

        [Display(Name = "Amount", Description = "Amount of the component item in the collection.")]
        [DataType(DataType.Text)]
        public int Amount { get; set; }

        public InanimateComponent()
        {
            Amount = 0;
        }

        public InanimateComponent(IInanimateTemplate item, int amount)
        {
            Amount = amount;
            Item = item;
        }

        public override string ToString()
        {
            return string.Format("{0} of {1}", Amount, Item.Name);
        }
    }
}
