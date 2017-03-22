﻿using System;
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
                    var a = context.Applications.Include(app => app.Data).ThenInclude(data => data.DataItems).Where(q => q.PublicKey == publicKey).ToList();
                    if (a == null) throw new Exception("Application not found");
                    return Json(a[0].Data);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        [HttpPost]
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
                        int.TryParse(Request.Query["timestamp"][0], out int ts) && 
                        ts < (CurrentEpoch + 60000))
                    {
                        d.InsertDate = new DateTime(1970, 1, 1).AddMilliseconds(ts);
                    }
                    else
                    {
                        d.InsertDate = DateTime.Now;
                    }

                    if (Request.Query.Count == 0 && Request.Form.Count == 0)
                    {
                        throw new Exception("No data found, must be a query string or x-www-form-urlencoded");
                    }
                    else if (Request.Query.Count > 0 && Request.Form.Count > 0)
                    {
                        throw new Exception("Either the query string or x-www-form-urlencoded data can be used but not both together");
                    }
                    else
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
    }
}
