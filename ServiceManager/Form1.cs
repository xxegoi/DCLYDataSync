using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Collections;
using System.Configuration.Install;
using log4net;
using System.Threading;
using System.Configuration;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ServiceManager
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool run = true;
        Thread t1=null;
        private ContextMenu notifyiconMenu;

        private void InitializenotifyiconMenu()
        {
            List<MenuItem> items = new List<MenuItem>();
            MenuItem item = new MenuItem("退出");
            item.Click += new EventHandler(this.Exit);
            items.Add(item);
            this.notifyiconMenu = new ContextMenu(items.ToArray());
            this.notifyIcon1.ContextMenu = notifyiconMenu;
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            LogHandler.Log("开始同步数据");
            this.btn_Start.Enabled = false;
            t1 = new Thread(ServiceStart);
            this.t1.Start();
            this.btn_Stop.Enabled = true;
            this.btn_Start.Enabled = false;
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            if(this.t1!=null&&this.t1.IsAlive) this.t1.Abort();
            this.btn_Start.Enabled = true;
            this.btn_Stop.Enabled = false;
            LogHandler.Log("停止同步数据");
        }



        //判断服务是否存在
        private bool IsServiceExisted(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController sc in services)
            {
                if (sc.ServiceName.ToLower() == serviceName.ToLower())
                {
                    return true;
                }
            }
            return false;
        }


        //启动服务
        private void ServiceStart()
        {
            while (run)
            {
                var dataList = Check();
                var list = DataSync(dataList);
                if (list.Count > 0) { WriteBack(list); }

                var intervalStr = ConfigurationManager.AppSettings["Interval"];
                ConfigurationManager.RefreshSection("appSettings");
                if (intervalStr != null)
                {
                    int interval = int.Parse(intervalStr);
                    interval = interval * 60 * 1000;
                    Thread.CurrentThread.Join(interval);
                }
                else
                {
                    throw new Exception("同步间隔未设置");
                }
                
            }
        }


        private List<t_HK_PGData> Check()
        {
            LogHandler.Log("检查需要同步的数据");
            try
            {
                DYJXCEntities db = new DYJXCEntities();
                var result = db.t_HK_PGData.Where(p => !p.IsSync).ToList();
                LogHandler.Log(string.Format("发现待同步数据{0}条", result.Count));
                return result;
            }
            catch (Exception ex)
            {
                LogHandler.Error("发生错误：" + ex.InnerException);
            }
            return null;
        }

        private List<string> DataSync(List<t_HK_PGData> list)
        {
            List<string> result = new List<string>();
            MySQLWriter db = new MySQLWriter();

            int count = 0;

            list.ForEach(p =>
            {
                count += db.Insert(p);
                result.Add(p.batch_no);
            });

            LogHandler.Log(string.Format("本次共同步{0}条数据", count));
            return result;
        }

        private void WriteBack(List<string> list)
        {
            try
            {
                LogHandler.Log("开始回写SQLSERVER");
                DYJXCEntities db = new DYJXCEntities();
                var data = (from d in db.t_HK_PGData
                            where list.Contains(d.batch_no)
                            select d).ToList();

                data.ForEach(p =>
                {
                    db.t_HK_PGData.Attach(p);
                    p.IsSync = true;
                    db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                });

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogHandler.Error("回写发生错误：\r\n" + ex.InnerException);
                list.ForEach(p =>
                {
                    MySQLWriter db = new MySQLWriter();
                    db.Delete(p);
                });
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Hide();
            if (this.t1 != null && this.t1.IsAlive)
            {
                this.t1.Abort();
            }
        }

        private void Exit(object sender,EventArgs e)
        {
            if (this.t1 != null && this.t1.IsAlive)
            {
                this.t1.Abort();
            }

            this.Close();
            this.Dispose();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.notifyIcon1.Visible = true;
                this.ShowInTaskbar = false;
                this.InitializenotifyiconMenu();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.notifyIcon1.Visible = false;
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        
    }
}
