using Client_Web_MVC.Extensions;
using Client_Web_MVC.Models;
using Client_Web_MVC.Models.ViewModels;
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
        private readonly IPhotoService _photoService;

        public AdminProductsController(IUserSession userSession, IProductService productService, IPhotoService photoService)
        {
            _userSession = userSession;
            _productService = productService;
            _photoService = photoService;
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
                int productId = (id ?? 1);
                Dictionary<string, object> result = new Dictionary<string, object>();
                result = await GetProductFromDB(productId, "EditProduct");

                if (result["Product"] == null)
                {
                    return View("Error", result["ErrObj"]);
                }
                else
                {
                    productFromDb = result["Product"] as Product;
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
                int productId = (id ?? 1);
                Dictionary<string, object> result = new Dictionary<string, object>();
                result = await GetProductFromDB(productId, "Delete");

                if (result["Product"] == null)
                {
                    return View("Error", result["ErrObj"]);
                }
                else
                {
                    productFromDb = result["Product"] as Product;
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
                        // Delete Product successfully
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

        #region Edit Photo
        public async Task<ActionResult> EditPhoto(int? id)
        {
            if (id == null || id < 1)
            {
                return RedirectToAction("Index");
            }

            try
            {
                Product productFromDb = new Product();

                int productId = (id ?? 1);
                Dictionary<string, object> result = new Dictionary<string, object>();
                result = await GetProductFromDB(productId, "EditPhoto");

                if (result["Product"] == null)
                {
                    return View("Error", result["ErrObj"]);
                }
                else
                {
                    productFromDb = result["Product"] as Product;
                }

                Product_PhotosViewModel vm = new Product_PhotosViewModel
                {
                    Product = productFromDb,
                    Photos = productFromDb.Photos as List<Photo>
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in editing Product photo!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        [HttpPost, ActionName("UploadNewPhoto")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPhoto(HttpPostedFileBase newPhotoFile, int id)
        {
            try
            {
                Product productFromDb = new Product();
                ViewBag.ErrMsg = "";
                Product_PhotosViewModel vm = new Product_PhotosViewModel();

                if (newPhotoFile != null && newPhotoFile.ContentLength > 0)
                {
                    // Validate File extension
                    string extensionsList = "png,jpg,jpeg,gif";
                    List<string> allowedExtensions = extensionsList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    bool isValid = allowedExtensions.Any(y => newPhotoFile.FileName.EndsWith(y));

                    ViewBag.ErrMsg = isValid ? "" : "'" + newPhotoFile.FileName + "' file is not supported. Please select only Supported Files .png | .jpg";

                    // Validate file size                   
                    int maxContentLength = 1024 * 1024 * 2; //2 MB
                    if (newPhotoFile.ContentLength > maxContentLength)
                    {
                        ViewBag.ErrMsg = "Your file is too large, maximum allowed size is: " + maxContentLength + " MB";
                    }

                    if (ViewBag.ErrMsg == "")
                    {
                        // Upload Photo
                        HttpResponseMessage response = await _photoService.UploadNewPhoto(id, newPhotoFile);
                        var responseData = response.Content.ReadAsStringAsync().Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            object cauThongBao = $"Error in uploading Photo!<br /> Reason: {responseData}";
                            return View("Error", cauThongBao);
                        }

                        // Upload Photo successfully
                    }
                }
                else
                {
                    ViewBag.ErrMsg = "Please select file or your selected file with empty content.";
                }

                // Get Product from Database
                Dictionary<string, object> result = new Dictionary<string, object>();
                result = await GetProductFromDB(id, "EditPhoto");

                if (result["Product"] == null)
                {
                    return View("Error", result["ErrObj"]);
                }
                else
                {
                    productFromDb = result["Product"] as Product;
                }

                // View Model
                vm.Product = productFromDb;
                vm.Photos = productFromDb.Photos as List<Photo>;
                
                return View("EditPhoto", vm);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in uploading new Photo!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        #endregion

        public async Task<Dictionary<string, object>> GetProductFromDB(int id, string action)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            object cauThongBao = null;

            HttpResponseMessage response = await _productService.GetProduct(id);
            var responseData = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                if (action == "EditPhoto")
                {
                    cauThongBao = $"Error in finding Product to edit photo!<br /> Reason: {responseData}";
                } else if (action == "Delete")
                {
                    cauThongBao = $"Error in finding Product to delete!<br /> Reason: {responseData}";
                } else if (action == "EditProduct")
                {
                    cauThongBao = $"Error in finding Product to edit!<br /> Reason: {responseData}";
                }
                                
                result.Add("ErrObj", cauThongBao);
                result.Add("Product", null);
            }
            else
            {
                Product productFromDb = new Product();
                productFromDb = JsonConvert.DeserializeObject<Product>(responseData);
                if (productFromDb == null)
                {
                    cauThongBao = $"Cannot find Product with Id (" + id.ToString() + ").";

                    result.Add("ErrObj", cauThongBao);
                    result.Add("Product", null);
                }
                else
                {
                    result.Add("ErrObj", null);
                    result.Add("Product", productFromDb);
                }                
            }
            
            return result;
        }


    }
}