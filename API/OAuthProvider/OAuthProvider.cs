using API.Model.Indentity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace API.OAuthProvider
{
    public class OAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public OAuthProvider(string publicClientId)
        {
            _publicClientId = publicClientId ?? throw new ArgumentNullException(nameof(publicClientId));
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<FarmaUserManager>();
            try
            {
                FarmaUser user = await userManager.FindAsync(context.UserName, context.Password);
                if (user == null || user.LockoutEndDateUtc.HasValue)
                {
                    //check user really exists or not
                    user = await userManager.FindByNameAsync(context.UserName);
                    if (user == null)
                    {
                        // non existing user
                        context.SetError("invalid_grant", "The user name or password is incorrect.");
                    }
                    else if (user.LockoutEndDateUtc.HasValue)
                    {
                        await userManager.AccessFailedAsync(user.Id);
                        if (DateTime.UtcNow.CompareTo(user.LockoutEndDateUtc) > 0)
                        {
                            // unlock account if password is valid and let login
                            if (await userManager.CheckPasswordAsync(user, context.Password))
                            {
                                await userManager.ResetAccessFailedCountAsync(user.Id);
                                user.LockoutEndDateUtc = null;
                                await userManager.UpdateAsync(user);
                                await ProcessAuthToken(context, userManager, user);
                            }
                            else
                            {
                                context.SetError("invalid_grant", "The user name or password is incorrect.");
                            }
                        }
                        else
                        {

                            context.SetError("invalid_grant", "User is locked.");
                        }
                    }
                    else
                    {
                        await userManager.AccessFailedAsync(user.Id);
                        context.SetError("invalid_grant", "The user name or password is incorrect.");
                    }
                }
                else
                {
                    await ProcessAuthToken(context, userManager, user);
                }
            }
            catch (Exception)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }
        }

        private static async Task ProcessAuthToken(OAuthGrantResourceOwnerCredentialsContext context, FarmaUserManager userManager, FarmaUser user)
        {
            ClaimsIdentity oAuthIdentity =
                await userManager.GenerateUserIdentityAsync(user, OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookiesIdentity =
                await userManager.GenerateUserIdentityAsync(user, CookieAuthenticationDefaults.AuthenticationType);

            AuthenticationProperties properties = CreateProperties(user);
            var ticket = new AuthenticationTicket(oAuthIdentity, properties);
            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                var expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(FarmaUser user)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", user.UserName },
                { "id", user.Id.ToString() }
            };
            return new AuthenticationProperties(data);
        }

    }

    public static class UserManagerExtensions
    {
        public static async Task<ClaimsIdentity> GenerateUserIdentityAsync(this FarmaUserManager userManager, FarmaUser user, string authenticationType)
        {
            try
            {
                // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
                var userIdentity = await userManager.CreateIdentityAsync(user, authenticationType);
                // Add custom user claims here
                return userIdentity;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}