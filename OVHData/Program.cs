using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OVHData
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings.init();
            Logger.InitFileLogger(Settings.single.LogPath, "MonitorNPRCH");
            Logger.Info("Run");
            DateTime now = DateTime.Now.AddDays(5);
            Logger.Info(now.ToString("MM'/'dd'/'yyyy HH:mm:ss"));

            OutputData.writeToOutput("test1", "tttt");
            OutputData.writeToOutput("test2", "dddd");
            OutputData.writeToOutput("test1", "12345");
            OutputData.writeToOutput("test2", "54321");

            Logger.Info("hello");
        }
    }
}
