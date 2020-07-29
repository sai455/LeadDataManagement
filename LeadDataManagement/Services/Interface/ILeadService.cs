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
        IQueryable<LeadMasterData> GetAllLeadMasterDataByLeadTypes(List<int> leadTypes);
        void SaveMasterData(List<long> PhoneNo, int leadTypeId);
        void UpdateLeadTypeStatus(int id);
        List<DropDownModel> GetLeadMasterdataGridList(int? leadTypeId);
        List<long> ScrubPhoneNos(List<int> leadTypes, List<long> inputPhoneList);
    }
}