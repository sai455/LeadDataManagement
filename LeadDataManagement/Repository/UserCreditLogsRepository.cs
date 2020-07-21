using LeadDataManagement.Models.Context;
using LeadDataManagement.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Repository
{
    public class UserCreditLogsRepository : GenericRepository<UserCreditLogs>, IUserCreditLogsRepository
    {
        public UserCreditLogsRepository(LeadDbContext leadDbContext) : base(leadDbContext)
        {
            leadDbContext.Database.CommandTimeout = 180;
        }
    }
}