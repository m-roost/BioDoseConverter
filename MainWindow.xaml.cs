using AnalyticsLibrary2;
using JsonFileConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;

namespace BioDoseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IEnumerable<PlanSetup> planSetupsInScope)
        {
            InitializeComponent();

            var _VM = new MainViewModel(planSetupsInScope);

            DataContext = _VM;

            VM = _VM;

            pat = planSetupsInScope.First().Course.Patient;

            patientID = pat.Id;

            existingCourseIds = planSetupsInScope.First().Course.Patient.Courses.Select(t => t.Id).ToList();

            singleNewCourseID.Text = "$EQD2Gy_" + DateTime.Now.ToString("yyyyMMdd");

        }

        private Patient pat;

        private MainViewModel VM;

        private string patientID;

        private List<string> existingCourseIds;


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.error_msg = "";

                if (string.IsNullOrEmpty(singleNewCourseID.Text))
                {
                    VM.error_msg = "Please provide New Course ID";

                    return;
                }

                if (singleNewCourseID.Text.Length > 16)
                {
                    VM.error_msg = "New Course ID cannot have more than 16 characters";

                    return;
                }

                if (existingCourseIds.Any(t => t.ToUpper() == singleNewCourseID.Text.ToUpper()))
                {
                    VM.error_msg = $"New Course ID [{singleNewCourseID.Text}] already exist in this patient. Please use a different Course ID.";

                    return;
                }


              

                Log3_static.writeBlankLines(1);


                pat.BeginModifications();

                var newC = pat.AddCourse();

                newC.Id = singleNewCourseID.Text;

                Log3_static.Information($"new course [{newC.Id}] created");

                var btn = sender as Button;
                
                btn.IsEnabled = false;

                await VM.ProcessRows(pat, newC);

                CancelBtn.Content = "Close";
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString(), "Error - " + this.Title);

                Log3_static.Error($"{ex.ToString()}");

                this.Close();
            }
        }

        private void MyDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var item = e.Row.Item as PlanInfo; // Replace YourDataType with your actual data type
            
            if (item != null && ShouldBeDisabled(item)) // Implement your disabling logic
            {
                e.Row.Background = new SolidColorBrush(Colors.Gainsboro); // Change appearance

                e.Row.IsEnabled = false; // This makes the row non-interactive, but visually might not be enough
            }

            DataGrid dataGrid = sender as DataGrid;

            dataGrid.UnselectAll();
        }

        private bool ShouldBeDisabled(PlanInfo item)
        {
            if(item.IsDoseValid == false)
            {
                return true;
            }

            if(item.IsBioPlan == true)
            {
                return true;
            }

            return false;
        }

        private void MyDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // not used any more, since Loading row event is used later.
            //DataGrid dataGrid = sender as DataGrid;

            //dataGrid.UnselectAll();
        }
    }


}
