using AnalyticsLibrary2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace BioDoseUI
{
    public static class ESAPI_Extension_Writable
    {

        public static Course Add_or_Select_Course(this Patient curpat, string CourseID)
        {
            Course curcourse = null;

            // if Course with specified Course ID exist.
            if (curpat.Courses.Where(x => x.Id == CourseID).Any())
            {
                curcourse = curpat.Courses.Where(x => x.Id == CourseID).Single();
                Log3_static.Information($"Use Existing Course: {CourseID}");
            }

            if (curcourse == null)
            {
                curcourse = curpat.AddCourse();
                curcourse.Id = CourseID;
                Log3_static.Information($"Use Existing Course: {CourseID}");
            }

            return curcourse;
        }

    }
}
