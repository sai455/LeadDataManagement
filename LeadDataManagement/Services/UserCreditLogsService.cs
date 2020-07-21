using LeadDataManagement.Helpers;
using LeadDataManagement.Models.Context;
using LeadDataManagement.Repository.Interface;
using LeadDataManagement.Services.Interface;
using System;
using System.Linq;

namespace LeadDataManagement.Services
{
    public class UserCreditLogsService : IUserCreditLogsService
    {
        private IUserCreditLogsRepository userCreditLogsRepository;
        private readonly DateTime currentPstTime = DateTimeHelper.GetDateTimeNowByTimeZone(DateTimeHelper.TimeZoneList.PacificStandardTime);

        public UserCreditLogsService(IUserCreditLogsRepository _userCreditLogsRepository)
        {
            userCreditLogsRepository = _userCreditLogsRepository;
        }
        public IQueryable<UserCreditLogs> GetAllUserCreditLogs()
        {
            return userCreditLogsRepository.GetAll();
        }
        public void BuyCredits(int userId,int packageId, int qty, long credits, long amount, int discountPercentage, float finalAmount,long referalCredits)
        {
            userCreditLogsRepository.AddAsyn(new UserCreditLogs
            {
                PurchaseId = Guid.NewGuid().ToString(),
                PackageId=packageId,
                Credits=credits,
                TransactionDetails=string.Empty,
                UserId= userId,
                Amount=amount,
                DiscountPercentage=discountPercentage,
                FinalAmount=finalAmount,
                CreatedAt= currentPstTime,
                ModifiedAt=currentPstTime,
                ReferalUserCredits= referalCredits
            });
        }
    }
}