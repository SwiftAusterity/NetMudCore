using Microsoft.AspNetCore.Identity;
using NetMudCore.Data.Players;
using NetMudCore.DataStructure.Administrative;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace NetMudCore.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Generates a user identity
        /// </summary>
        /// <param name="manager">the user manager used to get the identity</param>
        /// <returns>the identity (async)</returns>
        public async Task<IdentityResult> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            IdentityResult userIdentity = await manager.CreateAsync(this);

            // Add custom user claims here
            return userIdentity;
        }

        [ForeignKey("GameAccount")]
        public required string GlobalIdentityHandle { get; set; }

        /// <summary>
        /// The game account connected to this user identity
        /// </summary>
        [ForeignKey("GlobalIdentityHandle")]
        public virtual required Account GameAccount { get; set; }

        /// <summary>
        /// Get the staffrank of this account
        /// </summary>
        /// <returns>the staffrank</returns>
        public static StaffRank GetStaffRank(IPrincipal identity)
        {
            StaffRank rank = StaffRank.Player;

            if (identity.IsInRole("Admin"))
            {
                rank = StaffRank.Admin;
            }
            else if (identity.IsInRole("Builder"))
            {
                rank = StaffRank.Builder;
            }
            else if (identity.IsInRole("Guest"))
            {
                rank = StaffRank.Guest;
            }

            return rank;
        }
    }
}
