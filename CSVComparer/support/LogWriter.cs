using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSVComparer.support
{
    public static class LogWriter
    {
        private static readonly string LogDirPath = "Logs";
        private static readonly string LogFilePath = $"{LogDirPath}/Log_{DateTime.Now:yyyy-dd-M--HH-mm-ss}.txt";

        public static void WriteLine(string message = "")
        {
            Write(message + Environment.NewLine);
        }

        public static void Write(string message = "")
        {
            if (!Directory.Exists(LogDirPath))
            {
                Directory.CreateDirectory(LogDirPath);
            }

            File.AppendAllText(LogFilePath, message);
            Console.Write(message);
        }
    }


}
