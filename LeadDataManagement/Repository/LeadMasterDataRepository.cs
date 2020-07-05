using LeadDataManagement.Models.Context;
using LeadDataManagement.Models.ViewModels;
using LeadDataManagement.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.UI.WebControls;

namespace LeadDataManagement.Repository
{
    public class LeadMasterDataRepository : GenericRepository<LeadMasterData>, ILeadMasterDataRepository
    {
        public LeadMasterDataRepository(LeadDbContext leadDbContext) : base(leadDbContext)
        {
            leadDbContext.Database.CommandTimeout = 180;
        }

        public void USPLoadMasterData(List<long> phonesList, int leadTypeId)
        {
            var dt = PrepareUDT(phonesList);
            var parameter1 = new SqlParameter("@LeadTypeId", SqlDbType.Int);
            parameter1.Value = leadTypeId;
            var parameter2 = new SqlParameter("@PhoneList", SqlDbType.Structured);
            parameter2.Value = dt;
            parameter2.TypeName = "MasterDataLoadType";

          
            var data =ExecuteSqlCommand("EXEC usp_LoadMasterData @LeadTypeId, @PhoneList",parameter1, parameter2);
        }

        public List<DropDownModel> UspGetLeadMasterDataGrid(int? leadTypeId)
        {
            var retData = SQLQuery<DropDownModel>(string.Format("Exec usp_GetLeadMasterDataGrid {0}",leadTypeId)).ToList();
            return retData;
        }
        private DataTable PrepareUDT(List<long> Phones)
        {
            var retdt = new DataTable();
            retdt.Columns.Add("Phone", typeof(long));
            foreach(var  p in Phones)
            {
                retdt.Rows.Add(p);
            }
            return retdt;
        }
    }
}