using LeadDataManagement.Models.Context;
using LeadDataManagement.Models.ViewModels;
using LeadDataManagement.Services.Interface;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace LeadDataManagement.Controllers
{
    public class LoginController : Controller
    {
        private IUserService _userService;
        public LoginController(IUserService userService)
        {
            _userService = userService;
        }
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string email, string password)
        {
            var userData = _userService.ValidateUserByEmailId(email,password);
            if (userData!=null)
            {
                if (userData.StatusId == 1)//Pending Activation
                {
                    ModelState.AddModelError("", "Pending for activation, please contact admin");
                }
                else if(userData.StatusId==3)//InActive
                {
                    ModelState.AddModelError("", "User is Inactive, please contact admin");
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(email, true);
                    if(userData.IsAdmin)
                     return RedirectToAction("Dashboard", "Admin");
                    else
                        return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError("", "Incorrect Email & Password");
            }
            return View();
        }

        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Signup(UserViewModel user)
        {
            if (ModelState.IsValid)
            {
                if(_userService.GetUsers().Where(x=>x.Email== user.Email).Any())
                {
                    ModelState.AddModelError("Email", "Email already in use.");
                    return View();
                }
                else
                {
                    _userService.SaveUser(user);
                    return RedirectToAction("SignUpCompleted", "Login");
                }
            }
            return View();
        }


        public ActionResult SignUpCompleted()
        {
             return View();
        }

        public ActionResult ForgotPassword()
        {

            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string email,string password,string confirmpassword)
        {
            if (_userService.GetUsers().Where(x => x.Email == email).Any())
            {
                if(password!=confirmpassword)
                {
                    ModelState.AddModelError("", "Confirm Password and Password do not match");
                    return View();
                }
                _userService.UpdateUserPassword(email, password);
                return RedirectToAction("ResetPasswordCompleted", "Login");
            }
            else
            {
                ModelState.AddModelError("", "Email does not exists");
            }
            return View();
        }
        public ActionResult ResetPasswordCompleted()
        {
            return View();
        }

        public RedirectToRouteResult LogOut()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }

        
    }
}