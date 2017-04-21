using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using TotalRecall.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TotalRecall.Controllers
{
    public class AppsController : Controller
    {
        public double CurrentEpoch => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        public double GetEpoch(DateTime d) => (d - new DateTime(1970, 1, 1)).TotalMilliseconds;
        public DateTime FromEpoch(double d) => new DateTime(1970, 1, 1).AddMilliseconds(d);


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New(TRApplication model)
        {
            ViewData["Title"] = "New application";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.AdminKey = Guid.NewGuid();
            model.PublicKey = Guid.NewGuid();
            model.UpdateKey = Guid.NewGuid();

            using (var trContext = new TRContext())
            {
                trContext.Applications.Add(model);
                trContext.SaveChanges();
            }

            using (var context = new ApplicationContext(model.PublicKey))
            {
                context.Database.EnsureCreated();
            }

            return RedirectToAction("Admin", new { adminKey = model.AdminKey });
        }

        public IActionResult New()
        {
            ViewData["Title"] = "New application";
            return View();
        }

        [Route("Apps/Download/{AdminKey}")]
        public IActionResult Download(Guid AdminKey)
        {
            using (var trContext = new TRContext())
            {
                var app = trContext.Applications.Where(q => q.AdminKey == AdminKey).FirstOrDefault();
                return new FileStreamResult(System.IO.File.Open("dbs" + System.IO.Path.DirectorySeparatorChar + app.PublicKey.ToString() + ".db", System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite), "application/octet-stream");
            }
        }

        [Route("Apps/Browse")]
        public IActionResult Browse()
        {
            List<TRApplication> apps = new List<TRApplication>();
            using (var trContext = new TRContext())
            {
                apps = trContext.Applications.Where(q => q.Public == true).OrderByDescending(o => o.LastUpdated).Take(10).ToList();
            }

            return View(apps);
        }

        //[Route("Apps/V/{publicKey}")]
        //public IActionResult V(Guid publicKey)
        //{
        //    Dictionary<string, Guid> model = new Dictionary<string, Guid>();
        //    if (publicKey != null) model["publicKey"] = publicKey;
        //    return View(model);
        //}

        [Route("Apps/Admin/{adminKey}")]
        public IActionResult Admin(Guid adminKey)
        {
            if (adminKey != null)
            {
                using (var context = new TRContext())
                {
                    var model = context.Applications.Where(q => q.AdminKey == adminKey).FirstOrDefault();
                    if (model != null)
                    {
                        return View(model);
                    }
                }
            }

            return RedirectToAction("Index", "Apps");

        }

        //public IActionResult V(TRApplication model)
        //{
        //    return View(model);
        //}

        [HttpGet] [Route("Apps/json/{publicKey}")]
        public IActionResult Json(Guid publicKey)
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");

            var returnValue = new List<Dictionary<string, string>>();

            TRApplication trApp = null;

            try
            {
                using (var trContext = new TRContext())
                {
                    trApp = trContext.Applications.Where(q => q.PublicKey == publicKey).FirstOrDefault();
                    if (trApp == null)
                    {
                        return Json(new { success = false, message = "No such app." });
                    }
                }

                using (var context = new ApplicationContext(publicKey))
                {

                    var data = context.Data.AsQueryable();

                    foreach (var v in Request.Query["timestamp"])
                    {
                        if (v.StartsWith("gt"))
                        {
                            string t = v.Replace("gt", "");
                            if (double.TryParse(t, out double dt))
                            {
                                data = data.Where(q => q.InsertDate > FromEpoch(dt));
                            }
                        }
                        else if (v.StartsWith("lt"))
                        {
                            string t = v.Replace("lt", "");
                            if (double.TryParse(t, out double dt))
                            {
                                data = data.Where(q => q.InsertDate < FromEpoch(dt));
                            }
                        }
                    }

                    foreach (var item in Request.Query)
                    {
                        if (item.Key == "timestamp" || item.Key == "limit")
                        {
                            continue;
                        }

                        foreach (var v in item.Value)
                        {
                            data = data.Where(q => q.DataItems.Any(r => r.PropertyName == item.Key && r.PropertyValue == v));
                        }
                    }

                    List<Data> listData = null;

                    if (Request.Query.ContainsKey("limit"))
                    {
                        int i = 2880;
                        foreach (var limit in Request.Query["limit"])
                        {
                            if (int.TryParse(limit, out int limitInt))
                            {
                                if (limitInt >= 0)
                                {
                                    i = Math.Min(i, limitInt);
                                }
                            }
                        }
                        listData = data.Include(d => d.DataItems).OrderByDescending(o => o.InsertDate).Take(i).ToList();
                    }
                    else
                    {
                        listData = data.Include(d => d.DataItems).OrderByDescending(o => o.InsertDate).Take(2880).ToList();
                    }



                    foreach (var item in listData)
                    {
                        var d = new Dictionary<string, string> { { "timestamp", String.Format("{0:yyyy-MM-dd}T{0:HH:mm:ss.fff}Z", item.InsertDate.ToUniversalTime()) } };
                        foreach (var di in item.DataItems)
                        {
                            d.Add(di.PropertyName, di.PropertyValue);
                        }
                        returnValue.Add(d);
                    }

                }

                return Json(returnValue);

            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        [HttpPost, HttpGet]
        [Route("Apps/Update/{updateKey}")]
        public IActionResult Update(Guid updateKey)
        {
            try
            {
                TRApplication trApp = null;
                using (var trContext = new TRContext())
                {
                    trApp = trContext.Applications.Where(q => q.UpdateKey == updateKey).FirstOrDefault() ?? throw new Exception("No such app");
                }

                using (var context = new ApplicationContext(trApp.PublicKey))
                {
                    var d = new Data();

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

                    context.Data.Add(d);

                    context.SaveChanges();

                }

                using (var trContext = new TRContext())
                {
                    trContext.Applications.Where(q => q.UpdateKey == updateKey).FirstOrDefault().LastUpdated = DateTime.Now;
                    trContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }

            return Json(true);
        }

        [Route("Apps/View/{publicKey}")]
        public IActionResult View(Guid publicKey)
        {
            TRApplication trApp = null;

            try
            {

                using (var trContext = new TRContext())
                {
                    trApp = trContext.Applications.Where(q => q.PublicKey == publicKey).FirstOrDefault() ?? throw new Exception("App not found");
                }

            }
            catch (Exception)
            {
            }

            return View(trApp);
        }
    }
}
