using System.Globalization;

namespace CodingTracker
{
    internal class Validation
    {
        public bool ValidateDate(string date)
        {
            return DateTime.TryParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        public bool ValidateTime(string time)
        {
            return DateTime.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        public bool ValidateDateTime(string dateTime)
        {
            return DateTime.TryParseExact(dateTime, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        public bool ValidateDateTimeRange(DateTime startDateTime, DateTime endDateTime)
        {
            if (startDateTime > DateTime.Now || endDateTime > DateTime.Now || endDateTime < startDateTime)
            {
                return false;
            }
            return true;
        }
    }
}