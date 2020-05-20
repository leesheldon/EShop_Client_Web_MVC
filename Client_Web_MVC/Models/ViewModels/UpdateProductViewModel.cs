using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client_Web_MVC.Models.ViewModels
{
    public class UpdateProductViewModel
    {
        public List<Product> Products { get; set; }

        public List<ProductBrand> Product { get; set; }


    }
}