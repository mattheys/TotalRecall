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

        public IActionResult json(Guid publicKey)
        {
            try
            {
                using (var context = new Models.TRModelContext())
                {
                    var a = context.Applications.Include(app => app.DataItems).Where(q => q.PublicKey == publicKey).ToList();
                    if (a == null) throw new Exception("Application not found");
                    return Json(a[0].DataItems.GroupBy(g => g.InsertDate).Select(s => new { timestamp = s.Key, Items = s.SelectMany(l => new { property = l.PropertyName, value = l.PropertyValue }) });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        [HttpPost, HttpGet]
        public IActionResult U(Guid publicKey, Guid privateKey)
        {
            try
            {
                using (var context = new Models.TRModelContext())
                {
                    var insertDate = DateTime.Now;
                    var a = context.Applications.Where(q => q.PublicKey == publicKey && q.PrivateKey == privateKey).FirstOrDefault();
                    if (a == null)
                    {
                        throw new Exception("Application not found");
                    }

                    if (Request.Query.ContainsKey("timestamp") &&
                        int.TryParse(Request.Query["timestamp"][0], out int ts) &&
                        ts < (CurrentEpoch + 60000))
                    {
                        insertDate = new DateTime(1970, 1, 1).AddMilliseconds(ts);
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
                                    PropertyValue = item.Value[0],
                                    InsertDate = insertDate
                                };
                                a.DataItems.Add(di);
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
                                a.DataItems.Add(di);
                            }
                        }
                    }
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
                                  .Include(app => app.DataItems)
                                  .Where(q => q.HideFromSearch == false)
                                  .OrderByDescending(o => o.InsertDate)
                                  .Take(10)
                                  .ToList();
                return View(apps);
            }
        }

        public IActionResult Test()
        {

            using (var context = new Models.TRModelContext())
            {
                var insertDate = DateTime.Now;
                var a = new Application()
                {
                    PrivateKey = Guid.NewGuid(),
                    PublicKey = Guid.NewGuid(),
                    InsertDate = insertDate,
                    HideFromSearch = false,
                    Name = "Test"
                };

                a.DataItems.Add(new DataItem()
                {
                    PropertyName = "sensorName",
                    PropertyValue = "nursery",
                    InsertDate = insertDate
                });

                a.DataItems.Add(new DataItem()
                {
                    PropertyName = "temperature",
                    PropertyValue = "17",
                    InsertDate = insertDate
                });

                a.DataItems.Add(new DataItem()
                {
                    PropertyName = "humidity",
                    PropertyValue = "58",
                    InsertDate = insertDate
                });

                context.Applications.Add(a);
                context.SaveChanges();

                return View(a);
            }

            
        }

    }
}
