using LeadDataManagement.Models.Context;
using LeadDataManagement.Repository.Interface;
using LeadDataManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Services
{
    public class UserScrubService:IUserScrubService
    {
        private IUserScrubRepository _userScrubRepository;
        public UserScrubService(IUserScrubRepository userScrubRepository)
        {
            _userScrubRepository = userScrubRepository;
        }

        public IList<UserScrub> GetScrubsByUserId(int userId)
        {
            return _userScrubRepository.FindAll(x => x.UserId == userId).OrderByDescending(x=>x.Id).ToList();
        }
        public void SaveUserScrub(long scrubInputRecords,int userId, string leadTypeIds, long matchedCount, long unmatchedCount, string matchedPath, string unMatchedPath, string fileName, int duration)
        {
            _userScrubRepository.Add(new UserScrub()
            {
                UserId = userId,
                LeadTypeIds = leadTypeIds,
                CreatedDate = DateTime.Now,
                Duration = duration,
                MatchedCount = matchedCount,
                UnMatchedCount = unmatchedCount,
                InputFilePath = "/Content/DataLoads/"+fileName,
                MatchedPath = "/Content/DataLoads/" + matchedPath,
                UnMatchedPath = "/Content/DataLoads/" + unMatchedPath,
                ScrubCredits= scrubInputRecords
            });
        }
    }
}