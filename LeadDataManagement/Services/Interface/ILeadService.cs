using LeadDataManagement.Models.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Services.Interface
{
    public interface ILeadService
    {
        IQueryable<LeadType> GetLeadTypes();
        void AddEditLeadTypes(int id, string leadType);
        IQueryable<LeadMasterData> GetAllLeadMasterData();
        void SaveMasterData(List<int> PhoneNo, int leadTypeId);
    }
}