using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client_Web_MVC.Models.ViewModels
{
    public class Products_PhotosViewModel
    {
        public List<Product> Products { get; set; }

        public List<Photo> Photos { get; set; }

    }
}