using NetMudCore.DataStructure.Architectural.ActorBase;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.DataStructure.Architectural.PropertyValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RaceValidator : ValidationAttribute
    {
        public bool Optional { get; set; }

        public RaceValidator() : base()
        {
            Optional = false;
        }

        public override bool IsValid(object? value)
        {
            if (value is not IRace item)
            {
                return !Optional;
            }

            return !string.IsNullOrWhiteSpace(item.CollectiveNoun) 
                && !string.IsNullOrWhiteSpace(item.Name);
        }
    }
}