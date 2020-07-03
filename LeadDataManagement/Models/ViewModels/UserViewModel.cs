using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Models.ViewModels
{
    public class UserViewModel
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm Password and Password do not match")]
        public string ConfirmPassword { get; set; }
    }

    public class UserGridViewModel
    {
        public int SNo { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public long CreditScore { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedAt { get; set; }
        public string EditBtn { get; set; }
        public int StatusId { get; set; }
    }
}