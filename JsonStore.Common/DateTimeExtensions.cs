using System;

namespace Money.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsGreaterThan(this DateTimeOffset thisInstant, DateTimeOffset otherInstant, DateTimeRounding rounding = DateTimeRounding.Second)
        {
            return Difference(thisInstant, otherInstant, rounding) > TimeSpan.Zero;
        }

        public static bool IsLessThan(this DateTimeOffset thisInstant, DateTimeOffset otherInstant, DateTimeRounding rounding = DateTimeRounding.Second)
        {
            return Difference(otherInstant, thisInstant, rounding) > TimeSpan.Zero;
        }

        private static TimeSpan Difference(DateTimeOffset leftInstant, DateTimeOffset rightInstant, DateTimeRounding rounding = DateTimeRounding.Second)
        {
            var leftInstantUtc = leftInstant.ToUniversalTime();
            var rightInstantUtc = rightInstant.ToUniversalTime();

            var difference = (leftInstantUtc - rightInstantUtc);
            switch (rounding)
            {
                case DateTimeRounding.Second:
                    difference = TimeSpan.FromSeconds(Math.Floor(difference.TotalSeconds));
                    break;
            }
            return difference;
        }
    }

    //---------------
    public enum DateTimeRounding
    {
        None = 0,
        Second = 1
    }

}
