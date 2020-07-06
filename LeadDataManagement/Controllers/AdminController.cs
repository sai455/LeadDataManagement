using LeadDataManagement.Models.ViewModels;
using LeadDataManagement.Services.Interface;
using Newtonsoft.Json;
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
        private IUserScrubService userScrubService;
        public AdminController(IUserService _userService, ILeadService _leadService, IUserScrubService userScrubService)
        {
            this.userService = _userService;
            this.leadService = _leadService;
            this.userScrubService = userScrubService;
        }

        #region Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.CurrentUser = this.CurrentLoggedInUser;


            return View();
        }
        public ActionResult UserScrubsGrid(DateTime date)
        {

            var usersList = userService.GetUsers().ToList();
            var Leads = leadService.GetLeadTypes().ToList();
            List<UserScrubsGridModel> retData = new List<UserScrubsGridModel>();
            var userScrubs = userScrubService.GetAllUserScrubs().OrderByDescending(x=>x.CreatedDate).ToList().Where(x=>x.CreatedDate.Date==date.Date);
            int iCount = 0;
            foreach (var u in userScrubs)
            {
                iCount += 1;
                List<int> leadTypes = JsonConvert.DeserializeObject<List<DropDownModel>>(u.LeadTypeIds).Select(x => x.Id).ToList();
                string InputExtensions = u.InputFilePath.Split('.')[1];
                retData.Add(new UserScrubsGridModel()
                {
                    Sno = iCount,
                    UserName= usersList.Where(x=>x.Id==u.UserId).FirstOrDefault().Name,
                    ScrubCredits = u.ScrubCredits,
                    CreatedAt = u.CreatedDate.ToString("dd-MMM-yyyy hh:mm:ss tt"),
                    LeadType = String.Join(",", Leads.Where(x => leadTypes.Contains(x.Id)).Select(x => x.Name).ToList()),
                    Matched = "Input File  <a href='" + u.InputFilePath + "' style='cursor:pointer' download='InputFile-" + u.Id + "." + InputExtensions + "'><i class='fa fa-download' ></i></a><br>"+"Matched- " + u.MatchedCount + " <a href='" + u.MatchedPath + ".csv' style='cursor:pointer' download='Matched-" + u.Id + ".csv'><i class='fa fa-download' ></i></a><br>"+ "Un-Matched- " + u.UnMatchedCount + " <a href='" + u.UnMatchedPath + ".csv' style='cursor:pointer' download='UnMatched-" + u.Id + ".csv'><i class='fa fa-download' ></i></a>",
                });
            }
            var jsonData = new { data = from emp in retData select emp };
            return new JsonResult()
            {
                Data = jsonData,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue
            };
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
            var leadList = leadService.GetLeadMasterdataGridList(leadTypeId);
            int iCount = 0;
            foreach (var l in leadList)
            {
                iCount += 1;
                retData.Add(new LeadMasterDataGridViewModel()
                {
                    SNo = iCount,
                    Id = l.Id,
                    PhoneCount = l.Count,
                    LeadType=l.Name,
                    EditBtn = "<button type='button' class='btn btn-primary m-b-10 btnView btn-sm' data-name='"+l.Name+"' data-id='" + l.Id + "' ><i class='fa fa-eye'></i> View Data</button>"
                }); ;
            }
            var jsonData = new { data = from emp in retData select emp };
            return new JsonResult()
            {
                Data = jsonData,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue // Use this value to set your maximum size for all of your Requests
            };
        }
        public JsonResult GetViewList(int leadTypeId)
        {
            var data = leadService.GetAllLeadMasterDataByLeadType(leadTypeId).Select(x=>x.Phone).ToArray();
            string retData = string.Join(", ", data);
            return new JsonResult()
            {
                Data = retData,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue 
            };
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
                    List<long> PhoneNo = lines.Select(x => Convert.ToInt64(x.Replace(",","").Trim())).Take(200000).ToList();
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