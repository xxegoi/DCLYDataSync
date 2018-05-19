using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Timers;
using log4net;
using System.Reflection;
using log4net.Config;
using DataSyncServices.DAL;
using MySql.Data.MySqlClient;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace DataSyncServices
{
    partial class DataSyncServices : ServiceBase
    {
        Timer timer = GetTimer();

        ILog logger = log4net.LogManager.GetLogger("DataLog");

        DYJCX_SQLServerContainer sqlserver_db = new DYJCX_SQLServerContainer();
        MySqlDBContext mysql_db = new MySqlDBContext();

        MySqlConnection conn = MySqlDBConnection.GetConnection();

        public DataSyncServices()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: 在此处添加代码以启动服务。
            //logger.Info("数据同步服务开启");
           


            double interval = Convert.ToDouble(ConfigurationManager.AppSettings["Interval"]);
            //如果同步间隔修改了，则应用新的同步间隔
            if (timer.Interval != interval)
            {
                timer.Stop();
                timer = GetTimer();
            }
            //间隔到达后执行同步

            timer.Elapsed += DataSync;
            timer.Start();
            
        }



        //数据同步
        private void DataSync(object sender, ElapsedEventArgs e)
        {

            var syncList = Check();
            Task.Run(() => logger.Info(string.Format("发现未同步数据{0}条",syncList.Count)));
            if (syncList != null)
            {
                var count = SyncToMysql(syncList);
                string message = string.Format("同步完成，本次同步了{0}条数据，同步失败{1}条", count, syncList.Count - count);

                Task.Run(() => logger.Info(message));
            }
        }

        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            //Task.Run(() => logger.Info("数据同步服务停止"));
        }

        private void test()
        {
            Random random = new Random();
            var r = random.Next(1, 4);

            switch (r)
            {
                case 1:
                    {
                        logger.Info("ok");
                        break;
                    }
                case 2:
                    {
                        logger.Warn("warnning");
                        break;
                    }
                case 3:
                    {
                        logger.Debug("debug");
                        break;
                    }
                case 4:
                    {
                        logger.Error("error");
                        break;
                    }
            }
        }

        //检查是否有未同步的数据
        private List<DAL.t_HK_PGData> Check()
        {
            try
            {
                return sqlserver_db.t_HK_PGData.Where(p => p.IsSync == false).ToList();
            }
            catch (Exception ex)
            {
                Task.Run(() => logger.Error(ex.TargetSite + "\t" + ex.Message));
                return null;
            }
        }

        private int SyncToMysql(List<DAL.t_HK_PGData> list)
        {
            int count = 0;
            //遍历数据并插入到MYSQL中
            list.ForEach(p =>
            {
                try
                {
                    Task.Run(() => logger.Info("同步数据：" + p.batch_no));
                    var item = new Model.MySql.t_MySql_HK_PGData(p);

                    conn.Open();
                    using (conn)
                    {
                        MySqlCommand cmd = new MySqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = item.InsertSql;
                        cmd.Parameters.AddRange(item.Parameters.ToArray());

                        Task.Run(() => logger.Info("执行SQL： " + cmd.CommandText));
                        count+= cmd.ExecuteNonQuery();

                        p.IsSync = true;
                        sqlserver_db.t_HK_PGData.Attach(p);
                        sqlserver_db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                        
                    }
                    
                    //mysql_db.t_HK_PGData.Add(item);
                    //count += mysql_db.Database.ExecuteSqlCommand(item.InsertSql, item.Parameters.ToArray());
                }
                catch(Exception ex)
                {
                    Task.Run(() => logger.Error(p.batch_no + "插入失败,原因："+ex.Message));
                }
            });
            sqlserver_db.SaveChanges();
            return count;
            //return mysql_db.SaveChanges();


        }

        private static System.Timers.Timer GetTimer()
        {
            //默认5分钟
            double interval = 300000;
            try
            {
                ConfigurationManager.RefreshSection("appSettings");
                //读取配置文件中的同步间隔时间
                interval = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["Interval"]);
                if (interval == 0)
                {
                    interval = 300000;
                }
                else
                {
                    //分钟转换为毫秒
                    interval = interval * 1000 * 60;
                }
            }
            catch
            {
                interval = 300000;
            }

            return new System.Timers.Timer(interval);
        }
    }
}
