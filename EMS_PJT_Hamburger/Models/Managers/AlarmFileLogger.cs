using System;
using System.IO;
using System.Text;

namespace EMS_PJT_Hamburger.Models.Managers
{
    public static class AlarmFileLogger
    {
        private static readonly object SyncRoot = new object();

        public static void WriteFault(
            string source,
            string category,
            int code,
            string name,
            string message,
            string rawValue,
            DateTime occurredAt)
        {
            try
            {
                var time = occurredAt == default(DateTime) ? DateTime.Now : occurredAt;
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var logDirectory = Path.Combine(baseDirectory, "logs", "alarm_fault");
                Directory.CreateDirectory(logDirectory);

                var filePath = Path.Combine(logDirectory, $"alarm_fault_{time:yyyyMMdd}.log");
                var line = string.Join("\t",
                    time.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Safe(source),
                    Safe(category),
                    code.ToString(),
                    Safe(name),
                    Safe(message),
                    Safe(rawValue));

                lock (SyncRoot)
                {
                    File.AppendAllText(filePath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // File logging must never interrupt alarm processing.
            }
        }

        private static string Safe(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "-"
                : value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        }
    }
}
