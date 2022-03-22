using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tellyt.Models;


namespace Tellyt.Controllers
{
  public class ReturnStatus
  {
    public bool Success { get; set; }
    public string Message { get; set; }
  }
  public class BetaController : Controller
  {
    // GET: Beta
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult About()
    {
      return View();
    }

    public ActionResult AboutThankYou()
    {
      return View();
    }


    [HttpPost]
    public string CheckInviteCode(string inviteCode)
    {
      var returnStatus = new ReturnStatus { Message = string.Empty, Success = true };
      if (inviteCode != "Tellyt2022")
      {
        returnStatus.Success = false;
        returnStatus.Message = "You have entered an invalid invite code";
      }
      return JsonConvert.SerializeObject(returnStatus);
    }

  }
}