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

using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mail;
using System.Net.Mail;

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

      ServicePointManager.SecurityProtocol =
        SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

      var response = client.SendAsync(newUserRequest).Result;

      if (!response.IsSuccessStatusCode)
      {
        return "There was an error processing your order at this time. We apologize for this inconvenience.";
      }
      inputUser = JsonConvert.DeserializeObject<InputUserAccount>(response.Content.ReadAsStringAsync().Result);

      return JsonConvert.SerializeObject(inputUser);
    }

    [HttpPost]
    public async Task SendBetaEmailConfirmation(string firstName, string lastName, string email)
    {
      try
      {
        ServicePointManager.SecurityProtocol =
          SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        var betaSiteUrl = "<a href=\"https://tellytdev.azurewebsites.net/interview\">https://tellytdev.azurewebsites.net/interview</a>";

        var htmlContent = "<h2>Thanks for Helping!</h2><br>";
        htmlContent += "<p>";
        htmlContent += "Your user account has been created! You may now access the beta site at the following location: ";
        htmlContent += betaSiteUrl;
        htmlContent += "</p>";

        var smtpAddress = "smtp.gmail.com";
        var portNumber = 587;

        using (System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage())
        {
          mail.From = new MailAddress("tellytservices@gmail.com");
          mail.To.Add(email);
          mail.Subject = "Thank you for signing up with Tellyt";
          mail.Body = htmlContent;
          mail.IsBodyHtml = true;

          using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
          {
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("tellytservices@gmail.com", "rlvxxlpcxgzbyhir");
            smtp.EnableSsl = true;
            
            smtp.Send(mail);
          }
        }
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
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