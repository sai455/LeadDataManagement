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
}
