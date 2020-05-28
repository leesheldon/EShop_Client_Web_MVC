using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Client_Web_MVC.Models;
using Client_Web_MVC.Extensions;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Web.Mvc;

namespace Client_Web_MVC.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserSession _userSession;

        public AccountService(IUserSession userSession)
        {
            _userSession = userSession;
        }

        public async Task<HttpResponseMessage> Login(Login login)
        {
            using (var client = new HttpClient())
            {
                string requestParams = SD.BaseUrl + "api/account/login";
                client.BaseAddress = new Uri(SD.BaseUrl);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                client.DefaultRequestHeaders.Clear();

                var contentLogin = new StringContent(JsonConvert.SerializeObject(login));
                contentLogin.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await client.PostAsync(requestParams, contentLogin);

                return response;
            }
        }

        public async Task<HttpResponseMessage> Register(Register register)
        {
            using (var client = new HttpClient())
            {
                string requestParams = SD.BaseUrl + "api/account/register";
                client.BaseAddress = new Uri(SD.BaseUrl);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                client.DefaultRequestHeaders.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var contentRegister = new StringContent(JsonConvert.SerializeObject(register));
                contentRegister.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await client.PostAsync(requestParams, contentRegister);

                return response;
            }
        }

        public void SignInCustomIdentity(Controller ctrler, LoggedInUser userLoggedIn)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(userLoggedIn.Token);

            // Get list of User roles
            string strUserRoles = "";
            List<Claim> roleClaims = jwtToken.Claims.Where(c => c.Type == "role").ToList();
            foreach (Claim role in roleClaims)
            {
                strUserRoles = strUserRoles + role.Value + ",";
            }

            if (strUserRoles[strUserRoles.Length - 1] == ',')
            {
                strUserRoles = strUserRoles.Remove(strUserRoles.Length - 1);
            }

            // Store user information and access token
            AuthenticationProperties options = new AuthenticationProperties();
            options.AllowRefresh = true;
            options.IsPersistent = true;
            options.ExpiresUtc = jwtToken.ValidTo;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userLoggedIn.DisplayName),
                //new Claim("AcessToken", string.Format("Bearer {0}", token["access_token"])),
                new Claim(ClaimTypes.NameIdentifier, userLoggedIn.Email),
                new Claim("AccessToken", userLoggedIn.Token),
                new Claim("UserRoles", strUserRoles),
            };

            var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            // Add roles to logged in User
            foreach (Claim role in roleClaims)
            {
                var newClaim = new Claim(ClaimTypes.Role, role.Value);
                identity.AddClaim(newClaim);
            }

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            Thread.CurrentPrincipal = principal;

            ctrler.Request.GetOwinContext().Authentication.SignIn(options, identity);
        }

        public async Task<HttpResponseMessage> GetUsersList(string search_Data, string sort_Data, int pageIndex, int pageSize)
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/account/userslist?pageSize=" + pageSize + "&pageIndex=" + pageIndex;

                if (!string.IsNullOrEmpty(search_Data))
                {
                    requestParams = requestParams + "&search=" + search_Data;
                }

                if (!string.IsNullOrEmpty(sort_Data))
                {
                    requestParams = requestParams + "&sort=" + sort_Data;
                }
                                
                client.BaseAddress = new Uri(SD.BaseUrl);
                client.DefaultRequestHeaders.Clear();
                
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                // Access token
                string accessToken = _userSession.AccessToken;

                var request = new HttpRequestMessage(HttpMethod.Get, requestParams);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                
                HttpResponseMessage response = await client.SendAsync(request);

                return response;
            }
        }

        public async Task<HttpResponseMessage> GetUser(string id)
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/account/user/" + id;

                client.BaseAddress = new Uri(SD.BaseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                // Access token
                string accessToken = _userSession.AccessToken;

                var request = new HttpRequestMessage(HttpMethod.Get, requestParams);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.SendAsync(request);

                return response;
            }
        }

        public async Task<HttpResponseMessage> LockUser(UserForCRUD userToLock)
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/account/" + userToLock.Id + "/lock";

                client.BaseAddress = new Uri(SD.BaseUrl);
                client.DefaultRequestHeaders.Clear();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                // Access token
                string accessToken = _userSession.AccessToken;

                var request = new HttpRequestMessage(HttpMethod.Post, requestParams);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = new StringContent(JsonConvert.SerializeObject(userToLock));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                return response;
            }
        }

        public async Task<HttpResponseMessage> UnLockUser(UserForCRUD userToLock)
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/account/" + userToLock.Id + "/unlock";

                client.BaseAddress = new Uri(SD.BaseUrl);
                client.DefaultRequestHeaders.Clear();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                // Access token
                string accessToken = _userSession.AccessToken;

                var request = new HttpRequestMessage(HttpMethod.Post, requestParams);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = new StringContent(JsonConvert.SerializeObject(userToLock));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                return response;
            }
        }

        public async Task<HttpResponseMessage> DeleteUser(string id)
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/account/" + id + "/delete";

                client.BaseAddress = new Uri(SD.BaseUrl);
                client.DefaultRequestHeaders.Clear();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                // Access token
                string accessToken = _userSession.AccessToken;

                var request = new HttpRequestMessage(HttpMethod.Delete, requestParams);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.SendAsync(request);
                return response;
            }
        }

    }
}