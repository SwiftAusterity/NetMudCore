using NetMudCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Logging
{
    public class DashboardViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<string> ChannelNames { get; set; }
        public string SelectedLogContent { get; set; }

        
        [Display(Name = "Selected Channel:", Description = "Logs channels are named by purpose and function.")]
        public string SelectedLog { get; set; }
    }
}