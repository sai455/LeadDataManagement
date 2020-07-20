using LeadDataManagement.Models.Context;
using LeadDataManagement.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Services.Interface
{
    public interface IUserService
    {
        IQueryable<User> GetUsers();
        User ValidateUserByEmailId(string email, string password);
        void SaveUser(UserViewModel u);
        string GetStatusById(int statusId);
        void UpdateUserPassword(string email, string password);
        void UpdateUserStatus(int userId,long CreditScore,int statusId, int discountPercentage);
    }
}