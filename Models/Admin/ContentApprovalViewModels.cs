using NetMudCore.DataStructure.Architectural;

namespace NetMudCore.Models.Admin
{
    public class ManageContentApprovalsViewModel : PagedDataModel<IKeyedData>
    {
        public ManageContentApprovalsViewModel(IEnumerable<IKeyedData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IKeyedData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IKeyedData, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IKeyedData, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

}