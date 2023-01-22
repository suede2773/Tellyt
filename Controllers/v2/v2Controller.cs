using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tellyt.Controllers.v2
{
    public class v2Controller : Controller
    {
        // GET: v2
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult Search()
        {
            return View();
        }

        public ActionResult AccountSettings()
        {
            return View();
        }

        public ActionResult HelpCenter()
        {
            return View();
        }
    }
}