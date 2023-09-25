using NetMudCore.DataStructure.NPC;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.DataStructure.Architectural.PropertyValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MerchandiseValidator : ValidationAttribute
    {
        public bool Optional { get; set; }

        public MerchandiseValidator() : base()
        {
            Optional = false;
        }

        public override bool IsValid(object? value)
        {
            if (value is not IMerchandise item)
            {
                return !Optional;
            }

            return item.Item != null && item.QualityRange.Low <= item.QualityRange.High;
        }
    }
}