using Client_Web_MVC.Extensions;
using Client_Web_MVC.Models;
using Client_Web_MVC.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Client_Web_MVC.Controllers
{
    [Authorize(Roles = SD.AdminEndUser)]
    public class AdminProductsController : Controller
    {
        private readonly IUserSession _userSession;
        private readonly IProductService _productService;

        public AdminProductsController(IUserSession userSession, IProductService productService)
        {
            _userSession = userSession;
            _productService = productService;
        }
                
        public async Task<ActionResult> GetBrands_And_Types_List(int? selectedBrandId, int? selectedTypeId)
        {
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
            brandsList.Insert(0, new ProductBrand { Id = -1, Name = "-- Select Brand --" });

            // Get Types list
            List<ProductType> typesList = new List<ProductType>();
            HttpResponseMessage responseTyp = await _productService.GetTypesList();
            var resData_Type = responseTyp.Content.ReadAsStringAsync().Result;
            if (!responseTyp.IsSuccessStatusCode)
            {
                object cauThongBao = $"Error in getting list of Types!<br /> Reason: {resData_Type}";
                return View("Error", cauThongBao);
            }

            typesList = JsonConvert.DeserializeObject<List<ProductType>>(resData_Type);
            typesList.Insert(0, new ProductType { Id = -1, Name = "-- Select Type --" });

            ViewBag.BrandsList = new SelectList(brandsList, "Id", "Name", selectedBrandId);
            ViewBag.TypesList = new SelectList(typesList, "Id", "Name", selectedTypeId);

            return null;
        }

        // GET: AdminProducts
        public async Task<ActionResult> Index(string search_Data, int? page = 1)
        {
            ViewBag.ProductsAct = "active";

            List<Product> productsList = new List<Product>();
            int totalItems = 0;
            int pageSize = 50;
            int pageIndex = (page ?? 1);

            try
            {
                // Brand Id = -1, Type Id = -1 means that we do not find products by Brand Id, Type Id
                HttpResponseMessage response = await _productService.GetProductsList(search_Data, pageIndex, pageSize, -1, -1);

                //Storing the response details recieved from web api   
                var responseData = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    object cauThongBao = $"Error in getting list of Products!<br /> Reason: {responseData}";
                    return View("Error", cauThongBao);
                }

                Pagination<Product> pagination = JsonConvert.DeserializeObject<Pagination<Product>>(responseData);
                if (pagination == null)
                {
                    object cauThongBao = $"There is no Product in list! Pagination is null.";
                    return View("Error", cauThongBao);
                }
                                
                productsList = pagination.Data.ToList<Product>();
                ViewBag.TotalItems = pagination.Count;

                return View(productsList);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in getting list of Products!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }            
        }

        #region Create New
                
        public async Task<ActionResult> Create()
        {
            // Get Brands and Types list
            object errObj = await GetBrands_And_Types_List(-1, -1);
            if (errObj != null)
            {
                return View("Error", errObj);
            }

            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Product product)
        {
            try
            {
                string accessToken = "";
                accessToken = _userSession.AccessToken;

                if (ModelState.IsValid)
                {
                    product.PictureUrl = SD.Url_Pic_PlaceHolder_SavedToDb;

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(SD.BaseUrl);

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                            SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                        client.DefaultRequestHeaders.Clear();

                        // Create new Product
                        var request = new HttpRequestMessage(HttpMethod.Post, SD.BaseUrl + "api/products");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        request.Content = new StringContent(JsonConvert.SerializeObject(product));
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        HttpResponseMessage response = await client.SendAsync(request);
                        var responseData = response.Content.ReadAsStringAsync().Result;

                        if (response.IsSuccessStatusCode)
                        {
                            // Create new Product successfully
                            return RedirectToAction("Index");
                        }

                        // Create new Product failed
                        ModelState.Clear();
                        ModelState.AddModelError("", $"Error in creating new Product! Reason: {responseData}");
                    }
                }

                // Get Brands and Types list
                object errObj = await GetBrands_And_Types_List(-1, -1);
                if (errObj != null)
                {
                    return View("Error", errObj);
                }

                return View(product);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in creating Product!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        #endregion

        #region Edit

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null || id < 1)
            {
                return RedirectToAction("Index");
            }

            try
            {
                Product productFromDb = new Product();
                HttpResponseMessage response = await _productService.GetProduct(id);
                var responseData = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    object cauThongBao = $"Error in finding Product to edit!<br /> Reason: {responseData}";
                    return View("Error", cauThongBao);
                }

                productFromDb = JsonConvert.DeserializeObject<Product>(responseData);
                if (productFromDb == null)
                {
                    object cauThongBao = $"Cannot find Product with Id (" + id.ToString() + ").";
                    return View("Error", cauThongBao);
                }

                // Get Brands and Types list
                object errObj = await GetBrands_And_Types_List(productFromDb.ProductBrandId, productFromDb.ProductTypeId);
                if (errObj != null)
                {
                    return View("Error", errObj);
                }

                return View(productFromDb);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in editing Product!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(SD.BaseUrl);

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                            SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                        client.DefaultRequestHeaders.Clear();

                        // Access token
                        string accessToken = "";
                        accessToken = _userSession.AccessToken;

                        // Edit Product
                        string requestParams = SD.BaseUrl + "api/products/" + product.Id.ToString();
                        var request = new HttpRequestMessage(HttpMethod.Put, requestParams);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        request.Content = new StringContent(JsonConvert.SerializeObject(product));
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        HttpResponseMessage response = await client.SendAsync(request);
                        var responseData = response.Content.ReadAsStringAsync().Result;

                        if (response.IsSuccessStatusCode)
                        {
                            // Edit Product successfully
                            return RedirectToAction("Index");
                        }

                        // Edit Product failed
                        ModelState.Clear();
                        ModelState.AddModelError("", $"Error in editing Product! Reason: {responseData}");
                    }
                }

                // Get Brands and Types list
                object errObj = await GetBrands_And_Types_List(product.ProductBrandId, product.ProductTypeId);
                if (errObj != null)
                {
                    return View("Error", errObj);
                }

                return View(product);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in editing Product!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        #endregion

        #region Delete

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null || id < 1)
            {
                return RedirectToAction("Index");
            }

            try
            {
                Product productFromDb = new Product();
                HttpResponseMessage response = await _productService.GetProduct(id);
                var responseData = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    object cauThongBao = $"Error in finding Product to delete!<br /> Reason: {responseData}";
                    return View("Error", cauThongBao);
                }

                productFromDb = JsonConvert.DeserializeObject<Product>(responseData);
                if (productFromDb == null)
                {
                    object cauThongBao = $"Cannot find Product with Id (" + id.ToString() + ").";
                    return View("Error", cauThongBao);
                }

                return View(productFromDb);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in editing Product!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(SD.BaseUrl);

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                        SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    client.DefaultRequestHeaders.Clear();

                    // Access token
                    string accessToken = "";
                    accessToken = _userSession.AccessToken;

                    // Delete Product
                    string requestParams = SD.BaseUrl + "api/products/" + id.ToString();
                    var request = new HttpRequestMessage(HttpMethod.Delete, requestParams);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    HttpResponseMessage response = await client.SendAsync(request);
                    var responseData = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        // Edit Product successfully
                        return RedirectToAction("Index");
                    }

                    // Delete Product failed
                    ModelState.Clear();
                    ModelState.AddModelError("", $"Error in deleting Product! Reason: {responseData}");
                }

                // Delete Product successfully
                return View();
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in deleting Product!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        #endregion


    }
}