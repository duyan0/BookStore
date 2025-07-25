using BookStore.Core.Services;
using System.Globalization;

namespace BookStore.Infrastructure.Services
{
    public class TimezoneService : ITimezoneService
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        private static readonly CultureInfo VietnamCulture = new CultureInfo("vi-VN");

        public DateTime GetVietnamTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        }

        public DateTime GetVietnamTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
        }

        public DateTime ConvertToUtc(DateTime vietnamDateTime)
        {
            if (vietnamDateTime.Kind == DateTimeKind.Utc)
            {
                return vietnamDateTime;
            }
            
            var vietnamTime = DateTime.SpecifyKind(vietnamDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(vietnamTime, VietnamTimeZone);
        }

        public string FormatVietnamDateTime(DateTime dateTime, string format = "dd/MM/yyyy HH:mm:ss")
        {
            var vietnamTime = dateTime.Kind == DateTimeKind.Utc 
                ? GetVietnamTime(dateTime) 
                : dateTime;
            
            return vietnamTime.ToString(format, VietnamCulture);
        }

        public DateTime ParseVietnamDateTime(string dateTimeString)
        {
            if (DateTime.TryParse(dateTimeString, VietnamCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            
            throw new FormatException($"Unable to parse datetime string: {dateTimeString}");
        }
    }
}
