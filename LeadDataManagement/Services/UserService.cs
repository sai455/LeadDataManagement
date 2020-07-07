using LeadDataManagement.Helpers;
using LeadDataManagement.Models.Context;
using LeadDataManagement.Models.ViewModels;
using LeadDataManagement.Repository.Interface;
using LeadDataManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Services
{
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        private readonly DateTime currentPstTime = DateTimeHelper.GetDateTimeNowByTimeZone(DateTimeHelper.TimeZoneList.PacificStandardTime);

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public User ValidateUserByEmailId(string email, string password)
        {
            return _userRepository.FindBy(x => x.Email == email && x.Password == password).FirstOrDefault();
        }

        public IQueryable<User> GetUsers()
        {
            return _userRepository.GetAll();
        }

        public void SaveUser(UserViewModel user)
        {
            _userRepository.Add(new Models.Context.User
            {
                Name = user.Name,
                Email = user.Email,
                Password = user.Password,
                IsAdmin = false,
                Phone = user.Phone,
                CreditScore = 100000,
                StatusId = 1,
                CreatedAt = currentPstTime,
            });
        }

        public string GetStatusById(int statusId)
        {
            string retVal = string.Empty;
            switch(statusId)
            {
                case 1: retVal = "Pending";break;
                case 2: retVal = "Active"; break;
                case 3: retVal = "Inactive"; break;
            }
            return retVal;
        }
        public void UpdateUserPassword(string email, string password)
        {
            var userData = _userRepository.FindBy(x => x.Email == email).FirstOrDefault();
            if(userData!=null)
            {
                userData.Password = password;
            }
            _userRepository.Update(userData, userData.Id);
        }

        public void UpdateUserStatus(int userId, long CreditScore, int statusId)
        {
            var userData = _userRepository.FindBy(x => x.Id == userId).FirstOrDefault();
            if(userData!=null)
            {
                userData.CreditScore = CreditScore;
                userData.StatusId = statusId;
                userData.ModifiedAt = currentPstTime;
                _userRepository.Update(userData, userData.Id);
            }
        }
    }
}