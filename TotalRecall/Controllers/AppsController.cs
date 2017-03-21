using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TotalRecall.Controllers
{
    public class AppsController : Controller
    {
        [Route("Apps")]
        public IActionResult Index()
        {
            using (var context = new Models.TotalRecall.TRModelContext())
            {
                var apps = context.Applications.Where(q => q.HideFromSearch == false).ToList();
                return View(apps);
            }
        }

        [Route("Apps/V/{publicKey}")]
        public IActionResult V(Guid publicKey)
        {
            Dictionary<string, Guid> model = new Dictionary<string, Guid>();
            if (publicKey != null) model["publicKey"] = publicKey;
            return View(model);
        }

        [Route("Apps/V/{publicKey}/{privateKey}")]
        public IActionResult V(Guid publicKey, Guid privateKey)
        {
            Dictionary<string, Guid> model = new Dictionary<string, Guid>();
            if(publicKey != null) model["publicKey"] = publicKey;
            if(privateKey != null) model["privateKey"] = privateKey;
            return View(model);
        }

        public IActionResult V(Models.TotalRecall.Application model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult U(string publicKey, string privateKey)
        {
            return Json(new
            {
                publicKey = publicKey,
                privateKey = privateKey,
                QueryString = Request.QueryString,
                Query = Request.Query
            });
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New(Models.TotalRecall.Application model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.PrivateKey = Guid.NewGuid();
            model.PublicKey = Guid.NewGuid();

            return RedirectToAction("V", new { privateKey = model.PrivateKey, publicKey = model.PublicKey });
        }
    }
}
