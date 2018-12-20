using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace API.Model.Indentity
{
    public class FarmaUserManager:UserManager<FarmaUser, int>
    {
        public FarmaUserManager(IUserStore<FarmaUser, int> store):base(store)
        {

        }

        public static FarmaUserManager Create(IdentityFactoryOptions<FarmaUserManager> options, IOwinContext context)
        {
            var manager = new FarmaUserManager(
                new UserStore<FarmaUser, Role, int, UserLogin, UserRole, UserClaim>(context.Get<FarmaContext>())
                )
            {
                MaxFailedAccessAttemptsBeforeLockout = 5,
                DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(15)                
            };

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<FarmaUser, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<FarmaUser, int>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

    }
}
