using NetMudCore.Authentication;
using NetMudCore.DataStructure.Combat;

namespace NetMudCore.Models.Admin
{
    public class ManageFightingArtViewModel : PagedDataModel<IFightingArt>
    {
        public ManageFightingArtViewModel(IEnumerable<IFightingArt> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IFightingArt, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IFightingArt, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }

        internal override Func<IFightingArt, object> OrderSecondary
        {
            get
            {
                return item => item.Name;
            }
        }
    }

    public class AddEditFightingArtViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public AddEditFightingArtViewModel()
        {
        }

        public IFightingArt DataObject { get; set; }
    }
}