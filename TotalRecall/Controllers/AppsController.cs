using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TotalRecall.Models;
using Microsoft.EntityFrameworkCore;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TotalRecall.Controllers
{
    public class AppsController : Controller
    {
        public double CurrentEpoch => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        public double GetEpoch(DateTime d) => (d - new DateTime(1970, 1, 1)).TotalMilliseconds; 
        [Route("Apps")]
        public IActionResult Index()
        {
            using (var context = new TRModelContext())
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
            if (publicKey != null && privateKey != null)
            {
                using (var context = new TRModelContext())
                {
                    var model = context.Applications.Where(q => q.PublicKey == publicKey && q.PrivateKey == privateKey).FirstOrDefault();
                    if (model != null)
                    {
                        return View(model);
                    }
                }
            }

            return RedirectToAction("Index", "Home");

        }

        public IActionResult V(Application model)
        {
            return View(model);
        }

        [HttpGet][Route("Apps/json/{publicKey}")]
        public IActionResult json(Guid publicKey)
        {
            try
            {
                using (var context = new Models.TRModelContext())
                {
                    //var a = context.Applications.Include(app => app.Data).ThenInclude(data => data.DataItems).Where(q => q.PublicKey == publicKey).ToList();
                    
                    var b = context.Applications.Include(app => app.Data).ThenInclude(data => data.DataItems).Where(q => q.PublicKey == publicKey);

                    foreach (var item in Request.Query)
                    {
                        b = b.Where(q => q.PrivateKey == Guid.NewGuid()); //q.Data.Any(r => r.DataItems.Any(s => s.PropertyName.Equals(item.Key) && s.PropertyValue.Equals(item.Value[0]))));
                    }
                    
                    var x = new List<Dictionary<string,string>>();

                    foreach (var data in b.ToList()[0].Data)
                    {
                        var d = new Dictionary<string, string> { { "timestamp", String.Format("{0:yyyy-MM-dd}T{0:HH:mm:ss.fff}Z", data.InsertDate.ToUniversalTime()) } };

                        foreach (var dataItem in data.DataItems)
                        {
                            d.Add(dataItem.PropertyName, dataItem.PropertyValue);
                        }
                        x.Add(d);
                    }
                    if (b == null) throw new Exception("Application not found");
                    //return Json(a[0].Data);
                    return Json(x);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        [HttpPost,HttpGet]
        public IActionResult U(Guid publicKey, Guid privateKey)
        {
            try
            {
                using (var context = new Models.TRModelContext())
                {
                    var d = new Models.Data();
                    var a = context.Applications.Where(q => q.PublicKey == publicKey && q.PrivateKey == privateKey).FirstOrDefault();
                    if (a == null)
                    {
                        throw new Exception("Application not found");
                    }

                    if (Request.Query.ContainsKey("timestamp") && 
                        long.TryParse(Request.Query["timestamp"][0], out long ts) && 
                        ts < (CurrentEpoch + 60000))
                    {
                        d.InsertDate = new DateTime(1970, 1, 1).AddMilliseconds(ts);
                    }
                    else
                    {
                        d.InsertDate = DateTime.Now;
                    }

                    if (Request.Method == "GET")
                    {
                        foreach (var item in Request.Query)
                        {
                            if (item.Key != "timestamp")
                            {
                                var di = new DataItem()
                                {
                                    PropertyName = item.Key,
                                    PropertyValue = item.Value[0]
                                };
                                d.DataItems.Add(di);
                            }
                        }
                    }

                    if (Request.Method == "POST")
                    {
                        foreach (var item in Request.Form)
                        {
                            if (item.Key != "timestamp")
                            {
                                var di = new DataItem()
                                {
                                    PropertyName = item.Key,
                                    PropertyValue = item.Value[0]
                                };
                                d.DataItems.Add(di);
                            }
                        }
                    }

                    a.Data.Add(d);

                    context.SaveChanges();

                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }

            return Json(true);
        }

        public IActionResult New()
        {
            ViewData["Title"] = "New application";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New(Application model)
        {
            ViewData["Title"] = "New application";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.PrivateKey = Guid.NewGuid();
            model.PublicKey = Guid.NewGuid();

            using (var context = new TRModelContext())
            {
                context.Applications.Add(model);
                context.SaveChanges();
            }

            return RedirectToAction("V", new { privateKey = model.PrivateKey, publicKey = model.PublicKey });
        }

        public IActionResult Browse()
        {
            ViewData["Title"] = "List of recent Applications";
            using (var context = new Models.TRModelContext())
            {
                var apps = context.Applications
                                  .Include(app=>app.Data)
                                  .Where(q => q.HideFromSearch == false)
                                  .OrderByDescending(o => o.InsertDate)
                                  .Take(10)
                                  .ToList();
                return View(apps);
            }
        }
    }
}
