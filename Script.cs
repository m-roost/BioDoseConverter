using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using BioDoseUI;
using AnalyticsLibrary2;
using JsonFileConfig;

[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }
       

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext scriptContext)
        {
            if(scriptContext.Patient == null || scriptContext.PlansInScope == null || scriptContext.PlansInScope.Count() == 0)
            {
                MessageBox.Show("Please open a patient and at least one plan in Eclipse before running this script.", "Info - BioDoseConverter");
                
                return;
            }   

            //scriptContext.Patient.BeginModifications();

            Run(scriptContext.CurrentUser,
                scriptContext.Patient,
                scriptContext.Image,
                scriptContext.StructureSet,
                scriptContext.PlanSetup,
                scriptContext.PlansInScope,
                scriptContext.PlanSumsInScope, null);

        }

        public void Run(
            User user,
            Patient patient,
            Image image,
            StructureSet structureSet,
            PlanSetup planSetup,
            IEnumerable<PlanSetup> planSetupsInScope,
            IEnumerable<PlanSum> planSumsInScope,
            Window window)
        {
            try
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                string logFileName = @"\\uhrofilespr1\EclipseScripts\Aria18\Data\BioDoseUI\log_" +
                    Environment.UserName.Replace(@"\", "_").Replace(@"/", "_").Replace(":", "_") +
                    DateTime.Now.ToString("_yyyy_MM") + ".txt";


                Log3_static.initiate_logger3(logFileName, Log_levels.info);

                Log3_static.writeBlankLines(10);

                Log3_static.Information($"{Environment.UserName}\t{Environment.MachineName} --- BioDoseDicom UI ---  {Assembly.GetExecutingAssembly().Location}; {version}");


                //var xxx = MessageBox.Show(
                //    JsonConfig.ReadSetting<string>("Warning"),
                //    JsonConfig.ReadSetting<string>("AppName"),
                //    MessageBoxButton.OKCancel, 
                //    MessageBoxImage.Information);

                //if (xxx == MessageBoxResult.Cancel) return;

                // TTT Check Approval status!! Approved plan can cause problem here.


                var msg = "";
                var n_not_unapproved = 0;
                var ids_not_unapproved = new List<string>();

                Log3_static.writeBlankLines(1);

                Log3_static.Information($"------------ Script Context Patient: [{patient.Id}] -----------");

                foreach (var ps in planSetupsInScope)
                {
                    Log3_static.Information(
                        $"    Course: {ps.Course.Id,-16} ({ps.Course.ClinicalStatus.ToString()})"
                        + $"    Plan: {ps.Id,-16} ({ps.ApprovalStatus.ToString()})");

                    if (ps.ApprovalStatus != PlanSetupApprovalStatus.UnApproved)
                    {
                        n_not_unapproved++;

                        ids_not_unapproved.Add(ps.Id);
                    }
                }

                ////Log3_static.Information(msg);

                //if (n_not_unapproved > 0)
                //{
                //    Console.ForegroundColor = ConsoleColor.Red;
                //    Console.WriteLine($"\n\nOnly UnApproved Plans can be copied into new course. Other plans [{string.Join(", ", ids_not_unapproved)}] will be ignored.\n\n");
                //    Console.ResetColor();
                //}



                bool is_loaded = planSetup == null;

                int cn_plan = planSetupsInScope.Count();

                int cn_plansum = planSumsInScope.Count();

                window = new MainWindow(planSetupsInScope);

                window.Title = JsonConfig.ReadSetting<string>("AppName") + " v" + version;

                window.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error - BioDoseConverter");
                Log3_static.Error(ex.ToString());
            }   

        }
    }
}
