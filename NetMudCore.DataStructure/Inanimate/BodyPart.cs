using System.ComponentModel.DataAnnotations;

namespace NetMudCore.DataStructure.Inanimate
{
    [Serializable]
    public class BodyPart(IInanimateComponent item, string name)
    {
        [UIHint("IndividualInanimateComponent")]
        [Display(Name = "Body Part", Description = "The # of and object the part is made up of.")]
        public IInanimateComponent Part { get; set; } = item;

        [DataType(DataType.Text)]
        [Display(Name = "Name", Description = "The name of the body part.")]
        public string Name { get; set; } = name;
    }
}
