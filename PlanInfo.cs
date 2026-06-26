using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace BioDoseUI
{
    public class PlanInfo : ViewModelBase
    {
        private string _courseID;
        public string CourseID
        {
            get { return _courseID; }
            set { Set(ref _courseID, value); }
        }

        private string _CourseStatus;
        public string CourseStatus
        {
            get { return _CourseStatus; }
            set { Set(ref _CourseStatus, value); }
        }

        private string _planID;
        public string PlanID
        {
            get { return _planID; }
            set { Set(ref _planID, value); }
        }

        private string _PlanStatus;
        public string PlanStatus
        {
            get { return _PlanStatus; }
            set { Set(ref _PlanStatus, value); }
        }

        private string _newPlanID;
        public string NewPlanID
        {
            get { return _newPlanID; }
            set { Set(ref _newPlanID, value); }
        }

        private double _alphabeta;
        public double Alphabeta
        {
            get { return _alphabeta; }
            set { Set(ref _alphabeta, value); }
        }

        private int _rxFraction;
        public int RxFraction
        {
            get { return _rxFraction; }
            set { Set(ref _rxFraction, value); }
        }

        private bool _ifUsed = true;
        public bool IfUsed
        {
            get { return _ifUsed; }
            set { Set(ref _ifUsed, value); }
        }

        private string _status = "";

        public string Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }

        private string _newCourseID;

        public string NewCourseID
        {
            get { return _newCourseID; }
            set { Set(ref _newCourseID, value); }
        }

        private bool _isDoseValid;
        public bool IsDoseValid
        {
            get { return _isDoseValid; }
            set { Set(ref _isDoseValid, value); }
        }



        private bool _isBioPlan;
        public bool IsBioPlan
        {
            get { return _isBioPlan; }
            set { Set(ref _isBioPlan, value); }
        }


        internal PlanSetup ref_ps;
    
    }
}
