using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client_Web_MVC.Models
{
    public class Test_ProductToUpdate_Test
    {
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public decimal Price { get; set; }

        public string PictureUrl { get; set; }
        
        public int ProductTypeId { get; set; }

        public ProductType ProductType { get; set; }

        public int ProductBrandId { get; set; }

        public ProductBrand ProductBrand { get; set; }

    }
}