using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Text;

// The following using statements were added for this sample.
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Tellyt.Controllers
{
  public class InputUserAccount
  {
    public string Email { get; set; }
    public string Password { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PostalCode { get; set; }
    public string State { get; set; }
    public string StreetAddress { get; set; }
    public string ObjectId { get; set; }
    public string Error { get; set; }
    public string CompanyName { get; set; }
    public string AccountKey { get; set; }
    public bool IsValid { get; set; }
  }
  [AllowAnonymous]
  public class AccountController : Controller
  {
    private static string signupGatewayUrl = ConfigurationManager.AppSettings["api:SignupGatewayUrl"];
    [AllowAnonymous]
    public void Login()
    {
      if (!Request.IsAuthenticated)
      {
        // To execute a policy, you simply need to trigger an OWIN challenge.
        // You can indicate which policy to use by specifying the policy id as the AuthenticationType
        HttpContext.GetOwinContext().Authentication.Challenge(
            new AuthenticationProperties() { RedirectUri = "/" }, Startup.SusiPolicyId);
      }
    }

    public void ResetPassword()
    {
      if (!Request.IsAuthenticated)
      {
        HttpContext.GetOwinContext().Authentication.Challenge(
        new AuthenticationProperties() { RedirectUri = "/" }, Startup.PasswordResetPolicyId);
      }
    }

    public void Profile()
    {
      if (Request.IsAuthenticated)
      {
        HttpContext.GetOwinContext().Authentication.Challenge(
            new AuthenticationProperties() { RedirectUri = "/" }, Startup.EditProfileId);
      }
    }

    [HttpPost]
    public string CreateUserAccount(string firstName, string lastName, string email, string password)
    {
      var inputUser = new InputUserAccount
      {
        AccountKey = string.Empty, //Add support for existing accounts later on
        City = string.Empty,
        CompanyName = string.Empty,
        Country = string.Empty,
        DisplayName = firstName + lastName,
        Email = email,
        FirstName = firstName,
        LastName = lastName,
        PostalCode = string.Empty,
        State = string.Empty,
        StreetAddress = string.Empty,
        Password = password
      };

      var postBody = JsonConvert.SerializeObject(inputUser);
      var content = new StringContent(postBody, Encoding.UTF8, "application/json");
      var client = new HttpClient();
      var newUserRequest = new HttpRequestMessage(HttpMethod.Post,
        signupGatewayUrl + "api/Account/createuser")
      { Content = content };

      var response = client.SendAsync(newUserRequest).Result;

      if (!response.IsSuccessStatusCode)
      {
        return "There was an error processing your order at this time. We apologize for this inconvenience.";
      }
      inputUser = JsonConvert.DeserializeObject<InputUserAccount>(response.Content.ReadAsStringAsync().Result);

      //if (!inputUser.IsValid) return inputUser.Error;
      

      return JsonConvert.SerializeObject(inputUser);
    }

    public ActionResult Logout()
    {
      if (Request.IsAuthenticated)
      {
        IEnumerable<AuthenticationDescription> authTypes = HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes();
        HttpContext.GetOwinContext().Authentication.SignOut(authTypes.Select(t => t.AuthenticationType).ToArray());
        Request.GetOwinContext().Authentication.GetAuthenticationTypes();
      }
      return RedirectToAction("Index", "Home");
    }

  }
}