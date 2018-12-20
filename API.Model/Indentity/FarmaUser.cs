using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace API.Model.Indentity
{
    public class FarmaUser : IdentityUser<int, UserLogin, UserRole, UserClaim>
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<FarmaUser, int> manager, string authenticationType)
        {
            try
            {
                // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
                var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
                // Add custom user claims here
                return userIdentity;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [MaxLength(5)]
        [Required]
        public string CodUsuario { get; set; }
    }


    public class UserLogin : IdentityUserLogin<int>
    {

    }

    public class Role : IdentityRole<int, UserRole>
    {

    }

    public class UserRole : IdentityUserRole<int>
    {

    }

    public class UserClaim : IdentityUserClaim<int>
    {

    }
}
