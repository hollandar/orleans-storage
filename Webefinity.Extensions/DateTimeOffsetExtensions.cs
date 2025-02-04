namespace Webefinity.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static string RelativeTime(this DateTimeOffset dateTime)
        {

            string relativity = "{0} ago";
            if (dateTime.ToLocalTime() > DateTimeOffset.Now)
            {
                relativity = "in {0}";
            }

            var timeSpan = DateTimeOffset.Now - dateTime.ToLocalTime();
            if (Math.Abs(timeSpan.TotalSeconds) < 60)
            {
                return "Just now";
            }
            if (Math.Abs(timeSpan.TotalMinutes) < 60)
            {
                return String.Format(relativity, $"{Math.Abs(timeSpan.Minutes)} minutes");
            }
            if (Math.Abs(timeSpan.TotalHours) < 24)
            {
                return String.Format(relativity, $"{Math.Abs(timeSpan.Hours)} hours");
            }
            if (Math.Abs(timeSpan.TotalDays) < 7)
            {
                return String.Format(relativity, $"{Math.Abs(timeSpan.Days)} days");
            }
            if (Math.Abs(timeSpan.TotalDays) < 30)
            {
                return String.Format(relativity, $"{Math.Abs(timeSpan.Days / 7)} weeks");
            }
            if (Math.Abs(timeSpan.TotalDays) < 365)
            {
                return String.Format(relativity, $"{Math.Abs(timeSpan.Days / 30)} months");
            }
            return String.Format(relativity, $"{Math.Abs(timeSpan.Days / 365)} years");
        }

    }
}
