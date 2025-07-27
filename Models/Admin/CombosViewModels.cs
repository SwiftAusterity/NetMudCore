using NetMudCore.Authentication;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Combat;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Admin
{
    public class ManageCombosViewModel : PagedDataModel<IFightingArtCombination>
    {
        public ManageCombosViewModel(IEnumerable<IFightingArtCombination> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IFightingArtCombination, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IFightingArtCombination, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IFightingArtCombination, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditCombosViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public AddEditCombosViewModel()
        {
            ValidArts = TemplateCache.GetAll<IFightingArt>(true);
        }

        public IEnumerable<IFightingArt> ValidArts { get; set; }

        [UIHint("FightingArtCombination")]
        public IFightingArtCombination DataObject { get; set; }
    }
}