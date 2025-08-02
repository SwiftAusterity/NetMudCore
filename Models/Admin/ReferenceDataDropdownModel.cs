using NetMudCore.DataStructure.Architectural;

namespace NetMudCore.Models.Admin
{
    public class ReferenceDataDropdownModel(string controlName, string label, IEnumerable<IKeyedData> validItemList, long selectedItemId)
    {
        public string ControlName { get; set; } = controlName;
        public string Label { get; set; } = label;
        public IEnumerable<IKeyedData> ValidList { get; set; } = validItemList;
        public long SelectedItemId { get; set; } = selectedItemId;
    }
}