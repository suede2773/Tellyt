using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// The following using statements were added for this sample
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Notifications;
using Microsoft.IdentityModel.Protocols;
using System.Web.Mvc;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Tellyt.Controllers;

namespace Tellyt
{
  
  public partial class Startup
  {
    private static string signupGatewayUrl = ConfigurationManager.AppSettings["api:SignupGatewayUrl"];
    // App config settings
    private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
    private static string aadInstance = ConfigurationManager.AppSettings["ida:AadInstance"];
    private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
    private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

    // B2C policy identifiers
    public static string SusiPolicyId = ConfigurationManager.AppSettings["ida:SusiPolicyId"];
    public static string PasswordResetPolicyId = ConfigurationManager.AppSettings["ida:PasswordResetPolicyId"];
    public static string EditProfileId = ConfigurationManager.AppSettings["ida:EditProfilePolicyId"];

    public void ConfigureAuth(IAppBuilder app)
    {
      app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

      app.UseKentorOwinCookieSaver();

      app.UseCookieAuthentication(new CookieAuthenticationOptions {CookieManager = new SystemWebCookieManager()});

      // Configure OpenID Connect middleware for each policy
      app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(EditProfileId));
      app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(PasswordResetPolicyId));
      app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(SusiPolicyId));
      //

    }

    private Task OnSecurityTokenValidated(SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
    {
      // If you wanted to keep some local state in the app (like a db of signed up users),
      // you could use this notification to create the user record if it does not already
      // exist.
      try
      {
        var claims = notification.AuthenticationTicket.Identity.Claims;
        var name = claims.Where(c => c.Type == "name").Select(c => c.Value).SingleOrDefault();
        var email = claims.Where(c => c.Type == "emails").Select(c => c.Value).SingleOrDefault();
        var firstName =
          claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")
            .Select(c => c.Value)
            .SingleOrDefault();
        var lastName =
          claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")
            .Select(c => c.Value)
            .SingleOrDefault();
        var identityProvider =
          claims.Where(c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider")
            .Select(c => c.Value)
            .SingleOrDefault();
        //Now insert if necessary

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post,
          signupGatewayUrl + "api/account/useremailexists?email=" + email);

        var response = client.SendAsync(request).Result;

        var userExists = Convert.ToBoolean(response.Content.ReadAsStringAsync().Result.ToLower());

        if (!userExists)
        {
          var inputUser = new InputUserAccount
          {
            AccountKey = string.Empty, //Add support for existing accounts later on
            City = string.Empty,
            CompanyName = name,
            Country = string.Empty,
            DisplayName = name,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PostalCode = string.Empty,
            State = string.Empty,
            StreetAddress = string.Empty
          };

          var postBody = JsonConvert.SerializeObject(inputUser);
          var content = new StringContent(postBody, Encoding.UTF8, "application/json");
          client = new HttpClient();
          var newUserRequest = new HttpRequestMessage(HttpMethod.Post,
            signupGatewayUrl + "api/Account/createuserinfo")
          {Content = content};

          response = client.SendAsync(newUserRequest).Result;
        }
      }
      catch (Exception ex)
      {
        //Log the exception and send an email?
      }

      return Task.FromResult(0);
    }

    // Used for avoiding yellow-screen-of-death TODO
    private Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
    {
      notification.HandleResponse();

      if (notification.ProtocolMessage.ErrorDescription != null && notification.ProtocolMessage.ErrorDescription.Contains("AADB2C90118"))
      {
        // If the user clicked the reset password link, redirect to the reset password route
        notification.Response.Redirect("/Account/ResetPassword");
      }
      else if (notification.Exception.Message == "access_denied")
      {
        // If the user canceled the sign in, redirect back to the home page
        notification.Response.Redirect("/");
      }
      else
      {
        notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
      }

      return Task.FromResult(0);
    }

    private OpenIdConnectAuthenticationOptions CreateOptionsFromPolicy(string policy)
    {
      return new OpenIdConnectAuthenticationOptions
      {
        // For each policy, give OWIN the policy-specific metadata address, and
        // set the authentication type to the id of the policy
        MetadataAddress = String.Format(aadInstance, tenant, policy),
        AuthenticationType = policy,

        // These are standard OpenID Connect parameters, with values pulled from web.config
        ClientId = clientId,
        RedirectUri = redirectUri,
        PostLogoutRedirectUri = redirectUri,
        Notifications = new OpenIdConnectAuthenticationNotifications
        {
          AuthenticationFailed = AuthenticationFailed,
          SecurityTokenValidated = OnSecurityTokenValidated,
        },
        Scope = "openid",
        ResponseType = "id_token",

        // This piece is optional - it is used for displaying the user's name in the navigation bar.
        TokenValidationParameters = new TokenValidationParameters
        {
          NameClaimType = "name",
          SaveSigninToken = true
        }
      };
    }

  }
}