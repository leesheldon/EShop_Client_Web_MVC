using Client_Web_MVC.Extensions;
using Client_Web_MVC.Models;
using Client_Web_MVC.Models.ViewModels;
using Client_Web_MVC.Services;
using Newtonsoft.Json;
using PagedList;
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
    public class ProductsController : Controller
    {
        public Products_PhotosViewModel vm { get; set; }
        private readonly IUserSession _userSession;
        private readonly IProductService _productService;

        public ProductsController(IUserSession userSession, IProductService productService)
        {
            _userSession = userSession;
            _productService = productService;
        }

        // GET: Products
        public async Task<ActionResult> Index(string search_Data, int? page = 1)
        {
            List<Product> productsList = new List<Product>();
            int totalItems = 0;
            int pageSize = 8;
            int pageIndex = (page ?? 1);

            try
            {
                // Brand Id = -1, Type Id = -1 means that we do not find products by Brand Id, Type Id
                HttpResponseMessage response = await _productService.GetProductsList(search_Data, "", pageIndex, pageSize, -1, -1);
                
                var responseData = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    ResponseObj resObj = JsonConvert.DeserializeObject<ResponseObj>(responseData);

                    object cauThongBao = "Error in getting list of Products!<br /> Reason: " + resObj.Message;
                    return View("Error", cauThongBao);
                }

                Pagination<Product> pagination = JsonConvert.DeserializeObject<Pagination<Product>>(responseData);
                if (pagination == null)
                {
                    object cauThongBao = $"There is no Product in list! Pagination is null.";
                    return View("Error", cauThongBao);
                }

                productsList = pagination.Data.ToList<Product>();
                totalItems = pagination.Count;

                List<Photo> photosList = new List<Photo>();
                foreach (var item in pagination.Data)
                {
                    photosList.AddRange(item.Photos);
                }

                ViewBag.Url_Img_PlaceHolder = SD.BaseUrl + SD.Url_Pic_PlaceHolder;
                vm = new Products_PhotosViewModel
                {
                    Products = productsList,
                    Photos = photosList
                };

                // Paging
                var productsAsIPagedList = new StaticPagedList<Product>(productsList, pageIndex, pageSize, totalItems);
                ViewBag.OnePageOfProducts = productsAsIPagedList;
                
                return View(vm);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Lỗi truy cập dữ liệu.<br /> Lý do: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null || id < 1) return RedirectToAction("Index", "Products");
                        
            try
            {
                Product productFromDb = new Product();
                HttpResponseMessage response = await _productService.GetProduct(id);
                var responseData = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    ResponseObj resObj = JsonConvert.DeserializeObject<ResponseObj>(responseData);

                    object cauThongBao = "Error in finding Product to view Detail.<br /> Reason: " + resObj.Message;
                    return View("Error", cauThongBao);
                }

                productFromDb = JsonConvert.DeserializeObject<Product>(responseData);
                if (productFromDb == null)
                {
                    object cauThongBao = $"Cannot find Product with Id (" + id.ToString() + ").";
                    return View("Error", cauThongBao);
                }

                List<Photo> photosList = new List<Photo>();
                photosList.AddRange(productFromDb.Photos);

                ViewBag.Url_Img_PlaceHolder = SD.BaseUrl + SD.Url_Pic_PlaceHolder;

                // Get Brands list
                List<ProductBrand> brandsList = new List<ProductBrand>();
                HttpResponseMessage responseBr = await _productService.GetBrandsList();
                var resData_Brand = responseBr.Content.ReadAsStringAsync().Result;
                if (!responseBr.IsSuccessStatusCode)
                {
                    object cauThongBao = $"Error in getting list of Brands!<br /> Reason: {resData_Brand}";
                    return View("Error", cauThongBao);
                }

                brandsList = JsonConvert.DeserializeObject<List<ProductBrand>>(resData_Brand);
                ViewBag.BrandsList = brandsList;

                return View(productFromDb);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Lỗi truy cập dữ liệu.<br /> Lý do: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }
                
        public async Task<ActionResult> SearchByBrand(int? id, int? page)
        {
            if (id == null || id < 1) return RedirectToAction("Index", "Products");
            if (page == null || page < 1) page = 1;

            List<Product> productsList = new List<Product>();
            int totalItems = 0;
            int pageSize = 8;
            int pageIndex = (page ?? 1);

            try
            {
                // Type Id = -1 means that we do not find products by Brand Id, Type Id
                HttpResponseMessage response = await _productService.GetProductsList("", "", pageIndex, pageSize, id, -1);

                //Storing the response details recieved from web api   
                var responseData = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    ResponseObj resObj = JsonConvert.DeserializeObject<ResponseObj>(responseData);

                    object cauThongBao = "Error in getting list of Products!<br /> Reason: " + resObj.Message;
                    return View("Error", cauThongBao);
                }

                Pagination<Product> pagination = JsonConvert.DeserializeObject<Pagination<Product>>(responseData);
                if (pagination == null)
                {
                    object cauThongBao = $"There is no Product in list via Search By Brand! Pagination is null.";
                    return View("Error", cauThongBao);
                }

                productsList = pagination.Data.ToList<Product>();
                totalItems = pagination.Count;
                if (productsList.Count > 0)
                {
                    ViewBag.Title = $"Products List by : {productsList[0].ProductBrand.Name}";
                }
                else
                {
                    ViewBag.Title = "Products List";
                }

                List<Photo> photosList = new List<Photo>();
                foreach (var item in pagination.Data)
                {
                    photosList.AddRange(item.Photos);
                }

                ViewBag.Url_Img_PlaceHolder = SD.BaseUrl + SD.Url_Pic_PlaceHolder;
                vm = new Products_PhotosViewModel
                {
                    Products = productsList,
                    Photos = photosList
                };

                ViewBag.BrandId = id;
                ViewBag.PageNumber = pageIndex;
                ViewBag.PageCount = (int)Math.Ceiling((decimal)pagination.Count / pagination.PageSize);

                // Paging
                var productsAsIPagedList = new StaticPagedList<Product>(productsList, pageIndex, pageSize, totalItems);
                ViewBag.OnePageOfProducts = productsAsIPagedList;
                                                
                // Get Brands list
                List<ProductBrand> brandsList = new List<ProductBrand>();
                HttpResponseMessage responseBr = await _productService.GetBrandsList();
                var resData_Brand = responseBr.Content.ReadAsStringAsync().Result;
                if (!responseBr.IsSuccessStatusCode)
                {
                    object cauThongBao = $"Error in getting list of Brands!<br /> Reason: {resData_Brand}";
                    return View("Error", cauThongBao);
                }

                brandsList = JsonConvert.DeserializeObject<List<ProductBrand>>(resData_Brand);
                ViewBag.BrandsList = brandsList;

                return View("ProductsListByBrand", vm);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Lỗi truy cập dữ liệu.<br /> Lý do: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

    }
}