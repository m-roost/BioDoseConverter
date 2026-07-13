using AnalyticsLibrary2;
using GalaSoft.MvvmLight;
using JsonFileConfig;
using Lib_EQD2Gy_Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;
using VMS.TPS.Common.Model.API;

namespace BioDoseUI
{
    internal class MainViewModel : ViewModelBase
    {

        Patient pat;


        string NewCourseID(string oldID)
        {
            int limit = 16;

            string rv = prefix_CourseID + oldID + suffix_CourseID;

            if (rv.Length <= limit) return rv;

            int n_pre = prefix_CourseID.Length;

            int n_suf = suffix_CourseID.Length;

            rv = prefix_CourseID + oldID.Substring(0, limit - n_pre - n_suf) + suffix_CourseID;

            return rv.Substring(0, limit);
        }


        string prefix_CourseID = JsonConfig.ReadSetting<string>("prefix_CourseID");

        string suffix_CourseID = JsonConfig.ReadSetting<string>("suffix_CourseID");

        string prefix_PlanID = JsonConfig.ReadSetting<string>("prefix_PlanID");

        string suffix_PlanID = JsonConfig.ReadSetting<string>("suffix_PlanID");

        string Tag_In_Plan_Comment = JsonConfig.ReadSetting<string>("Tag_In_Plan_Comment");


        string NewPlanID(string oldID)
        {
            int limit = 13;

            string rv = prefix_PlanID + oldID + suffix_PlanID;

            if (rv.Length <= limit) return rv;

            int n_pre = prefix_PlanID.Length;

            int n_suf = suffix_PlanID.Length;

            rv = prefix_PlanID + oldID.Substring(0, limit - n_pre - n_suf) + suffix_PlanID;

            rv = rv.Substring(0,limit);

            return rv.Trim();
        }


        public static void RemoveNewPlanIdDuplications(IEnumerable<PlanInfo> list)
        {
            int limit = 13;

            var groupedByProperty = list.GroupBy(obj => obj.NewPlanID);

            foreach (var group in groupedByProperty)
            {
                if (group.Count() > 1)
                {
                    int counter = 0;

                    foreach (var obj in group) // Skip the first occurrence
                    {
                        string baseString = obj.NewPlanID;

                        if (string.IsNullOrEmpty(baseString)) break;

                        if (baseString.Length > limit - 1)
                        {
                            baseString = baseString.Substring(0, limit - 1);
                        }
                        
                        obj.NewPlanID = baseString + counter;
                        
                        counter++;
                    }
                }
            }
        }


        public MainViewModel(IEnumerable<PlanSetup> planSetupsInScope)
        {
            //PlanSetupsInScope = planSetupsInScope;

            foreach (var pl in planSetupsInScope)
            {
                var plrow = new PlanInfo()
                {
                    PlanID = pl.Id,
                    CourseID = pl.Course.Id,
                    NewCourseID = NewCourseID(pl.Course.Id),
                    NewPlanID = NewPlanID(pl.Id),
                    RxFraction = pl.NumberOfFractions ?? 0,
                    Alphabeta = 2.5,
                    IfUsed = true,
                    ref_ps = pl,

                    IsDoseValid = pl.IsDoseValid,
                    IsBioPlan = (prefix_CourseID.Length >=2 && pl.Course.Id.StartsWith(prefix_CourseID)) ||
                                (prefix_PlanID.Length >= 2 && pl.Id.StartsWith(prefix_PlanID)) || 
                                pl.Comment.Contains(Tag_In_Plan_Comment),

                    PlanStatus = pl.ApprovalStatus.ToString(),
                    CourseStatus = pl.Course.ClinicalStatus.ToString(),
                };


                if (!plrow.IsDoseValid)
                {
                    plrow.Status = "Dose not valid";
                    plrow.IfUsed = false;

                    plrow.NewCourseID = "";
                    plrow.NewPlanID = "";
                }

                if(plrow.IsBioPlan)
                {
                    plrow.Status = "Already Bio Dose";
                    plrow.IfUsed = false;

                    plrow.NewCourseID = "";
                    plrow.NewPlanID = "";
                }

                if (plrow.IfUsed) 
                {
                    plrow.Status = "Ready";
                }
                else
                {
                    plrow.NewPlanID = "";
                }

                PlanInfoList.Add(plrow);

            }

            RemoveNewPlanIdDuplications(PlanInfoList);

            pat = planSetupsInScope.First().Course.Patient;

            //pat.BeginModifications();

        }


        string _error_msg = "";
        public string error_msg { get { return _error_msg; } set { Set(ref _error_msg, value); } }

        public string Notes { get; set; } = JsonConfig.ReadSetting<string>("Notes");


        public ObservableCollection<PlanInfo> PlanInfoList { get; set; } = new ObservableCollection<PlanInfo>();

        //public IEnumerable<PlanSetup> PlanSetupsInScope { get; }

        private static string GetDuplicateSummary(IEnumerable<string> items)
        {
            var duplicates = items
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => $"{g.Key} ({g.Count()} times)")
                .ToList();

            return duplicates.Any()
                ? $"Duplicate new PlanID found: {string.Join(", ", duplicates)}"
                : string.Empty;
        }

        internal async Task ProcessRows(Patient pat, Course newC, System.Windows.Controls.Button btn)
        {
            try
            {
                var dup_msg = GetDuplicateSummary(PlanInfoList.Select(t => t.NewPlanID));

                if (!string.IsNullOrEmpty(dup_msg))
                {
                    MessageBox.Show(dup_msg + "\n\nPlease rename some of the New Plan IDs", "Error - EQD2Gy");

                    btn.IsEnabled = true;

                    return;
                }

                foreach (var pl in PlanInfoList.Where(t => t.IfUsed))
                {
                    Log3_static.writeBlankLines(1);

                    Log3_static.Information($"start process course: [{pl.CourseID}] plan: [{pl.PlanID}]");

                    pl.Status = "Processing..."; await Task.Delay(10);

                    PlanSetup sourcePlan = pl.ref_ps;

                    try
                    {
                        ExternalPlanSetup targetPlan = newC.AddExternalPlanSetup(sourcePlan.StructureSet);

                        targetPlan.Id = pl.NewPlanID;

                        var newDose = targetPlan.Copy_EvaluationDose_n_adjust_to_EQD2Gy(sourcePlan, pl.Alphabeta);

                        string Comment = $"{Tag_In_Plan_Comment}--alphabeta:{pl.Alphabeta}--SourcePlan:{pl.PlanID}--FromCourse:{pl.CourseID}--RequestedTime:{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}";

                        targetPlan.Comment = Comment;
                   
                        string msg = $"Old Dose UID: {pl.ref_ps.Dose?.UID} --- New Plan ID: {pl.NewPlanID} --- New Evaluation Dose UID: {newDose.UID}";

                        Log3_static.Information($"Finished - course: [{pl.CourseID}] plan: [{pl.PlanID}] --- {msg}");

                        pl.Status = "Finished"; await Task.Delay(10);

                    }
                    catch(Exception ex)
                    {
                        Log3_static.Information($"Failed - course: [{pl.CourseID}] plan: [{pl.PlanID}] ---  {ex}");

                        pl.Status = "Failed: " + ex.Message.Substring(0, 20); await Task.Delay(10);

                        if (string.IsNullOrEmpty(error_msg)) error_msg = "Error happened for some plan:\n\n";

                        error_msg += ex.Message;
                    }

                    Log3_static.writeBlankLines(2);

                }

                if(error_msg.Contains("Unable to get dose file from ARIA"))
                {
                    error_msg += "\n\nPlease try again on the failed plan.";
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString(), "Error - EQD2Gy");

                Log3_static.Error($"{ex.ToString()}");
            }
        }


        public static string GetLastPart(string path)
        {
            // Define the delimiters
            char[] delimiters = new char[] { '/', '\\' };

            // Split the string by both '/' and '\'
            string[] parts = path.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            // Retrieve the last part
            if (parts.Length > 0)
            {
                return parts[parts.Length - 1];
            }
            else
            {
                return ""; // Return an empty string if there are no parts
            }
        }
    }
}
