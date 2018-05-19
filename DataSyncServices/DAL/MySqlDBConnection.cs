using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;
using log4net;

namespace DataSyncServices.DAL
{
    public class MySqlDBConnection
    {
        public static MySqlConnection GetConnection()
        {
            ILog logger = log4net.LogManager.GetLogger("DataLog");
            var conString = ConfigurationManager.ConnectionStrings["MySql"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(conString);
            try
            {
                conn.Open();
                conn.Close();
                Task.Run(() => logger.Info( "MySql连接成功"));
            }catch(Exception ex)
            {
                Task.Run(() => logger.Error("MySql连接失败，错误信息：" + ex.Message));

            }
            return conn;
        }
    }
}
