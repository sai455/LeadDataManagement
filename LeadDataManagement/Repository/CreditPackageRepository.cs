using LeadDataManagement.Models.Context;
using LeadDataManagement.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Repository
{
    public class CreditPackageRepository : GenericRepository<CreditPackage>, ICreditPackageRepository
    {
        public CreditPackageRepository(LeadDbContext leadDbContext) : base(leadDbContext)
        {
            
        }
    }
}