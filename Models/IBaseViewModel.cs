using NetMudCore.Authentication;

namespace NetMudCore.Models
{
    public interface IBaseViewModel
    {
        ApplicationUser? AuthedUser { get; set; }
    }
}