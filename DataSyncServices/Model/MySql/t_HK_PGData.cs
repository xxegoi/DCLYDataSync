using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace DataSyncServices.Model.MySql
{
    public class t_MySql_HK_PGData
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

        public string InsertSql { get; set; }
        public List<MySqlParameter> Parameters { get; set; }

        //构造函数
        public t_MySql_HK_PGData(DAL.t_HK_PGData data)
        {
            //反射数据源类
            Type t = data.GetType();
            //反射自身数据类
            Type t2 = this.GetType();
            //取得数据源所有属性
            var pis = t.GetProperties().Where(p => p.Name != "IsSync").ToList();

            this.Parameters = new List<MySqlParameter>();

            //            var sql = @"Insert into t_HK_PGData (batch_no,colour_name,lab_dip_ref,division,customer,
            //fabric_desc,yarn_desc,weight,width,finishing,qty,no_of_roll,production_desc,IsSync) value (@batch_no,@colour_name,@lab_dip_ref,)";

            var sql = @"INSERT INTO t_HK_PGData (";
            var valueSql = @" VALUE (";
            //遍历数据源所有属性
            pis.ForEach(p =>
            {
                //由于数据源类与此类所有属性相同（属性命名与数据类型），所以可以用源数据的属性名取得本类的属并进行赋值
                var t2_pi = t2.GetProperty(p.Name);

                var value = p.GetValue(data);
                Parameters.Add(new MySqlParameter("@" + p.Name, value));

                if (value != null)
                    t2_pi.SetValue(this, value);


                if (pis.Last() != p)
                {
                    sql += p.Name + ",";
                    valueSql += "@" + p.Name + ",";
                }
                else
                {
                    sql += p.Name + ")";
                    valueSql += "@" + p.Name + ")";
                }




                this.InsertSql = sql + valueSql;
            });
        }
    }


}
