using LeadDataManagement.Models.Context;
using LeadDataManagement.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Repository
{
        public class UserRepository : GenericRepository<User>, IUserRepository
        {
            public UserRepository(LeadDbContext leadDbContext) : base(leadDbContext)
            {
            }
        }
}