using System;
using System.Globalization;
using UnityEditor;

namespace Varwin.Editor
{
    public class BuildTimeUtils
    {
        public static DateTime StartTime
        {
            get
            {
                var str = SessionState.GetString("StartTime", DateTime.Now.ToString());
                return DateTime.Parse(str, CultureInfo.InvariantCulture);
            }
            set => SessionState.SetString("StartTime", value.ToString(CultureInfo.InvariantCulture));
        }
        
        public static DateTime FinishTime
        {
            get
            {
                var str = SessionState.GetString("FinishTime", DateTime.Now.ToString());
                return DateTime.Parse(str, CultureInfo.InvariantCulture);
            }
            set => SessionState.SetString("FinishTime", value.ToString(CultureInfo.InvariantCulture));
        }

        public static string GetBuildTime()
        {
            return (FinishTime - StartTime).ToString(@"hh\:mm\:ss");
        }
    }
}