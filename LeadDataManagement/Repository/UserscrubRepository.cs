using LeadDataManagement.Models.Context;
using LeadDataManagement.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Repository
{
    public class UserScrubRepository : GenericRepository<UserScrub>, IUserScrubRepository
    {
        public UserScrubRepository(LeadDbContext leadDbContext) : base(leadDbContext)
        {
        }
    }
}