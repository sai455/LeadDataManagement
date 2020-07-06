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
                    CreatedAt = DateTime.Now,
                });
            }
            else
            {
                leadTypeData.Name = leadType;
                leadTypeData.ModifiedAt = DateTime.Now;
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
    }
}