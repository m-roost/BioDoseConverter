using System;
using System.IO;
using System.Text;

namespace AnalyticsLibrary2
{

    public static class Log3_static // An exact copy of Log and Logger2 in AP_lib project. Added here to improvie logging in AnalyticsLibrary project.
    {
        private static logger3 logger;

        public static void Warning(string msg)
        {
            if (logger != null)
            {
                logger.WriteLine(msg, Log_levels.warn);
            }
        }

        public static void Error(string msg)
        {
            if (logger != null)
            {
                logger.WriteLine(msg, Log_levels.error);
            }
        }

        public static void Debug(string msg)
        {
            if (logger != null)
            {
                logger.WriteLine(msg, Log_levels.debug);
            }
        }


        public static void Information(string msg)
        {
            if (logger != null)
            {
                logger.WriteLine(msg, Log_levels.info);
            }
        }

        public static void initiate_logger3(string filePath, Log_levels minimal_level = Log_levels.debug)
        {
            // Get folder path
            string dir = Path.GetDirectoryName(filePath);

            // Create full directory structure if needed
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            logger = new logger3(filePath, minimal_level);
        }


        public static void initiate_logger3_intLocalDirectory(string logFileName = "", Log_levels minimal_level = Log_levels.debug, string logFolderName = "Logs")
        {
            if (string.IsNullOrEmpty(logFileName))
            {
                logFileName = "log_" +
                    Environment.UserName.Replace(@"\", "_").Replace(@"/", "_").Replace(":", "_") +
                    DateTime.Now.ToString("_yyyy_MM") + ".txt";
            }

            string dllFolder = Path.GetDirectoryName(typeof(AnalyticsLibrary2.logger3).Assembly.Location);

            string logFolder = Path.Combine(dllFolder, logFolderName);
            
            Directory.CreateDirectory(logFolder);

            string logFilePath = Path.Combine(logFolder, logFileName);

            logger = new logger3(logFilePath, minimal_level);
        }


        public static void writeBlankLines(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Log3_static.Error("");
            }
        }

    }

    public class logger3
    {
        private string DatetimeFormat = "yyyy-MM-dd HH-mm-ss.fff";
        private string Filename;



        public logger3(string path_filename, Log_levels lv = Log_levels.debug)
        {
            Filename = path_filename;
            level_min = lv;
        
            Directory.CreateDirectory(Path.GetDirectoryName(path_filename));
        }

        private readonly Log_levels level_min;

        internal void WriteLine(string text, Log_levels level = Log_levels.debug)
        {
            if (level < level_min) return;

            try
            {
                using (StreamWriter Writer = new StreamWriter(Filename, true, Encoding.UTF8))
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        Writer.WriteLine(DateTime.Now.ToString(DatetimeFormat) + "\t" + text);
                    }
                    else
                    {
                        Writer.WriteLine();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Logger3 is not working properly.\n\nPlease notify developer.", e);
            }
        }
    }

    public enum Log_levels
    {
        debug,
        info,
        warn,
        error
    }
}
