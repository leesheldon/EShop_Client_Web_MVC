using Antlr.Runtime;
using Client_Web_MVC.Extensions;
using Client_Web_MVC.Models;
using Client_Web_MVC.Services;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using PagedList;
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
        public async Task<ActionResult> Index(string search_Data, string sort_Data, int? page = 1)
        {
            ViewBag.UsersAct = "active";
            List<UserForCRUD> usersList = new List<UserForCRUD>();
            int totalItems = 0;
            int pageSize = 5;
            int pageIndex = (page ?? 1);

            try
            {
                HttpResponseMessage response = await _accountService.GetUsersList(search_Data, sort_Data, pageIndex, pageSize);

                var responseData = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    ResponseObj resObj = JsonConvert.DeserializeObject<ResponseObj>(responseData);

                    object cauThongBao = "Error in getting list of Users!<br /> Reason: " + resObj.Message;
                    return View("Error", cauThongBao);
                }

                Pagination<UserForCRUD> pagination = JsonConvert.DeserializeObject<Pagination<UserForCRUD>>(responseData);
                if (pagination == null)
                {
                    object cauThongBao = $"There is no User in list! Pagination is null.";
                    return View("Error", cauThongBao);
                }

                usersList = pagination.Data.ToList<UserForCRUD>();
                totalItems = pagination.Count;

                // Paging
                var usersAsIPagedList = new StaticPagedList<UserForCRUD>(usersList, pageIndex, pageSize, totalItems);
                ViewBag.OnePageOfUsers = usersAsIPagedList;

                // Sort by list
                ViewBag.SortsList = new SelectList(
                    new List<SelectListItem>
                    {
                        new SelectListItem { Text = "&amp;#xf15d;", Value = "displaynameAsc" },
                        new SelectListItem { Text = "&#xf15d; User Name", Value = "usernameAsc" },
                        new SelectListItem { Text = "&#xf15e; User Name", Value = "usernameDesc" },
                    }, "Value", "Text");

                return View(usersList);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in getting list of Users!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }            
        }

        #region Edit User
        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            try
            {
                UserForCRUD userFromDb = new UserForCRUD();
                HttpResponseMessage response = await _accountService.GetUser(id);

                var responseData = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {                    
                    return View("Error", $"Error in finding User to edit!<br />");
                }
                else
                {                    
                    userFromDb = JsonConvert.DeserializeObject<UserForCRUD>(responseData);
                    if (userFromDb == null)
                    {
                        return View("Error", $"Cannot find User with Id (" + id + ").");
                    }

                    // Found User
                    userFromDb.StringLockoutEnd = !userFromDb.LockoutEnd.HasValue ? "" : userFromDb.LockoutEnd.Value.ToString("dd/MM/yyyy hh:mm:ss tt");
                }
                                                                
                return View(userFromDb);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in editing User!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserForCRUD userToUpdate)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Convert to Date Time for Lockout End
                    if (!string.IsNullOrEmpty(userToUpdate.StringLockoutEnd))
                    {
                        string[] dateAndTime = userToUpdate.StringLockoutEnd.Split(' '); // get dd/MM/yyyy hh:mm:ss AM/PM
                        string[] dmy = dateAndTime[0].Split('/'); // get dd/MM/yyyy
                        string[] hms = dateAndTime[1].Split(':'); // get hh:mm:ss

                        int dd = Convert.ToInt32(dmy[0]);
                        int mm = Convert.ToInt32(dmy[1]);
                        int yy = Convert.ToInt32(dmy[2]);

                        int hh = Convert.ToInt32(hms[0]);
                        int mi = Convert.ToInt32(hms[1]);
                        int ss = Convert.ToInt32(hms[2]);

                        if (dateAndTime[2] == "PM")
                        {
                            hh = hh + 12;
                        }

                        DateTime dt = new DateTime(yy, mm, dd, hh, mi, ss);
                        userToUpdate.LockoutEnd = dt;
                    }

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(SD.BaseUrl);

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                            SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                        client.DefaultRequestHeaders.Clear();

                        // Access token
                        string accessToken = "";
                        accessToken = _userSession.AccessToken;

                        // Edit User
                        string requestParams = SD.BaseUrl + "api/account/" + userToUpdate.Id + "/update";
                        var request = new HttpRequestMessage(HttpMethod.Put, requestParams);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        request.Content = new StringContent(JsonConvert.SerializeObject(userToUpdate));
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        HttpResponseMessage response = await client.SendAsync(request);
                        var responseData = response.Content.ReadAsStringAsync().Result;
                        if (response.IsSuccessStatusCode)
                        {
                            // Edit User successfully
                            return RedirectToAction("Index");
                        }

                        // Edit User failed
                        ResponseObj resObj = JsonConvert.DeserializeObject<ResponseObj>(responseData);

                        ModelState.Clear();
                        ModelState.AddModelError("", "Error in editing User! Reason: " + resObj.Message);
                    }
                }
                else
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", $"Error in editing User on form!");
                }

                return View(userToUpdate);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in editing User!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        #endregion

        #region Delete User
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            try
            {
                UserForCRUD userFromDb = new UserForCRUD();
                HttpResponseMessage response = await _accountService.GetUser(id);

                var responseData = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    return View("Error", $"Error in finding User to Delete!<br />");
                }
                else
                {
                    userFromDb = JsonConvert.DeserializeObject<UserForCRUD>(responseData);
                    if (userFromDb == null)
                    {
                        return View("Error", $"Cannot find User with Id (" + id + ").");
                    }

                    // Found User
                }

                return View(userFromDb);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in Deleting User!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(UserForCRUD userToDelete)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    HttpResponseMessage response = await _accountService.DeleteUser(userToDelete.Id);
                    var responseData = response.Content.ReadAsStringAsync().Result;
                    if (response.IsSuccessStatusCode)
                    {
                        // Delete User successfully                        
                        return RedirectToAction("Index");
                    }

                    // Delete User failed
                    ResponseObj resObj = JsonConvert.DeserializeObject<ResponseObj>(responseData);

                    ModelState.Clear();
                    ModelState.AddModelError("", "Error in Deleting User! Reason: " + resObj.Message);
                }
                else
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", $"Error in deleting User on form!");
                }

                return View(userToDelete);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in Deleting User!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        #endregion

        #region Lock User
        public async Task<ActionResult> Lock(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            try
            {
                UserForCRUD userFromDb = new UserForCRUD();
                HttpResponseMessage response = await _accountService.GetUser(id);

                var responseData = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    return View("Error", $"Error in finding User to Lock!<br />");
                }
                else
                {
                    userFromDb = JsonConvert.DeserializeObject<UserForCRUD>(responseData);
                    if (userFromDb == null)
                    {
                        return View("Error", $"Cannot find User with Id (" + id + ").");
                    }

                    // Found User
                }

                return View(userFromDb);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in Locking User!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Lock(UserForCRUD userToLock)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Convert to Date Time for Lockout End
                    if (!string.IsNullOrEmpty(userToLock.StringLockoutEnd))
                    {
                        string[] dateAndTime = userToLock.StringLockoutEnd.Split(' '); // get dd/MM/yyyy hh:mm:ss AM/PM
                        string[] dmy = dateAndTime[0].Split('/'); // get dd/MM/yyyy
                        string[] hms = dateAndTime[1].Split(':'); // get hh:mm:ss

                        int dd = Convert.ToInt32(dmy[0]);
                        int mm = Convert.ToInt32(dmy[1]);
                        int yy = Convert.ToInt32(dmy[2]);

                        int hh = Convert.ToInt32(hms[0]);
                        int mi = Convert.ToInt32(hms[1]);
                        int ss = Convert.ToInt32(hms[2]);

                        if (dateAndTime[2] == "PM")
                        {
                            hh = hh + 12;
                        }

                        DateTime dt = new DateTime(yy, mm, dd, hh, mi, ss);
                        userToLock.LockoutEnd = dt;
                    }
                    else
                    {
                        userToLock.LockoutEnd = DateTime.Now.AddYears(100);
                    }

                    HttpResponseMessage response = await _accountService.LockUser(userToLock);
                    var responseData = response.Content.ReadAsStringAsync().Result;
                    if (response.IsSuccessStatusCode)
                    {
                        // Lock User successfully                        
                        return RedirectToAction("Index");
                    }

                    // Lock User failed
                    ResponseObj resObj = JsonConvert.DeserializeObject<ResponseObj>(responseData);

                    ModelState.Clear();
                    ModelState.AddModelError("", "Error in locking User! Reason: " + resObj.Message);
                }
                else
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", $"Error in locking User on form!");
                }

                return View(userToLock);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in Locking User!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        #endregion
        
        #region Unlock User
        public async Task<ActionResult> UnLock(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            try
            {
                UserForCRUD userFromDb = new UserForCRUD();
                HttpResponseMessage response = await _accountService.GetUser(id);

                var responseData = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    return View("Error", $"Error in finding User to UnLock!<br />");
                }
                else
                {
                    userFromDb = JsonConvert.DeserializeObject<UserForCRUD>(responseData);
                    if (userFromDb == null)
                    {
                        return View("Error", $"Cannot find User with Id (" + id + ").");
                    }

                    // Found User
                }

                return View(userFromDb);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in Unlocking User!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UnLock(UserForCRUD userToUnLock)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    HttpResponseMessage response = await _accountService.UnLockUser(userToUnLock);
                    var responseData = response.Content.ReadAsStringAsync().Result;
                    if (response.IsSuccessStatusCode)
                    {
                        // UnLock User successfully                        
                        return RedirectToAction("Index");
                    }

                    // UnLock User failed
                    ResponseObj resObj = JsonConvert.DeserializeObject<ResponseObj>(responseData);

                    ModelState.Clear();
                    ModelState.AddModelError("", "Error in Unlocking User! Reason: " + resObj.Message);
                }
                else
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", $"Error in Unlocking User on form!");
                }

                return View(userToUnLock);
            }
            catch (Exception ex)
            {
                object cauThongBao = $"Error in UnLocking User!<br /> Reason: {ex.Message}";
                return View("Error", cauThongBao);
            }
        }

        #endregion

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
                    LoggedInUser userRegisteredIn = JsonConvert.DeserializeObject<LoggedInUser>(responseData);

                    // Sign in
                    SignInCustomIdentity(userRegisteredIn);

                    return RedirectToAction("Index", "Home");
                }

                ResponseObj resObj = JsonConvert.DeserializeObject<ResponseObj>(responseData);

                ModelState.Clear();
                ModelState.AddModelError("", "Error in register User! Reason: " + resObj.Message);
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
                    LoggedInUser userLoggedIn = JsonConvert.DeserializeObject<LoggedInUser>(responseData);

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

        public void SignInCustomIdentity(LoggedInUser userLoggedIn)
        {
            _accountService.SignInCustomIdentity(this, userLoggedIn);
                        
        }
                
    }
}