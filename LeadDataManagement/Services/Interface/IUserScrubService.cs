﻿using LeadDataManagement.Models.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Services.Interface
{
    public interface IUserScrubService
    {
        IList<UserScrub> GetScrubsByUserId(int userId);
        void SaveUserScrub(int userId, int leadTypeId, long matchedCount, long unmatchedCount, string matchedPath, string unMatchedPath, string fileName, int duration);
    }
}