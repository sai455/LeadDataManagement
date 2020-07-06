using ExcelDataReader;
using LeadDataManagement.Models.ViewModels;
using LeadDataManagement.Services.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

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
            ViewBag.IsFileError = "false";
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            ViewBag.LeadTypesList = leadService.GetLeadTypes().ToList().Select(x => new DropDownModel()
            {
                Name = x.Name,
                Id = x.Id
            }).OrderBy(x => x.Name).ToList();
            return View();
        }

        public ActionResult UserScrubsGrid()
        {
            var Leads = leadService.GetLeadTypes().ToList();
            List<UserScrubsGridModel> retData = new List<UserScrubsGridModel>();
            var userScrubs = userScrubService.GetScrubsByUserId(this.CurrentLoggedInUser.Id);
            int iCount = 0;
            foreach(var u in userScrubs)
            {
                iCount += 1;
                List<int>leadTypes = JsonConvert.DeserializeObject<List<DropDownModel>>(u.LeadTypeIds).Select(x=>x.Id).ToList();
                string InputExtensions = u.InputFilePath.Split('.')[1];
                retData.Add(new UserScrubsGridModel()
                {
                    Sno = iCount,
                    ScrubCredits=u.ScrubCredits,
                    CreatedAt =u.CreatedDate.ToString("dd-MMM-yyyy hh:mm:ss tt"),
                    LeadType = String.Join(",",Leads.Where(x => leadTypes.Contains(x.Id)).Select(x=>x.Name).ToList()),
                    Matched = "Matched- " + u.MatchedCount + " <a href='"+u.MatchedPath+ ".csv' style='cursor:pointer' download='Matched-"+u.Id+".csv'><i class='fa fa-download' ></i></a>",
                    UnMatched = "Un-Matched- " + u.UnMatchedCount + " <a href='" + u.UnMatchedPath + ".csv' style='cursor:pointer' download='UnMatched-"+u.Id+".csv'><i class='fa fa-download' ></i></a>",
                    Duration = u.Duration,
                    InputFile = "Download Input File  <a href='" + u.InputFilePath + "' style='cursor:pointer' download='InputFile-"+u.Id+"."+ InputExtensions + "'><i class='fa fa-download' ></i></a>",
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

        public ActionResult PerformUserScrub(FormCollection formCollection,string PhoneNos,string SelectedLeads)
        {
            bool isError = false;
            try
            {

                Stopwatch sw = Stopwatch.StartNew();
                List<int> selectedLeads = JsonConvert.DeserializeObject<List<DropDownModel>>(SelectedLeads).Select(x=>x.Id).ToList();
                if (string.IsNullOrEmpty(PhoneNos))
                {
                    HttpPostedFileBase file = Request.Files["ScrubFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string ext = Path.GetExtension(file.FileName);
                        string newFileName = Guid.NewGuid().ToString();
                        string path = Path.Combine(Server.MapPath("~/Content/DataLoads/"), newFileName + ext);
                        file.SaveAs(path);
                        List<long> UserScrubPhonesList = new List<long>();
                        if (ext.ToLower() == ".csv")
                        {
                            var csvDt=ReadCsvFile(path);
                            DataTable dt = FilterDatatableColoumn(csvDt, "phone");
                            foreach (DataRow dr in dt.Rows)
                            {
                                foreach(var r in dr.ItemArray)
                                {
                                    long number;
                                    bool isSuccess = Int64.TryParse(r.ToString(), out number);
                                    if (isSuccess)
                                    {
                                        UserScrubPhonesList.Add(number);
                                    }
                                }
                            }
                        }
                        else
                        {
                            FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);
                            IExcelDataReader excelReader=null;
                            if(ext==".xls")
                             excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                            else 
                             excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                            int fieldcount = excelReader.FieldCount;
                            int rowcount = excelReader.RowCount;
                            DataTable dt = new DataTable();
                            DataRow row;
                            DataTable dt_ = new DataTable();
                            dt_ = excelReader.AsDataSet().Tables[0];
                            for (int i = 0; i < dt_.Columns.Count; i++)
                            {
                                dt.Columns.Add(dt_.Rows[0][i].ToString());
                            }
                            int rowcounter = 0;
                            for (int row_ = 1; row_ < dt_.Rows.Count; row_++)
                            {
                                row = dt.NewRow();
                                for (int col = 0; col < dt_.Columns.Count; col++)
                                {
                                    row[col] = dt_.Rows[row_][col].ToString();
                                    rowcounter++;
                                }
                                dt.Rows.Add(row);
                            }
                            excelReader.Close();
                            excelReader.Dispose();
                            DataTable fDt = FilterDatatableColoumn(dt, "phone");
                            foreach (DataRow dr in fDt.Rows)
                            {
                                foreach (var r in dr.ItemArray)
                                {
                                    long number;
                                    bool isSuccess = Int64.TryParse(r.ToString(), out number);
                                    if (isSuccess)
                                    {
                                        UserScrubPhonesList.Add(number);
                                    }
                                }
                            }
                        }
                        
                        if (UserScrubPhonesList.Count > 0)
                        {
                           
                            var matchedList = leadService.GetAllLeadMasterDataByLeadTypes(selectedLeads).Select(x => x.Phone).Where(x => UserScrubPhonesList.Contains(x)).Distinct().ToList();
                            var unmatchedCount = UserScrubPhonesList.Except(matchedList).ToList();

                            sw.Stop();
                            //Matched File Create
                            string matchedFileName = Guid.NewGuid().ToString();
                            CreateSaveCsvFile(matchedFileName, matchedList);

                            // UnMatched File Create
                            string unMatchedFileName = Guid.NewGuid().ToString();
                            CreateSaveCsvFile(unMatchedFileName, unmatchedCount);

                            userScrubService.SaveUserScrub(UserScrubPhonesList.Count(),this.CurrentLoggedInUser.Id, SelectedLeads, matchedList.Count(), unmatchedCount.Count(), matchedFileName, unMatchedFileName, newFileName+ ext, sw.Elapsed.Seconds);
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
                    var matchedList = leadService.GetAllLeadMasterDataByLeadTypes(selectedLeads).Select(x => x.Phone).Where(x =>UserScrubPhonesList.Contains(x)).Distinct().ToList();
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

                    userScrubService.SaveUserScrub(UserScrubPhonesList.Count(), this.CurrentLoggedInUser.Id, SelectedLeads, matchedList.Count(), unmatchedCount.Count(), matchedFileName, unMatchedFileName, inputFileName+".csv", sw.Elapsed.Seconds);
                }

            }
            catch (Exception ex)
            {
                isError = true;
            }
            ViewBag.IsFileError = isError;
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            ViewBag.LeadTypesList = leadService.GetLeadTypes().ToList().Select(x => new DropDownModel()
            {
                Name = x.Name,
                Id = x.Id
            }).OrderBy(x => x.Name).ToList();
            return View("Scrubber");
        }

        private DataTable FilterDatatableColoumn(DataTable RecordDT_, string col)
        {
            try
            {
                DataTable TempTable = RecordDT_;
                DataView view = new DataView(TempTable);
                DataTable selected = view.ToTable("Selected", false, col);
                return selected;
            }
            catch(Exception ex)
            {
                throw ex;
            }
           
        }

        private DataTable ReadCsvFile(string path)
        {
            DataTable dtCsv = new DataTable();
            string Fulltext;
            using (StreamReader sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        Fulltext = sr.ReadToEnd().ToString(); //read full file text  
                        string[] rows = Fulltext.Split('\n'); //split full file text into rows  
                        for (int i = 0; i < rows.Count() - 1; i++)
                        {
                            string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values  
                            {
                                if (i == 0)
                                {
                                    for (int j = 0; j < rowValues.Count(); j++)
                                    {
                                        dtCsv.Columns.Add(rowValues[j]); //add headers  
                                    }
                                }
                                else
                                {
                                    DataRow dr = dtCsv.NewRow();
                                    for (int k = 0; k < rowValues.Count(); k++)
                                    {
                                        dr[k] = rowValues[k].ToString();
                                    }
                                    dtCsv.Rows.Add(dr); //add other rows  
                                }
                            }
                        }
                    }
                }
            return dtCsv;
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