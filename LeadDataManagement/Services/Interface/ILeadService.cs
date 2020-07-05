using LeadDataManagement.Models.Context;
using LeadDataManagement.Models.ViewModels;
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
        IQueryable<LeadMasterData> GetAllLeadMasterDataByLeadType(int leadTypeId);
        void SaveMasterData(List<long> PhoneNo, int leadTypeId);
        List<DropDownModel> GetLeadMasterdataGridList(int? leadTypeId);
    }
}