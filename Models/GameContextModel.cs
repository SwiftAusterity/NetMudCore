using NetMudCore.Authentication;
using NetMudCore.DataStructure.Players;

namespace NetMudCore.Models
{
    public class GameContextModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IDictionary<string, string> MusicTracks { get; set; }
        public HashSet<IPlaylist> MusicPlaylists { get; set; }
    }
}