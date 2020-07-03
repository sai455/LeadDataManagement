using LeadDataManagement.Models.Context;
using LeadDataManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeadDataManagement.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private LeadDbContext _context = new LeadDbContext();
        private bool currentUserNameCached = false;
        private string currentUserName = null;
        private bool currentUserCached = false;
        private User currentUser = null;

        public User CurrentLoggedInUser
        {
            get
            {
                if (!this.currentUserCached)
                {
                    if (this.CurrentUserName != null)
                    {
                        string username = string.Empty;
                        username = this.CurrentUserName;
                        this.currentUser = _context.Users.FirstOrDefault(x => x.Email == username);
                    }

                    this.currentUserCached = true;
                }

                return this.currentUser;
            }
        }
        protected string CurrentUserName
        {
            get
            {
                if (!this.currentUserNameCached)
                {
                    if (this.User != null)
                    {
                        this.currentUserName = this.User.Identity.Name;
                    }
                    this.currentUserNameCached = true;
                }
                return this.currentUserName;
            }
        }
    }
}