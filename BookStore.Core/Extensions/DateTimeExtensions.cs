namespace BookStore.Core.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public static DateTime ToVietnamTime(this DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
        }

        public static DateTime GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        }

        public static DateTime ToUtcFromVietnam(this DateTime vietnamDateTime)
        {
            if (vietnamDateTime.Kind == DateTimeKind.Utc)
            {
                return vietnamDateTime;
            }
            
            var vietnamTime = DateTime.SpecifyKind(vietnamDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(vietnamTime, VietnamTimeZone);
        }

        public static string ToVietnamString(this DateTime dateTime, string format = "dd/MM/yyyy HH:mm:ss")
        {
            var vietnamTime = dateTime.Kind == DateTimeKind.Utc 
                ? dateTime.ToVietnamTime() 
                : dateTime;
            
            return vietnamTime.ToString(format, new System.Globalization.CultureInfo("vi-VN"));
        }
    }
}
