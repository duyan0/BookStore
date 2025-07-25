namespace BookStore.Core.Services
{
    public interface ITimezoneService
    {
        DateTime GetVietnamTime();
        DateTime GetVietnamTime(DateTime utcDateTime);
        DateTime ConvertToUtc(DateTime vietnamDateTime);
        string FormatVietnamDateTime(DateTime dateTime, string format = "dd/MM/yyyy HH:mm:ss");
        DateTime ParseVietnamDateTime(string dateTimeString);
    }
}
