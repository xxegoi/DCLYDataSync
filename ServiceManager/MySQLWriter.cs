using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;


namespace ServiceManager
{
    public class MySQLWriter
    {
        string conStr = ConfigurationManager.ConnectionStrings["MySql"].ConnectionString;
        public MySQLWriter()
        {

        }

        public int Insert(t_HK_PGData data)
        {
            using (MySqlConnection conn = new MySqlConnection(conStr))
            {
                string message = "插入缸号："+data.batch_no+" 记录\r\n";
                int result = 0;
                try
                {
                    MySqlCommand cmd = GetInsertCommand(data);
                    message += "执行SQL语句：" + cmd.CommandText+"\r\n";
                    conn.Open();
                    cmd.Connection = conn;
                    result= cmd.ExecuteNonQuery();
                    message += message + "结果：成功";
                    LogHandler.Log(message);
                }
                catch (Exception ex)
                {
                    message += message + "结果：失败\r\n" + "原因：" + ex.InnerException;
                    LogHandler.Error(message);
                }
                
                return result;
            }
        }

        public int Delete(string batchNo)
        {
            using (MySqlConnection conn=new MySqlConnection())
            {
                string message = "删除缸号：" + batchNo + " 记录\r\n";
                int result = 0;
                try
                {
                    MySqlCommand cmd = GetDeleteCommand(batchNo);
                    message += "执行SQL语句：" + cmd.CommandText + "\r\n";
                    conn.Open();
                    cmd.Connection = conn;
                    result = cmd.ExecuteNonQuery();
                    message += message + "结果：成功";
                    LogHandler.Log(message);
                }
                catch (Exception ex)
                {
                    message += message + "结果：失败\r\n" + "原因：" + ex.InnerException;
                    LogHandler.Error(message);
                }

                return result;

            }
        }

        private MySqlCommand GetInsertCommand(t_HK_PGData data)
        {
            MySqlCommand cmd = new MySqlCommand();
            string InsertSql;
            //反射数据源类
            Type t = data.GetType();

            //取得数据源所有属性
            var pis = t.GetProperties().Where(p => p.Name != "IsSync").ToList();

            var sql = @"INSERT INTO t_HK_PGData (";
            var valueSql = @" VALUE (";
            //遍历数据源所有属性
            pis.ForEach(p =>
            {
                var value = p.GetValue(data);
                cmd.Parameters.Add(new MySqlParameter("@" + p.Name, value));

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
            });

            InsertSql = sql + valueSql;
            cmd.CommandText = InsertSql;
            return cmd;
        }

        private MySqlCommand GetDeleteCommand(string batchNo)
        {
            string sql = @"DELETE t_HK_PGData WHERE batch_no=@batch_no";
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new MySqlParameter("@batch_no", batchNo));
            return cmd;
        }
    }
}
