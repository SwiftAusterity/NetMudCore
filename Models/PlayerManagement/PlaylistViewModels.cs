using NetMudCore.Authentication;
using NetMudCore.DataStructure.Players;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.PlayerManagement
{
    public class ManagePlaylistsViewModel : PagedDataModel<IPlaylist>
    {
        public ManagePlaylistsViewModel(IEnumerable<IPlaylist> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IPlaylist, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IPlaylist, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IPlaylist, object> OrderSecondary
        {
            get
            {
                return item => item.Songs.Count;
            }
        }
    }

    public class AddEditPlaylistViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public AddEditPlaylistViewModel()
        {
        }

        [Display(Name = "Name", Description = "The name of the playlist.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Song", Description = "A song in the playlist.")]
        [DataType(DataType.Text)]
        public string[] SongList { get; set; }

        public IDictionary<string, string> ValidSongs { get; set; }
        public IPlaylist DataObject { get; set; }
    }
}