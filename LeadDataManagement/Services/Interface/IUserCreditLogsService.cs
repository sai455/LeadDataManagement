using LeadDataManagement.Models.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Services.Interface
{
    public interface IUserCreditLogsService
    {
        IQueryable<UserCreditLogs> GetAllUserCreditLogs();
        void BuyCredits(int userId,int packageId, int qty, long credits, long amount, int discountPercentage, float finalAmount, long referalCredits,string transactionDetails);
    }
}