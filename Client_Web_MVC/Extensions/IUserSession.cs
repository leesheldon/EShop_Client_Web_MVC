using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_Web_MVC.Extensions
{
    public interface IUserSession
    {
        string Username { get; }
        string AccessToken { get; }

    }
}
