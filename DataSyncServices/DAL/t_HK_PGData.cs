//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataSyncServices.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_HK_PGData
    {
        public string batch_no { get; set; }
        public string colour_name { get; set; }
        public string lab_dip_ref { get; set; }
        public string division { get; set; }
        public string customer { get; set; }
        public string fabric_desc { get; set; }
        public string yarn_desc { get; set; }
        public string weight { get; set; }
        public string width { get; set; }
        public string finishing { get; set; }
        public Nullable<decimal> qty { get; set; }
        public Nullable<int> no_of_roll { get; set; }
        public string production_desc { get; set; }
        public bool IsSync { get; set; }
    }
}
