using NetMudCore.Authentication;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Players;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.PlayerManagement
{
    public class ManageCharactersViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IRace> ValidRaces { get; set; }
        public IEnumerable<IGender> ValidGenders { get; set; }

        [UIHint("PlayerTemplate")]
        public IPlayerTemplate NewCharacter { get; set; }
    }


    public class AddEditCharacterViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public AddEditCharacterViewModel()
        {
        }

        [UIHint("PlayerTemplate")]
        public IPlayerTemplate DataObject { get; set; }
        public IEnumerable<IRace> ValidRaces { get; set; }
        public IEnumerable<IGender> ValidGenders { get; set; }
    }

}