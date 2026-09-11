namespace RequestFlow.Common.Helper
{
    public static class DateHelper
    {
        public static string CalculateTimeAgo(DateTime FromdateTime, DateTime Todatetime)
        {
            if (FromdateTime == DateTime.MinValue)//incase of no time 
                return "N/A";

            var Time = (Todatetime - FromdateTime);

            if (Time.Days > DateTime.DaysInMonth(FromdateTime.Year, FromdateTime.Month))
                return (FromdateTime.Date.ToShortDateString());
            else if (Time.TotalHours > 23)
                return (string.Format("{0}d", Convert.ToInt32(Time.TotalDays)));
            else if (Time.TotalMinutes > 59)
                return (string.Format("{0}h", Convert.ToInt32(Time.TotalHours)));
            else if (Time.TotalMinutes < 59 && Time.TotalSeconds > 59)
                return (string.Format("{0}m", Convert.ToInt32(Time.TotalMinutes)));
            else if (Time.TotalSeconds < 59)
                return (string.Format("{0}s", Convert.ToInt32(Time.TotalSeconds)));
            else
                return "N/A";
        }
    }
}
