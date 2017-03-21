using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;

namespace TotalRecall.Controllers
{
    public class HomeController : Controller
    {
        [ActionName("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [ActionName("Get")]
        public IActionResult Get(String publicKey)
        {
            return Json(new { success = true });
        }

        [ActionName("Create")]
        public IActionResult CreateAsync(String publicKey, String privateKey)
        {
            //if (!ValidateKeys(publicKey,privateKey))
            //{
            //    RedirectToAction("Error");
            //}

            //dynamic o = new ExpandoObject();

            //var expandoDict = o as IDictionary<string, object>;

            //foreach (var item in Request.Query)
            //{
            //    if (item.Key != "privateKey")
            //    {
            //        expandoDict[item.Key.ToLower()] = item.Value[0];
            //    }
            //}

            //if (!expandoDict.ContainsKey("timeStamp"))
            //{
            //    expandoDict["timeStamp"] = DateTimeOffset.Now.ToUnixTimeSeconds(); 
            //}

            ////var x = await DocumentDBRepository<BaseTotalRecallObject>.CreateItemAsync(o);
            //MyDB.SaveRawJson(Json(o));

            return Json(new { success = true });
        }

        private bool ValidateKeys(string publicKey, string privateKey)
        {
            return publicKey == privateKey ? true : false;
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
