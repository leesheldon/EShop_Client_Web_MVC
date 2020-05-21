using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client_Web_MVC.Models.ViewModels
{
    public class Product_PhotosViewModel
    {
        public Product Product { get; set; }

        public List<Photo> Photos { get; set; }

    }
}