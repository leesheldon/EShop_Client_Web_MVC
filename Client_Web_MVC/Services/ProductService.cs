using Client_Web_MVC.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net;

namespace Client_Web_MVC.Services
{
    public class ProductService : IProductService
    {
        private readonly IUserSession _userSession;

        public ProductService(IUserSession userSession)
        {
            _userSession = userSession;
        }

        public async Task<HttpResponseMessage> GetBrandsList()
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/products/brands/";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                                
                client.BaseAddress = new Uri(SD.BaseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                
                HttpResponseMessage response = await client.GetAsync(requestParams);

                return response;
            }
        }

        public async Task<HttpResponseMessage> GetProduct(int? id)
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/products/" + id.ToString();
                
                client.BaseAddress = new Uri(SD.BaseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); ;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                HttpResponseMessage response = await client.GetAsync(requestParams);

                return response;
            }
        }

        public async Task<HttpResponseMessage> GetProductsList(string search_Data, int pageIndex, int pageSize, int? brandId, int? typeId)
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/products?pageSize=" + pageSize + "&pageIndex=" + pageIndex;

                if (!string.IsNullOrEmpty(search_Data))
                {
                    requestParams = requestParams + "&search=" + search_Data;
                }

                if (brandId > -1)
                {
                    requestParams = requestParams + "&brandId=" + brandId.ToString();
                }

                if (typeId > -1)
                {
                    requestParams = requestParams + "&typeId=" + typeId.ToString();
                }

                //Passing service base url  
                client.BaseAddress = new Uri(SD.BaseUrl);
                client.DefaultRequestHeaders.Clear();
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                //Sending request to find web api REST service resource using HttpClient  
                HttpResponseMessage response = await client.GetAsync(requestParams);

                return response;
            }
        }

        public async Task<HttpResponseMessage> GetTypesList()
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/products/types/";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                
                client.BaseAddress = new Uri(SD.BaseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                HttpResponseMessage response = await client.GetAsync(requestParams);

                return response;
            }
        }

    }
}