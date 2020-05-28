using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client_Web_MVC.Models
{
    public class LoggedInUser
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Token { get; set; }

    }
}