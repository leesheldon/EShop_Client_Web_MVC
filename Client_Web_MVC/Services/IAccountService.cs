using Client_Web_MVC.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Client_Web_MVC.Services
{
    public interface IAccountService
    {
        Task<HttpResponseMessage> Register(Register register);

        Task<HttpResponseMessage> Login(Login login);

        void SignInCustomIdentity(Controller ctrler, LoggedInUser userLoggedIn);

        Task<HttpResponseMessage> GetUsersList(string search_Data, string sort_Data, int pageIndex, int pageSize);

        Task<HttpResponseMessage> GetUser(string id);

        Task<HttpResponseMessage> LockUser(UserForCRUD userToLock);

        Task<HttpResponseMessage> UnLockUser(UserForCRUD userToLock);

        Task<HttpResponseMessage> DeleteUser(string id);

    }
}
