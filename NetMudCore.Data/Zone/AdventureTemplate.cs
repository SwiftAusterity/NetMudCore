using NetMudCore.Data.Architectural;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Zone;

namespace NetMudCore.Data.Zone
{
    public class AdventureTemplate : LookupDataPartial, IAdventureTemplate
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

    }
}
