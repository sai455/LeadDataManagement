using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Models.Context
{
    public partial class LeadDbContext : DbContext
    {
        public LeadDbContext() : base("LeadDBContext")
        {

        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<LeadType> LeadTypes { get; set; }
        public virtual DbSet<LeadMasterData> LeadMasterDatas { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<LeadType>().ToTable("LeadType");
            modelBuilder.Entity<LeadMasterData>().ToTable("LeadMasterData");
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}