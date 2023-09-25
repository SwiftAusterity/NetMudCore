using System.ComponentModel.DataAnnotations;

namespace NetMudCore.DataStructure.Architectural.PropertyValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ShortValueRangeValidator : ValidationAttribute
    {
        public bool Optional { get; set; }

        public ShortValueRangeValidator() : base()
        {
            Optional = false;
        }

        public override bool IsValid(object? value)
        {
            if (value is not ValueRange<short> item)
            {
                return !Optional;
            }

            return item.Low <= item.High;
        }
    }
}