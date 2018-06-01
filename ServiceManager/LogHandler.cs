using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ServiceManager
{
    public  class LogHandler
    {
        readonly static ILog logger = LogManager.GetLogger("LogHandler");

        public static void Log(string message)
        {
            logger.Info(message);
        }

        public static void Error(string message)
        {
            logger.Error(message);
        }

    }
}
