using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client_Web_MVC.Extensions
{
    public static class SD
    {
        // Roles Name
        public const string AdminEndUser = "Admin";

        public const string MemberEndUser = "Member";

        // Hosted web API REST Service base Url
        public const string BaseUrl = "https://localhost:5001/";

        // Url of placeholder image in Physical location
        public const string Url_Pic_PlaceHolder = "Content/images/products/placeholder.png";

        // Url of placeholder image which saved into Database
        public const string Url_Pic_PlaceHolder_SavedToDb = "images/products/placeholder.png";

        public static bool IsAdmin(string strUserRoles)
        {
            string[] roles = strUserRoles.Split(',');
            foreach (string role in roles)
            {
                if (role.Trim().ToLower() == "admin")
                {
                    return true;
                }
            }

            return false;
        }

    }
}