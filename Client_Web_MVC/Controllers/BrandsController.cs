using Client_Web_MVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Client_Web_MVC.Controllers
{
    public class BrandsController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.BrandsAct = "active";
            
            return View();
        }

        //[ChildActionOnly]
        public PartialViewResult BrandsMenuPartial(List<ProductBrand> brandsList)
        {
            ViewBag.BrandsList = brandsList;
            ViewBag.ProductsAct = "active";

            return PartialView("_BrandsMenuPartial");
        }              

    }
}