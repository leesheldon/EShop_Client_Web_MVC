using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Client_Web_MVC.Models
{
    public class Register
    {
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }
                
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
                
        [DataType(DataType.Password, ErrorMessage = "Password must have 1 Uppercase, 1 Lowercase, 1 number, 1 non alphanumeric and at least 6 characters")]
        public string Password { get; set; }

    }
}