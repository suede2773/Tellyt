using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tellyt.Controllers
{
  public class ProfileController : Controller
  {
    // GET: Profile
    [Authorize]
    public ActionResult Index()
    {
      return View();
    }
  }
}