using LeadDataManagement.Models.Context;
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


        public IQueryable<LeadMasterData>GetAllLeadMasterData()
        {
            return _leadMasterDataRepository.GetAll();
        }

        public void SaveMasterData(List<int> PhoneNo, int leadTypeId)
        {
            foreach(var p in PhoneNo)
            {
                _leadMasterDataRepository.Add(new LeadMasterData()
                {
                    Phone = p,
                    IsActive=true,
                    CreatedAt=DateTime.Now,
                    LeadTypeId= leadTypeId
                });
            }
        }
    }
}