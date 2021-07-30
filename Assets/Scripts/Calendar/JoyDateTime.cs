using System;

namespace JoyGodot.Assets.Scripts.Calendar
{
    public class JoyDateTime :
        IComparable,
        IComparable<JoyDateTime>,
        IEquatable<JoyDateTime>
    {
        protected const long TicksPerMillisecond = 10000;
        protected const long TicksPerSecond = 10000000;
        protected const long TicksPerMinute = 600000000;
        protected const long TicksPerHour = 36000000000;
        protected const long TicksPerDay = 864000000000;
        protected const long TicksPerMonth = TicksPerDay * DaysPerMonth;
        protected const long TicksPerYear = TicksPerMonth * MonthsPerYear;
        public const int MillisPerSecond = 1000;
        public const int MillisPerMinute = 60000;
        public const int MillisPerHour = 3600000;
        public const int MillisPerDay = 86400000;
        public const int SecondsPerMinute = 60;
        public const int MinutesPerHour = 60;
        public const int HoursPerDay = 24;
        public const int DaysPerWeek = 7;
        public const int DaysPerMonth = 40;
        public const int DaysPerYear = 400;
        public const int MonthsPerYear = 10;
        protected const long MinTicks = 0;
        protected const long MaxTicks = 3155378975999999999;
        protected const long MaxMillis = 315537897600000;
        protected const long FileTimeOffset = 504911232000000000;
        protected const long DoubleDateOffset = 599264352000000000;
        protected const long OADateMinAsTicks = 31241376000000000;
        protected const double OADateMinAsDouble = -657435.0;
        protected const double OADateMaxAsDouble = 2958466.0;

        protected const long TickOffset = TicksPerDay;

        public int Year => (int) (this.InternalTicks / TicksPerYear);
        public int Month => (int) ((this.InternalTicks / TicksPerMonth) % MonthsPerYear);
        public int DayOfMonth => (int) ((this.InternalTicks / TicksPerDay) % DaysPerMonth);
        public int Hour => (int) ((this.InternalTicks / TicksPerHour) % HoursPerDay);
        public int Minute => (int) ((this.InternalTicks / TicksPerMinute) % MinutesPerHour);
        public int Second => (int) ((this.InternalTicks / TicksPerSecond) % SecondsPerMinute);
        public int Millisecond => (int) (this.InternalTicks / TicksPerSecond);

        public int DayOfWeek => (int) (this.InternalTicks / TicksPerDay) % DaysPerWeek;
        public int DayOfYear => (int) (this.InternalTicks / TicksPerDay) % DaysPerYear;
        public string DayOfWeekName => this.GetDayOfWeek();
        
        public DayOfWeek FirstDayOfMonth => ((DayOfWeek) (this.ModifyDays(-(this.DayOfMonth)).DayOfWeek));

        public long InternalTicks { get; protected set; }

        public JoyDateTime(
            int year,
            int month,
            int day,
            int hour = 0,
            int minute = 0,
            int second = 0,
            int millisecond = 0)
        {
            this.InternalTicks = this.DateToTicks(year - 1, month - 1, day - 1) + 
                                 this.TimeToTicks(hour, minute, second, millisecond);
        }

        public JoyDateTime(long ticks)
        {
            this.InternalTicks = ticks;
        }

        protected string GetDayOfWeek()
        {
            return ((DayOfWeek) this.DayOfWeek).ToString();
        }

        public JoyDateTime ModifySeconds(int value)
        {
            return new JoyDateTime(
                this.InternalTicks + (value * TicksPerSecond));
        }

        public JoyDateTime ModifyMinutes(int value)
        {
            return new JoyDateTime(
                this.InternalTicks + (value * TicksPerMinute));
        }

        public JoyDateTime ModifyHours(int value)
        {
            return new JoyDateTime(
                this.InternalTicks + (value * TicksPerHour));
        }

        public JoyDateTime ModifyDays(int value)
        {
            return new JoyDateTime(
                this.InternalTicks + (value * TicksPerDay));
        }

        public JoyDateTime ModifyMonths(int value)
        {
            return new JoyDateTime(
                this.InternalTicks + (value * TicksPerMonth));
        }

        public JoyDateTime ModifyYears(int value)
        {
            return new JoyDateTime(
                this.InternalTicks + (value * TicksPerYear));
        }

        public int CompareTo(object obj)
        {
            if (!(obj is JoyDateTime everseDateTime))
            {
                return 1;
            }

            return this.CompareTo(everseDateTime);
        }

        public int CompareTo(JoyDateTime other)
        {
            long num = other.InternalTicks;
            if (this.InternalTicks > num)
            {
                return 1;
            }

            return this.InternalTicks < num ? -1 : 0;
        }

        protected long DateToTicks(
            int year,
            int month,
            int day)
        {
            return ((year * TicksPerYear) + (month * TicksPerMonth) + (day * TicksPerDay));
        }

        protected long TimeToTicks(
            int hour,
            int minute,
            int second,
            int millisecond = 0)
        {
            return (hour * TicksPerHour) + (minute * TicksPerMinute) + (second * TicksPerSecond) +
                   (millisecond * TicksPerMillisecond);
        }

        public override string ToString()
        {
            return this.DayOfMonth + " / " + this.Month + " / " + this.Year;
        }

        public bool Equals(JoyDateTime other)
        {
            return other?.InternalTicks == this.InternalTicks;
        }
    }
}