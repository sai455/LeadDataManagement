using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Models.ViewModels
{
    public class LeadTypeGridViewModel
    {
        public int Id { get; set; }
        public int SNo { get; set; }
        public string LeadType { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
        public bool IsActive { get; set; }
       public string EditBtn { get; set; }
    }
    public class LeadMasterDataGridViewModel
    {
        public long Id { get; set; }
        public int SNo { get; set; }
        public string LeadType { get; set; }
        public long Phone { get; set; }
        public int LeadTypeId { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string EditBtn { get; set; }
    }
    public class UserScrubsGridModel
    {
        public string CreatedAt { get; set; }
        public int Sno { get; set; }
        public int LeadTypeId { get; set; }
        public string LeadType { get; set; }
        public string Matched { get; set; }
        public string UnMatched { get; set; }
        public int Duration { get; set; }
        public string InputFile { get; set; }
    }
    public class DropDownModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}