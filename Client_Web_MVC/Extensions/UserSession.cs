using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Client_Web_MVC.Extensions
{
    public class UserSession : IUserSession
    {
        public string Username
        {
            get {
                if (HttpContext.Current == null) return "";
                if (HttpContext.Current.User == null) return "";
                if (((ClaimsPrincipal)HttpContext.Current.User).Claims.Count() <= 0) return "";
                if (((ClaimsPrincipal)HttpContext.Current.User).FindFirst(ClaimTypes.Name) == null) return "";

                return ((ClaimsPrincipal)HttpContext.Current.User).FindFirst(ClaimTypes.Name).Value;
            }
        }

        public string AccessToken
        {
            get {
                if (HttpContext.Current == null) return "";
                if (HttpContext.Current.User == null) return "";
                if (((ClaimsPrincipal)HttpContext.Current.User).Claims.Count() <= 0) return "";
                if (((ClaimsPrincipal)HttpContext.Current.User).FindFirst("AccessToken") == null) return "";

                return ((ClaimsPrincipal)HttpContext.Current.User).FindFirst("AccessToken").Value;
            }
        }

    }
}