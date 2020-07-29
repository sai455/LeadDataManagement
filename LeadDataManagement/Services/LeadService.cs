using LeadDataManagement.Helpers;
using LeadDataManagement.Models.Context;
using LeadDataManagement.Models.ViewModels;
using LeadDataManagement.Repository.Interface;
using LeadDataManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeadDataManagement.Services
{
    public class LeadService:ILeadService
    {
        private ILeadRepository _leadRepository;
        private ILeadMasterDataRepository _leadMasterDataRepository;
        private readonly DateTime currentPstTime = DateTimeHelper.GetDateTimeNowByTimeZone(DateTimeHelper.TimeZoneList.PacificStandardTime);

        public LeadService(ILeadRepository leadRepository, ILeadMasterDataRepository leadMasterDataRepository)
        {
            _leadRepository = leadRepository;
            _leadMasterDataRepository = leadMasterDataRepository;
        }
        public IQueryable<LeadType> GetLeadTypes()
        {
            return _leadRepository.GetAll();
        }

        public void AddEditLeadTypes(int id, string leadType)
        {
            var leadTypeData = _leadRepository.FindBy(x => x.Id == id).FirstOrDefault();
            if(leadTypeData==null)
            {
                _leadRepository.Add(new LeadType()
                {
                    Name = leadType,
                    IsActive = true,
                    CreatedAt = currentPstTime,
                });
            }
            else
            {
                leadTypeData.Name = leadType;
                leadTypeData.ModifiedAt = currentPstTime;
                _leadRepository.Update(leadTypeData, leadTypeData.Id);
            }
        }

        public IQueryable<LeadMasterData> GetAllLeadMasterDataByLeadType(int leadTypeId)
        {
            return _leadMasterDataRepository.GetAllLeadMasterDataByLeadType(leadTypeId);
        }
        public IQueryable<LeadMasterData> GetAllLeadMasterDataByLeadTypes(List<int> leadTypes)
        {
            return _leadMasterDataRepository.GetAllLeadMasterDataByLeadTypes(leadTypes);
        }

        public List<long> ScrubPhoneNos(List<int> leadTypes,List<long> inputPhoneList)
        {
            return _leadMasterDataRepository.ScrubPhoneNos(leadTypes, inputPhoneList);
        }
        public IQueryable<LeadMasterData>GetAllLeadMasterData()
        {
            return _leadMasterDataRepository.GetAll();
        }

        public void SaveMasterData(List<long> PhoneNo, int leadTypeId)
        {
            _leadMasterDataRepository.USPLoadMasterData(PhoneNo, leadTypeId);
        }

        public List<DropDownModel> GetLeadMasterdataGridList(int? leadTypeId)
        {
            return _leadMasterDataRepository.UspGetLeadMasterDataGrid(leadTypeId);
        }

        public void UpdateLeadTypeStatus(int id)
        {
            var data = _leadRepository.FindBy(x => x.Id == id).FirstOrDefault();
            if(data!=null)
            {
                data.IsActive = !data.IsActive;
                _leadRepository.Update(data, data.Id);
            }
        }
    }
}