using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Models.ViewModels
{
    public class CreditPackageViewModel
    {
        public int Id { get; set; }
        public int SNo { get; set; }
        public string PackageName { get; set; }
        public long Credits { get; set; }
        public long Price { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; }
        public string EditBtn { get; set; }
    }

    public class UserCreditLogGridViewModel
    {
        public int SNo { get; set; }
        public long Id { get; set; }
        public string PackageName { get; set; }
        public long Credits { get; set; }
        public string DisCountPercentage { get; set; }
        public string AmountPaid { get; set; }
        public string Date { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
