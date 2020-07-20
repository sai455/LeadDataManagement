using LeadDataManagement.Models.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Services.Interface
{
    public interface ICreditPackageService
    {
        IQueryable<CreditPackage> GetAllCreditPackages();
        void SavePackage(int id, string packageName, long credits, long price, bool status);
    }
}