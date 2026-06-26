using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioDoseUI
{
    internal class ExeAPI
    {
        public class ProcessResult
        {
            public int ExitCode { get; set; }
            public string Result { get; set; }
            public string Error { get; set; }
        }

        public static ProcessResult CallConsoleApp(string path, string argument)
        {
            var processResult = new ProcessResult();
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = argument,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();

                string result = process.StandardOutput.ReadToEnd();

                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                processResult.ExitCode = process.ExitCode;
                processResult.Result = result;
                processResult.Error = error;
            }
            catch (Exception ex)
            {
                processResult.Error = ex.Message;
            }

            return processResult;
        }
    }

}
