using Antlr.Runtime;
using Client_Web_MVC.Extensions;
using Client_Web_MVC.Models;
using Client_Web_MVC.Services;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Client_Web_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserSession _userSession;
        private readonly IAccountService _accountService;

        public AccountController(IUserSession userSession, IAccountService accountService)
        {
            _userSession = userSession;
            _accountService = accountService;
        }

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        #region Register
        public ActionResult Register()
        {
            var model = new Register();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(Register register)
        {
            try
            {
                HttpResponseMessage response = await _accountService.Register(register);
                var responseData = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    // Get registered User
                    User userRegisteredIn = JsonConvert.DeserializeObject<User>(responseData);

                    // Sign in
                    SignInCustomIdentity(userRegisteredIn);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.Clear();
                ModelState.AddModelError("", responseData);
                return View(register);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in register new user!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }            
        }

        #endregion

        #region Login
        public ActionResult Login(string returnURL)
        {
            var model = new Login();

            ViewBag.ReturnURL = returnURL;

            return View(model);
        }
                
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(Login login, string returnUrl)
        {
            try
            {
                HttpResponseMessage response = await _accountService.Login(login);
                var responseData = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    // Get logged in User
                    User userLoggedIn = JsonConvert.DeserializeObject<User>(responseData);

                    // Sign in
                    SignInCustomIdentity(userLoggedIn);

                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.Clear();
                ModelState.AddModelError("", "The username or password is incorrect!");
                return View(login);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in login user!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        #endregion

        public ActionResult LogOff()
        {
            Request.GetOwinContext().Authentication.SignOut("ApplicationCookie");
            
            var user = User as ClaimsPrincipal;
            var identity = user.Identity as ClaimsIdentity;
            
            foreach (var claim in identity.Claims)
            {
                identity.RemoveClaim(claim);
            }
            
            return RedirectToAction("Login");
        }

        public void SignInCustomIdentity(User userLoggedIn)
        {
            _accountService.SignInCustomIdentity(this, userLoggedIn);
                        
        }
                
    }
}