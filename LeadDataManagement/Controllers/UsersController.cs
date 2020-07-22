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
        private IUserCreditLogsService userCreditLogsService;
        private ICreditPackageService creditPackageService;
        private string filterCols = "Phone,Ph,Home Phone,Telephone,phone,home phone,telephone";
        public UsersController(IUserScrubService _userScrubService, IUserService _userService, ILeadService _leadService, IUserCreditLogsService _userCreditLogsService, ICreditPackageService _creditPackageService)
        {
            userScrubService = _userScrubService;
            userService = _userService;
            leadService = _leadService;
            userCreditLogsService = _userCreditLogsService;
            creditPackageService = _creditPackageService;
        }
        public ActionResult Dashboard()
        {
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            ViewBag.PackagesList = creditPackageService.GetAllCreditPackages().Where(x => x.IsActive == true).Select(x => new DropDownModel
            {
                Id = x.Id,
                Name = x.PackageName,
                Count = x.Credits,
                Amount = x.Price
            }).ToList();

            return View();
        }
        public JsonResult GetUserCreditsDetails()
        {
            var usedCredits = userScrubService.GetScrubsByUserId(this.CurrentLoggedInUser.Id).Sum(x => x.ScrubCredits);
            var userTotalCredits = this.CurrentLoggedInUser.CreditScore;
            var percentage = userTotalCredits>0?Math.Round((usedCredits / (decimal)userTotalCredits) * 100, 2):0;
            var remainingCredits = userTotalCredits - usedCredits;
            var remainingPercentage = 100 - percentage;

            return Json(new
            {
                remainingCredits = remainingCredits,
                totalCredits = userTotalCredits,
                usedPercentage = percentage,
                remainingPercentage = remainingPercentage
            }, JsonRequestBehavior.AllowGet);
        }
        #region User Scrub

        public ActionResult Scrubber()
        {
            ViewBag.IsFileError = "false";
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            ViewBag.LeadTypesList = leadService.GetLeadTypes().Where(x=>x.IsActive==true).ToList().Select(x => new DropDownModel()
            {
                Name = x.Name,
                Id = x.Id
            }).OrderBy(x => x.Name).ToList();
            var usedCredits = userScrubService.GetScrubsByUserId(this.CurrentLoggedInUser.Id).Sum(x => x.ScrubCredits);
            var userTotalCredits = this.CurrentLoggedInUser.CreditScore;
            var percentage = Math.Round((usedCredits / (decimal)userTotalCredits) * 100, 2);
            ViewBag.remainingCredits = userTotalCredits - usedCredits;
            ViewBag.totalCredits = userTotalCredits;
            ViewBag.usedPercentage = percentage;
            ViewBag.remainingPercentage = 100 - percentage;
            ViewBag.NoCredits = false;
            return View();
        }

        public ActionResult UserScrubsGrid()
        {
            var Leads = leadService.GetLeadTypes().ToList();
            List<UserScrubsGridModel> retData = new List<UserScrubsGridModel>();
            var userScrubs = userScrubService.GetScrubsByUserId(this.CurrentLoggedInUser.Id);
            int iCount = 0;
            foreach (var u in userScrubs)
            {
                iCount += 1;
                List<int> leadTypes = JsonConvert.DeserializeObject<List<DropDownModel>>(u.LeadTypeIds).Select(x => x.Id).ToList();
                string InputExtensions = u.InputFilePath.Split('.')[1];
                retData.Add(new UserScrubsGridModel()
                {
                    Sno = iCount,
                    ScrubCredits = u.ScrubCredits,
                    CreatedAt = u.CreatedDate.ToString("dd-MMM-yyyy hh:mm:ss tt"),
                    LeadType = String.Join(",", Leads.Where(x => leadTypes.Contains(x.Id)).Select(x => x.Name).ToList()),
                    Matched = "Matched- " + u.MatchedCount + " <a href='" + u.MatchedPath + ".csv' style='cursor:pointer' download='Matched-" + u.Id + ".csv'><i class='fa fa-download' ></i></a><br>" + "Clean- " + u.UnMatchedCount + " <a href='" + u.UnMatchedPath + ".csv' style='cursor:pointer' download='UnMatched-" + u.Id + ".csv'><i class='fa fa-download' ></i></a>",
                    Duration = u.Duration,
                    InputFile = "Input File  <a href='" + u.InputFilePath + "' style='cursor:pointer' download='InputFile-" + u.Id + "." + InputExtensions + "'><i class='fa fa-download' ></i></a>",
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

        public ActionResult PerformUserScrub(FormCollection formCollection, string PhoneNos, string SelectedLeads)
        {
            bool isError = false;
            try
            {

                var used = userScrubService.GetScrubsByUserId(this.CurrentLoggedInUser.Id).Sum(x => x.ScrubCredits);
                Stopwatch sw = Stopwatch.StartNew();
                List<int> selectedLeads = JsonConvert.DeserializeObject<List<DropDownModel>>(SelectedLeads).Select(x => x.Id).ToList();
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
                        DataTable excelDt = new DataTable();
                        string phoneCol = string.Empty;
                        if (ext.ToLower() == ".csv")
                        {
                            var csvDt = ReadCsvFile(path, ref phoneCol);
                            List<DropDownModel> dt = FilterDatatableColoumn(csvDt.AsEnumerable(), csvDt, phoneCol);

                            foreach (var r in dt)
                            {
                                long number;
                                bool isSuccess = Int64.TryParse(r.Name, out number);
                                if (isSuccess)
                                {
                                    UserScrubPhonesList.Add(number);
                                }
                            }
                        }
                        else
                        {
                            FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);
                            IExcelDataReader excelReader = null;
                            if (ext == ".xls")
                                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                            else
                                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                            int fieldcount = excelReader.FieldCount;
                            int rowcount = excelReader.RowCount;

                            DataRow row;
                            DataTable dt_ = new DataTable();
                            dt_ = excelReader.AsDataSet().Tables[0];
                            for (int i = 0; i < dt_.Columns.Count; i++)
                            {
                                var thisCol = dt_.Rows[0][i].ToString();
                                excelDt.Columns.Add(thisCol);
                                if (filterCols.Split(',').ToList().Contains(thisCol))
                                {
                                    phoneCol = thisCol;
                                }
                            }
                            int rowcounter = 0;
                            for (int row_ = 1; row_ < dt_.Rows.Count; row_++)
                            {
                                row = excelDt.NewRow();
                                for (int col = 0; col < dt_.Columns.Count; col++)
                                {
                                    row[col] = dt_.Rows[row_][col].ToString();
                                    rowcounter++;
                                }
                                excelDt.Rows.Add(row);
                            }
                            excelReader.Close();
                            excelReader.Dispose();

                            List<DropDownModel> fDt = FilterDatatableColoumn(excelDt.AsEnumerable(), excelDt, phoneCol);
                            foreach (var r in fDt)
                            {
                                long number;
                                bool isSuccess = Int64.TryParse(r.Name, out number);
                                if (isSuccess)
                                {
                                    UserScrubPhonesList.Add(number);
                                }
                            }

                        }

                        if (UserScrubPhonesList.Count > 0)
                        {

                            if ((this.CurrentLoggedInUser.CreditScore - used) >= UserScrubPhonesList.Count())
                            {
                                var matchedList = leadService.GetAllLeadMasterDataByLeadTypes(selectedLeads).Select(x => x.Phone).Where(x => UserScrubPhonesList.Contains(x)).Distinct().ToList();
                                var unmatchedCount = UserScrubPhonesList.Except(matchedList).ToList();
                                var dtList = excelDt.AsEnumerable();
                                var matchedDt = GetFilteredPhones(dtList, matchedList, phoneCol);
                                var unMatchedDt = GetFilteredPhones(dtList, unmatchedCount, phoneCol);
                                sw.Stop();
                                //  Matched File Create
                                string matchedFileName = Guid.NewGuid().ToString();
                                CreatedDataTableCSV(matchedDt, matchedFileName);

                                // UnMatched File Create
                                string unMatchedFileName = Guid.NewGuid().ToString();
                                CreatedDataTableCSV(unMatchedDt, unMatchedFileName);

                                userScrubService.SaveUserScrub(UserScrubPhonesList.Count(), this.CurrentLoggedInUser.Id, SelectedLeads, matchedList.Count(), unmatchedCount.Count(), matchedFileName, unMatchedFileName, newFileName + ext, sw.Elapsed.Seconds);
                            }
                            else
                            {
                                ViewBag.NoCredits = true;
                                ViewBag.IsFileError = false;
                                ViewBag.CurrentUser = this.CurrentLoggedInUser;
                                ViewBag.LeadTypesList = leadService.GetLeadTypes().Where(x=>x.IsActive==true).ToList().Select(x => new DropDownModel()
                                {
                                    Name = x.Name,
                                    Id = x.Id
                                }).OrderBy(x => x.Name).ToList();
                                var usedCredits1 = userScrubService.GetScrubsByUserId(this.CurrentLoggedInUser.Id).Sum(x => x.ScrubCredits);
                                var userTotalCredits1 = this.CurrentLoggedInUser.CreditScore;
                                var percentage1 = Math.Round((usedCredits1 / (decimal)userTotalCredits1) * 100, 2);
                                ViewBag.remainingCredits = userTotalCredits1 - usedCredits1;
                                ViewBag.totalCredits = userTotalCredits1;
                                ViewBag.usedPercentage = percentage1;
                                ViewBag.remainingPercentage = 100 - percentage1;
                                return View("Scrubber");
                            }
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
                    if (UserScrubPhonesList.Count > 0)
                    {
                        if ((this.CurrentLoggedInUser.CreditScore - used) >= UserScrubPhonesList.Count())
                        {
                            var matchedList = leadService.GetAllLeadMasterDataByLeadTypes(selectedLeads).Select(x => x.Phone).Where(x => UserScrubPhonesList.Contains(x)).Distinct().ToList();
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

                            userScrubService.SaveUserScrub(UserScrubPhonesList.Count(), this.CurrentLoggedInUser.Id, SelectedLeads, matchedList.Count(), unmatchedCount.Count(), matchedFileName, unMatchedFileName, inputFileName + ".csv", sw.Elapsed.Seconds);
                        }
                        else
                        {
                            ViewBag.NoCredits = true;
                            ViewBag.IsFileError = false;
                            ViewBag.CurrentUser = this.CurrentLoggedInUser;
                            ViewBag.LeadTypesList = leadService.GetLeadTypes().Where(x=>x.IsActive==true).ToList().Select(x => new DropDownModel()
                            {
                                Name = x.Name,
                                Id = x.Id
                            }).OrderBy(x => x.Name).ToList();
                            var usedCredits1 = userScrubService.GetScrubsByUserId(this.CurrentLoggedInUser.Id).Sum(x => x.ScrubCredits);
                            var userTotalCredits1 = this.CurrentLoggedInUser.CreditScore;
                            var percentage1 = Math.Round((usedCredits1 / (decimal)userTotalCredits1) * 100, 2);
                            ViewBag.remainingCredits = userTotalCredits1 - usedCredits1;
                            ViewBag.totalCredits = userTotalCredits1;
                            ViewBag.usedPercentage = percentage1;
                            ViewBag.remainingPercentage = 100 - percentage1;
                            return View("Scrubber");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                isError = true;
            }
            ViewBag.NoCredits = false;
            ViewBag.IsFileError = isError;
            ViewBag.CurrentUser = this.CurrentLoggedInUser;
            ViewBag.LeadTypesList = leadService.GetLeadTypes().Where(x=>x.IsActive==true).ToList().Select(x => new DropDownModel()
            {
                Name = x.Name,
                Id = x.Id
            }).OrderBy(x => x.Name).ToList();
            var usedCredits = userScrubService.GetScrubsByUserId(this.CurrentLoggedInUser.Id).Sum(x => x.ScrubCredits);
            var userTotalCredits = this.CurrentLoggedInUser.CreditScore;
            var percentage = Math.Round((usedCredits / (decimal)userTotalCredits) * 100, 2);
            ViewBag.remainingCredits = userTotalCredits - usedCredits;
            ViewBag.totalCredits = userTotalCredits;
            ViewBag.usedPercentage = percentage;
            ViewBag.remainingPercentage = 100 - percentage;
            return View("Scrubber");
        }


        #region User Scrub Private Functions
        private DataTable GetFilteredPhones(EnumerableRowCollection<DataRow> uplodedFileDtRows, List<long> phonesNos, string phoneColName)
        {
            List<string> searchPhonesList = phonesNos.Select(x => x.ToString()).ToList();
            var newretDt = new DataTable();
            if (phonesNos.Count > 0)
                newretDt = uplodedFileDtRows.Where(x => searchPhonesList.Contains(x.Field<string>(phoneColName))).CopyToDataTable();
            return newretDt;
        }
        private List<DropDownModel> FilterDatatableColoumn(EnumerableRowCollection<DataRow> uplodedFileDtRows, DataTable RecordDT_, string col)
        {
            try
            {
                var dtt = uplodedFileDtRows.Select(x => new DropDownModel
                {
                    Name = x.Field<string>(col)
                }).ToList();
                //DataTable TempTable = RecordDT_;
                //DataView view = new DataView(TempTable);
                //DataTable selected = view.ToTable("Selected", false, col);
                return dtt;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private DataTable ReadCsvFile(string path, ref string phoneCol)
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
                                    var thisCol = rowValues[j];
                                    dtCsv.Columns.Add(thisCol); //add headers  
                                    if (filterCols.Split(',').ToList().Contains(thisCol) || filterCols.Split(',').ToList().Contains(thisCol + "\r"))
                                    {
                                        phoneCol = thisCol;
                                    }
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
        public void CreatedDataTableCSV(DataTable dtDataTable, string newFileName)
        {
            var path = Path.Combine(Server.MapPath("~/Content/DataLoads/"), newFileName + ".csv");
            StreamWriter sw = new StreamWriter(path, false);
            //headers  
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
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

        #endregion

        #endregion

        #region User Credits
        public ActionResult UserCreditLogGrid()
        {
            List<UserCreditLogGridViewModel> retData = new List<UserCreditLogGridViewModel>();
            var userCreditLogs = userCreditLogsService.GetAllUserCreditLogs().Where(x=>x.UserId==CurrentLoggedInUser.Id).ToList();
            int iCount = 0;
            foreach (var u in userCreditLogs)
            {
                iCount += 1;
                retData.Add(new UserCreditLogGridViewModel()
                {
                    SNo = iCount,
                    Id = u.Id,
                    Date = u.CreatedAt.ToString("dd-MMM-yyyy hh:mm:ss tt"),
                    CreatedAt = u.CreatedAt,
                    Credits = u.Credits,
                    DisCountPercentage = u.DiscountPercentage.ToString(),
                    AmountPaid = u.FinalAmount.ToString(),
                    PackageName = creditPackageService.GetAllCreditPackages().FirstOrDefault(x => x.Id == u.PackageId).PackageName
                });
            }
            var usersRefered = userService.GetUsers().Where(x => x.ReferedUserId.HasValue && x.ReferedUserId.Value == CurrentLoggedInUser.Id).ToList();
            var referalCreditsList = userCreditLogsService.GetAllUserCreditLogs().ToList().Where(x => usersRefered.Select(y=>y.Id).Contains(x.UserId));
            foreach (var u in referalCreditsList)
            {
                iCount += 1;
                var thisUser = usersRefered.Where(x => x.Id == u.UserId).FirstOrDefault();
                retData.Add(new UserCreditLogGridViewModel()
                {
                    SNo = iCount,
                    Id = u.Id,
                    Date = u.CreatedAt.ToString("dd-MMM-yyyy hh:mm:ss tt"),
                    CreatedAt = u.CreatedAt,
                    Credits = u.ReferalUserCredits,
                    DisCountPercentage = "-",
                    AmountPaid = "-",
                    PackageName = "Referral Bonus From "+ thisUser.Name
                });
            }

            retData = retData.OrderByDescending(x => x.CreatedAt).ToList();
            var jsonData = new { data = from emp in retData select emp };
            return new JsonResult()
            {
                Data = jsonData,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue
            };
        }

        [HttpPost]
        public ActionResult AddUserCredits(int packageId, int qty, long credits, long amount, int discountPercentage, float finalAmount, long referalCredits)
        {
            long rCredits = 0;
            if (CurrentLoggedInUser.ReferedUserId.HasValue && CurrentLoggedInUser.ReferedUserId.Value > 0)
            {
                rCredits = referalCredits;
                var userData = userService.GetUsers().Where(x => x.Id == CurrentLoggedInUser.ReferedUserId.Value).FirstOrDefault();
                userData.CreditScore += referalCredits;
                userService.UpdateUserDetails(userData);
            }
            userCreditLogsService.BuyCredits(CurrentLoggedInUser.Id, packageId, qty, credits, amount, discountPercentage, finalAmount, rCredits);
            CurrentLoggedInUser.CreditScore += credits;
            userService.UpdateUserDetails(CurrentLoggedInUser);
            return Json("");
        }
        #endregion
    }

}