using System;
using System.Globalization;

namespace Siotrix.Discord
{
    public static class DateTimeExtensions
    {
        // because dotnet core has no ToLongTimeString support at the moment.
        public static string ToShortDateString(this DateTime dateTime)
        {
            return dateTime.ToString("d");
        }

        public static string ToShortTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("t");
        }

        public static string ToLongDateString(this DateTime dateTime)
        {
            return dateTime.ToString("D");
        }

        public static string ToLongTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("T");
        }

        public static string FormatDateTime(DateTime? dt)
        {
            if (!dt.HasValue)
                return "N/A";

            var ndt = dt.Value.ToUniversalTime();
            return string.Format("{0} {1}, {2} at {3}",
                CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(ndt.Month),
                ndt.Day,
                ndt.Year,
                ndt.ToLongTimeString());
        }

        public static string FormatDateTime(DateTimeOffset? dt)
        {
            if (!dt.HasValue)
                return "N/A";

            return FormatDateTime(dt.Value.UtcDateTime);
        }

        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var culture = CultureInfo.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;
            if (diff < 0)
                diff += 7;
            return dt.AddDays(-diff).Date;
        }

        public static DateTime LastDayOfWeek(this DateTime dt)
        {
            return dt.FirstDayOfWeek().AddDays(6);
        }

        public static DateTime FirstDayOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime dt)
        {
            return dt.FirstDayOfMonth().AddMonths(1).AddDays(-1);
        }

        public static DateTime FirstDayOfNextMonth(this DateTime dt)
        {
            return dt.FirstDayOfMonth().AddMonths(1);
        }
    }
}