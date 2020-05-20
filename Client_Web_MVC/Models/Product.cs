using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Client_Web_MVC.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PictureUrl { get; set; }

        [Display(Name = "Product Type")]
        public int ProductTypeId { get; set; }

        public ProductType ProductType { get; set; }

        [Display(Name = "Product Brand")]
        public int ProductBrandId { get; set; }

        public ProductBrand ProductBrand { get; set; }

        public IReadOnlyList<Photo> Photos { get; set; }

    }
}