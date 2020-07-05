using LeadDataManagement.Models.ViewModels;
using LeadDataManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;
namespace LeadDataManagement.Controllers
{
    public class UsersController : BaseController
    {
        private IUserScrubService userScrubService;
        private IUserService userService;
        private ILeadService leadService;
        public UsersController(IUserScrubService _userScrubService,IUserService _userService, ILeadService _leadService)
        {
            userScrubService = _userScrubService;
            userService = _userService;
            leadService = _leadService;
        }
        public ActionResult Dashboard()
        {
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            return View();
        }

        public ActionResult Scrubber()
        {
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            ViewBag.LeadTypesList = leadService.GetLeadTypes().ToList().Select(x => new DropDownModel()
            {
                Name = x.Name,
                Id = x.Id
            }).OrderBy(x => x.Name).ToList();
            return View();
        }

        public ActionResult UserScrubsGrid(int? leadTypeId)
        {
            var Leads = leadService.GetLeadTypes().ToList();
            List<UserScrubsGridModel> retData = new List<UserScrubsGridModel>();
            var userScrubs = userScrubService.GetScrubsByUserId(this.CurrentLoggedInUser.Id);
            if(leadTypeId.HasValue)
            {
                userScrubs = userScrubs.Where(x => x.LeadTypeId == leadTypeId.Value).ToList();
            }
            int iCount = 0;
            foreach(var u in userScrubs)
            {
                iCount += 1;
                var matchedPath = Path.Combine(Server.MapPath("~/Content/DataLoads/"), u.MatchedPath);
                retData.Add(new UserScrubsGridModel()
                {
                    Sno = iCount,
                    CreatedAt=u.CreatedDate.ToString("dd-MMM-yyyy hh:mm:ss tt"),
                    LeadTypeId = u.LeadTypeId,
                    LeadType = Leads.Where(x => x.Id == u.LeadTypeId).FirstOrDefault().Name,
                    Matched = "Matched- " + u.MatchedCount + " <a href='"+u.MatchedPath+ ".csv' style='cursor:pointer' download='Matched-"+u.Id+".csv'><i class='fa fa-download' ></i></a>",
                    UnMatched = "Un-Matched- " + u.UnMatchedCount + " <a href='" + u.UnMatchedPath + ".csv' style='cursor:pointer' download='UnMatched-"+u.Id+".csv'><i class='fa fa-download' ></i></a>",
                    Duration = u.Duration,
                    InputFile = "Download Input File  <a href='" + u.InputFilePath + ".csv' style='cursor:pointer' download='InputFile-"+u.Id+".csv'><i class='fa fa-download' ></i></a>",
                }); ;
            }
            var jsonData = new { data = from emp in retData select emp };
            return new JsonResult()
            {
                Data = jsonData,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue 
            };
        }

        public ActionResult PerformUserScrub(FormCollection formCollection, int LeadTypeId,string PhoneNos)
        {
            Stopwatch sw = Stopwatch.StartNew();
           
            if (string.IsNullOrEmpty(PhoneNos))
            {
                HttpPostedFileBase file = Request.Files["ScrubFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string ext = Path.GetExtension(file.FileName);
                    string newFileName = Guid.NewGuid().ToString();
                    string path = Path.Combine(Server.MapPath("~/Content/DataLoads/"), newFileName + ext);
                    file.SaveAs(path);
                    List<string> inputList = new List<string>();
                    if (ext.ToLower() == ".csv")
                    {
                        StreamReader myFile = new StreamReader(path);
                        string myString = myFile.ReadToEnd();
                        myFile.Close();
                        string[] lines = myString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        inputList = lines.Skip(1).ToList();
                    }else if(ext.ToLower() == ".xl")
                    {
                        Excel.Application xlApp;
                        Excel.Workbook xlWorkBook;
                        Excel.Worksheet xlWorkSheet;
                        Excel.Range range;

                        string str;
                        int rCnt;
                        int cCnt;
                        int rw = 0;
                        int cl = 0;

                        xlApp = new Excel.Application();
                        xlWorkBook = xlApp.Workbooks.Open(path, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                        xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                        range = xlWorkSheet.UsedRange;
                        rw = range.Rows.Count;
                        cl = range.Columns.Count;
                       
                        for (rCnt = 1; rCnt <= rw; rCnt++)
                        {
                            
                        }
                         xlWorkBook.Close(true, null, null);
                        xlApp.Quit();

                        Marshal.ReleaseComObject(xlWorkSheet);
                        Marshal.ReleaseComObject(xlWorkBook);
                        Marshal.ReleaseComObject(xlApp);

                    }

                    if (inputList.Count > 0)
                    {
                        List<long> UserScrubPhonesList = new List<long>();
                        foreach (var i in inputList)
                        {
                            long number;
                            bool isSuccess = Int64.TryParse(i, out number);
                            if (isSuccess)
                            {
                                UserScrubPhonesList.Add(number);
                            }
                        }
                        var matchedList = leadService.GetAllLeadMasterDataByLeadType(LeadTypeId).Select(x => x.Phone).Where(x => UserScrubPhonesList.Contains(x)).ToList();
                        var unmatchedCount = UserScrubPhonesList.Except(matchedList).ToList();

                        sw.Stop();
                        //Matched File Create
                        string matchedFileName = Guid.NewGuid().ToString();
                        CreateSaveCsvFile(matchedFileName, matchedList);

                        // UnMatched File Create
                        string unMatchedFileName = Guid.NewGuid().ToString();
                        CreateSaveCsvFile(unMatchedFileName, unmatchedCount);

                        userScrubService.SaveUserScrub(this.CurrentLoggedInUser.Id, LeadTypeId, matchedList.Count(), unmatchedCount.Count(), matchedFileName, unMatchedFileName, newFileName+".csv", sw.Elapsed.Seconds);
                    }
                  
                }
            }
            else
            {
                List<string> inputList = PhoneNos.Split(',').ToList();

                List<long> UserScrubPhonesList = new List<long>();
                foreach (var i in inputList)
                {
                    long number;
                    bool isSuccess = Int64.TryParse(i, out number);
                    if (isSuccess)
                    {
                        UserScrubPhonesList.Add(number);
                    }
                }
                var matchedList = leadService.GetAllLeadMasterData().Where(x => x.LeadTypeId == LeadTypeId && UserScrubPhonesList.Contains(x.Phone)).Select(x => x.Phone).ToList();
                var unmatchedCount = UserScrubPhonesList.Except(matchedList).ToList();
                
                sw.Stop();

                //InputFile Create
                string inputFileName = Guid.NewGuid().ToString();
                CreateSaveCsvFile(inputFileName, UserScrubPhonesList);

                //Matched File Create
                string matchedFileName = Guid.NewGuid().ToString();
                CreateSaveCsvFile(matchedFileName, matchedList);

                // UnMatched File Create
                string unMatchedFileName = Guid.NewGuid().ToString();
                CreateSaveCsvFile(unMatchedFileName, unmatchedCount);

                userScrubService.SaveUserScrub(this.CurrentLoggedInUser.Id, LeadTypeId, matchedList.Count(), unmatchedCount.Count(), matchedFileName, unMatchedFileName, inputFileName, sw.Elapsed.Seconds);
            }
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            ViewBag.LeadTypesList = leadService.GetLeadTypes().ToList().Select(x => new DropDownModel()
            {
                Name = x.Name,
                Id = x.Id
            }).OrderBy(x => x.Name).ToList();
            return RedirectToAction("Scrubber");
        }

        private void CreateSaveCsvFile(string newFileName, List<long> UserScrubPhonesList)
        {
            var file = Path.Combine(Server.MapPath("~/Content/DataLoads/"), newFileName + ".csv");
            using (var stream = System.IO.File.CreateText(file))
            {
                string csvRow = string.Format("{0}", "Phone");
                stream.WriteLine(csvRow);
                for (int i = 0; i < UserScrubPhonesList.Count(); i++)
                {
                    csvRow = string.Format("{0}", UserScrubPhonesList[i].ToString());
                    stream.WriteLine(csvRow);
                }
            }
        }
    }
}