using LeadDataManagement.Helpers;
using LeadDataManagement.Models.Context;
using LeadDataManagement.Repository.Interface;
using LeadDataManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Services
{
    public class CreditPackageService:ICreditPackageService
    {
        private ICreditPackageRepository creditPackageRepository;
        private readonly DateTime currentPstTime = DateTimeHelper.GetDateTimeNowByTimeZone(DateTimeHelper.TimeZoneList.PacificStandardTime);

        public CreditPackageService(ICreditPackageRepository _creditPackageRepository)
        {
            creditPackageRepository = _creditPackageRepository;
        }

        public IQueryable<CreditPackage> GetAllCreditPackages()
        {
            return creditPackageRepository.GetAll();
        }

        public void SavePackage(int id, string packageName, long credits, long price, bool status)
        {
            if(id==-1)
            {
                creditPackageRepository.Add(new CreditPackage {
                    PackageName=packageName,
                    Credits=credits,
                    Price=price,
                    IsActive=status,
                    CreatedAt= currentPstTime,
                });
            }
            else
            {
                var data = creditPackageRepository.GetAll().FirstOrDefault(x => x.Id == id);
                data.PackageName = packageName;
                data.Credits = credits;
                data.Price = price;
                data.IsActive = status;
                data.ModifiedAt = currentPstTime;
                creditPackageRepository.Update(data, data.Id);
            }
        }
    }
}