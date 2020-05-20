using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client_Web_MVC.Models.ViewModels
{
    public class ProductsViewModel
    {
        public IReadOnlyList<Product> Products { get; set; }

        public List<Photo> Photos { get; set; }

    }
}