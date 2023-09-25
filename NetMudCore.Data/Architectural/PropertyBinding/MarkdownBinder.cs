using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.PropertyBinding;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class MarkdownBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return new MarkdownString(stringInput);
        }
    }
}
