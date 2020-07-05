﻿using LeadDataManagement.Models.Context;
using LeadDataManagement.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Repository.Interface
{
    public interface ILeadMasterDataRepository : IGenericRepository<LeadMasterData>
    {
        void USPLoadMasterData(List<long> phonesList, int leadTypeId);
        List<DropDownModel> UspGetLeadMasterDataGrid(int? leadTypeId);
    }
}