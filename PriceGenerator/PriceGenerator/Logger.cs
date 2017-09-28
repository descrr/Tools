using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceGenerator
{
    public class Logger
    {
        private static string LogFileName = string.Format(@"{0}\log.txt", Directory.GetCurrentDirectory());
        public static void LogMessage(string message)
        {
            string prefix = Environment.NewLine + string.Format("{0}.{1}.{2} {3}:{4}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, DateTime.Now.Hour, DateTime.Now.Minute);
            if(File.Exists(LogFileName))
                File.AppendAllText(LogFileName, string.Format("{0}--> {1}{2}", prefix, message, Environment.NewLine));
            else
                File.WriteAllText(LogFileName, message);
        }
    }
}
