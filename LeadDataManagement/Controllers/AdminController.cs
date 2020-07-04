using LeadDataManagement.Models.ViewModels;
using LeadDataManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeadDataManagement.Controllers
{
    public class AdminController : BaseController
    {
        private IUserService userService;
        private ILeadService leadService;
        public AdminController(IUserService _userService, ILeadService _leadService)
        {
            this.userService = _userService;
            this.leadService = _leadService;
        }

        #region Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.CurrentUser = this.CurrentLoggedInUser;


            return View();
        }
       
        #endregion

        #region Users
        public ActionResult Users()
        {
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            return View();
        }
        public ActionResult UsersGrid()
        {
            List<UserGridViewModel> retData = new List<UserGridViewModel>();
            retData = userService.GetUsers().Where(x => x.IsAdmin == false).ToList().Select(x => new UserGridViewModel 
            { 
                Id=x.Id,
                Name=x.Name,
                Email=x.Email,
                Phone=x.Phone,
                Password=x.Password,
                CreatedAt=x.CreatedAt.ToString("dd-MMM-yyyy hh:mm:ss tt"),
                CreditScore=x.CreditScore,
                Status= userService.GetStatusById(x.StatusId),
                ModifiedAt=x.ModifiedAt.HasValue?x.ModifiedAt.Value.ToString("dd-MMM-yyyy hh:mm:ss tt"):string.Empty,
                StatusId=x.StatusId
            }).ToList();
            int iCount = 0;
            foreach(var r in retData)
            {
                iCount += 1;
                r.SNo = iCount;
                if(r.StatusId==1)
                {
                    r.EditBtn = "<button type='button' class='btn btn-success m-b-10 btnapprove btn-sm' data-id='" + r.Id+"' data-score='"+r.CreditScore+"'>Approve</button>";
                }else if(r.StatusId==2)
                {
                    r.EditBtn = "<button type='button' class='btn btn-danger m-b-10 btninactivate btn-sm' data-id='" + r.Id + "' data-score='" + r.CreditScore + "'>In-Activate</button>";
                }
                else
                {
                    r.EditBtn = "<button type='button' class='btn btn-primary m-b-10 btnactivate btn-sm' data-id='" + r.Id + "' data-score='" + r.CreditScore + "'>Activate</button>";
                }
            }
            var jsonData = new { data = from emp in retData select emp };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateUserStatus(int userId,long creditScore,int statusId)
        {
            try
            {
                userService.UpdateUserStatus(userId, creditScore, statusId);
                
            }catch(Exception ex)
            {
                throw ex;
            }
            return Json("",JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Lead Types

        public ActionResult LeadTypes()
        {
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            return View();
        }
        public ActionResult LeadTypeGrid()
        {
            List<LeadTypeGridViewModel> retData = new List<LeadTypeGridViewModel>();
            var leadTypesList = leadService.GetLeadTypes().OrderBy(x => x.Name).ToList();
            int iCount = 0;
            foreach (var l in leadTypesList)
            {
                iCount += 1;
                retData.Add(new LeadTypeGridViewModel()
                {
                    LeadType=l.Name,
                    SNo=iCount,
                    Status=l.IsActive==true?"Active":"InActive",
                    Id=l.Id,
                    CreatedAt=l.CreatedAt.ToString("dd-MMM-yyyy hh:mm:ss tt"),
                    IsActive=l.IsActive,
                    EditBtn = "<button type='button' class='btn btn-primary m-b-10 btnedit btn-sm' data-lead='"+l.Name+"' data-id='" + l.Id + "'><i class='fa fa-pencil-square-o'></i> Edit</button>"
                });
            }
            var jsonData = new { data = from emp in retData select emp };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddEditLeadType(int id,string leadType)
        {
            if(leadService.GetLeadTypes().Any(x=>x.Name.ToLower()==leadType.ToLower() && x.Id!=id))
            {
                return Json("Duplicate lead type not allowed", JsonRequestBehavior.AllowGet);
            }
            leadService.AddEditLeadTypes(id, leadType);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Lead Data
        public ActionResult LeadMasterData()
        {
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            ViewBag.LeadTypesList = leadService.GetLeadTypes().ToList().Select(x => new DropDownModel()
            {
                Name=x.Name,
                Id=x.Id
            }).OrderBy(x=>x.Name).ToList();
            return View();
        }
        public ActionResult LeadMasterDataGrid(int? leadTypeId)
        {
            var leadTypesList = leadService.GetLeadTypes().ToList();
            List<LeadMasterDataGridViewModel> retData = new List<LeadMasterDataGridViewModel>();
            var leadList = leadService.GetAllLeadMasterData().ToList();
            if(leadTypeId.HasValue)
            {
                leadList = leadList.Where(x => x.LeadTypeId == leadTypeId).ToList();
            }
            int iCount = 0;
            foreach (var l in leadList)
            {
                iCount += 1;
                retData.Add(new LeadMasterDataGridViewModel()
                {
                    LeadType = leadTypesList.FirstOrDefault(x=>x.Id==l.LeadTypeId).Name,
                    LeadTypeId=l.LeadTypeId,
                    Phone=l.Phone,
                    SNo = iCount,
                    Status = l.IsActive == true ? "Active" : "InActive",
                    Id = l.Id,
                    CreatedAt = l.CreatedAt.ToString("dd-MMM-yyyy hh:mm:ss tt"),
                    IsActive = l.IsActive,
                    EditBtn = "<button type='button' class='btn btn-primary m-b-10 btnedit btn-sm' data-lead='" + l.Phone + "' data-id='" + l.Id + "'><i class='fa fa-pencil-square-o'></i> Edit</button>"
                });
            }
            var jsonData = new { data = from emp in retData select emp };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult UploadMasterData(FormCollection formCollection,int LeadTypeId)
        {
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["MasterLoadFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string path = Server.MapPath("~/Content/DataLoads/" + file.FileName);
                    file.SaveAs(path);
                    
                    string[] lines = System.IO.File.ReadAllLines(path);
                    List<long> PhoneNo = lines.Select(x => Convert.ToInt64(x.Replace(",","").Trim())).ToList();
                    leadService.SaveMasterData(PhoneNo, LeadTypeId);
                }
            }
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            ViewBag.LeadTypesList = leadService.GetLeadTypes().ToList().Select(x => new DropDownModel()
            {
                Name = x.Name,
                Id = x.Id
            }).OrderBy(x => x.Name).ToList();
            return View("LeadMasterData");
        }
        #endregion
    }
}