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

        public void SignInCustomIdentity(Controller ctrler, User userLoggedIn)
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
    }
}