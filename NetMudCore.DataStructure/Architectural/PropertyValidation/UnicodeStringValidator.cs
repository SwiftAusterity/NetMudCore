using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NetMudCore.DataStructure.Architectural.PropertyValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class UnicodeCharacterValidator : StringLengthAttribute
    {
        public bool Optional { get; set; }

        public UnicodeCharacterValidator() : base(1)
        {
            MinimumLength = 1;
            Optional = false;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return !Optional;
            }

            string? checkString = value.ToString();

            bool returnValue = true;

            byte[] bytes = Encoding.UTF32.GetBytes(checkString!);

            if (bytes.Length > 4)
            {
                returnValue = false;
            }

            return returnValue;
        }
    }
}