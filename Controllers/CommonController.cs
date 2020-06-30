using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Tellyt.Controllers
{
  public static class CommonController
  {
    private static string signupGatewayUrl = ConfigurationManager.AppSettings["api:SignupGatewayUrl"];
    public static string GetUserIdFromEmail(string email)
    {
      var client = new HttpClient();
      var getUserInfoRequest = new HttpRequestMessage(HttpMethod.Get,
        signupGatewayUrl + "api/account/getuserinfofromemail?email=" + email);
      var getUserInfoResponse = client.SendAsync(getUserInfoRequest).Result;

      var userInfo =
        JsonConvert.DeserializeObject<Tellyt.InputUserAccount>(getUserInfoResponse.Content.ReadAsStringAsync().Result);

      return userInfo.ObjectId;
    }

    public static string GetCurrentUserId()
    {
      var identity = (ClaimsIdentity)HttpContext.Current.User.Identity;
      var claims = identity.Claims;
      return
        claims.Where(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")
          .Select(c => c.Value)
          .SingleOrDefault();
    }

  }
}