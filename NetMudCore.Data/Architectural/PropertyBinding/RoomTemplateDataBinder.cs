using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Room;

namespace NetMudCore.Data.Architectural.PropertyBinding
{
    public class RoomTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<IRoomTemplate>(long.Parse(stringInput));
        }
    }
}
