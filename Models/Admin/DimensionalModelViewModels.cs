using NetMudCore.Authentication;
using NetMudCore.DataStructure.Architectural.EntityBase;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Admin
{
    public class ManageDimensionalModelDataViewModel : PagedDataModel<IDimensionalModelData>
    {
        public ManageDimensionalModelDataViewModel(IEnumerable<IDimensionalModelData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IDimensionalModelData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IDimensionalModelData, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IDimensionalModelData, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditDimensionalModelDataViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public AddEditDimensionalModelDataViewModel()
        {
        }

        [UIHint("DimensionalModelData")]
        public IDimensionalModelData DataObject { get; set; }
    }
}