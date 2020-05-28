using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client_Web_MVC.Services
{
    public interface IProductService
    {
        Task<HttpResponseMessage> GetProductsList(string search_Data, string sort_Data,
            int pageIndex, int pageSize, int? brandId, int? typeId);

        Task<HttpResponseMessage> GetProduct(int? id);

        Task<HttpResponseMessage> GetBrandsList();

        Task<HttpResponseMessage> GetTypesList();

    }
}
