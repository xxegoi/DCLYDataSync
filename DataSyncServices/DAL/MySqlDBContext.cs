using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;
using System.Data.Entity;

namespace DataSyncServices.DAL
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class MySqlDBContext :DbContext
    {
        public MySqlDBContext() : base("MySql") { }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Entity<Model.MySql.t_MySql_HK_PGData>().MapToStoredProcedures();
        //}

        public virtual DbSet<Model.MySql.t_MySql_HK_PGData> t_HK_PGData { get; set; }
    }
}
