// EDBTypes\DateDatatypes.cs
//
// Author:
//    Jon Hanna. (jon@hackcraft.net)
//
//    Copyright (C) 2007-2008 The EnterpriseDB.EDBClient Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EnterpriseDB.EDBClient;

//TODO: Lots of convenience methods! There should be nothing you can do with datetime and timestamp that you can't
//do just as well with these - and hence no reason not to choose these if they are appropriate.
//Similarly, lots of documentation is a must.

// Keep the xml comment warning quiet for this file.
#pragma warning disable 1591

namespace EDBTypes
{
    /// <summary>
    /// Represents the PostgreSQL interval datatype.
    /// <remarks>PostgreSQL differs from .NET in how it's interval type doesn't assume 24 hours in a day
    /// (to deal with 23- and 25-hour days caused by daylight savings adjustments) and has a concept
    /// of months that doesn't exist in .NET's <see cref="TimeSpan"/> class. (Neither datatype
    /// has any concessions for leap-seconds).
    /// <para>For most uses just casting to and from TimeSpan will work correctly &#x2014; in particular,
    /// the results of subtracting one <see cref="DateTime"/> or the PostgreSQL date, time and
    /// timestamp types from another should be the same whether you do so in .NET or PostgreSQL &#x2014;
    /// but if the handling of days and months in PostgreSQL is important to your application then you
    /// should use this class instead of <see cref="TimeSpan"/>.</para>
    /// <para>If you don't know whether these differences are important to your application, they
    /// probably arent! Just use <see cref="TimeSpan"/> and do not use this class directly &#x263a;</para>
    /// <para>To avoid forcing unnecessary provider-specific concerns on users who need not be concerned
    /// with them a call to <see cref="System.Data.IDataRecord.GetValue(int)"/> on a field containing an
    /// <see cref="EDBInterval"/> value will return a <see cref="TimeSpan"/> rather than an
    /// <see cref="EDBInterval"/>. If you need the extra functionality of <see cref="EDBInterval"/>
    /// then use <see cref="EnterpriseDB.EDBClient.EDBDataReader.GetInterval(Int32)"/>.</para>
    /// </remarks>
    /// <seealso cref="Ticks"/>
    /// <seealso cref="JustifyDays"/>
    /// <seealso cref="JustifyMonths"/>
    /// <seealso cref="Canonicalize()"/>
    /// </summary>
    [Serializable]
    public struct EDBInterval : IComparable, IComparer, IEquatable<EDBInterval>, IComparable<EDBInterval>,
                                   IComparer<EDBInterval>
    {
        #region Constants

        /// <summary>
        /// Represents the number of ticks (100ns periods) in one microsecond. This field is constant.
        /// </summary>
        public const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond/1000;

        /// <summary>
        /// Represents the number of ticks (100ns periods) in one millisecond. This field is constant.
        /// </summary>
        public const long TicksPerMillsecond = TimeSpan.TicksPerMillisecond;

        /// <summary>
        /// Represents the number of ticks (100ns periods) in one second. This field is constant.
        /// </summary>
        public const long TicksPerSecond = TimeSpan.TicksPerSecond;

        /// <summary>
        /// Represents the number of ticks (100ns periods) in one minute. This field is constant.
        /// </summary>
        public const long TicksPerMinute = TimeSpan.TicksPerMinute;

        /// <summary>
        /// Represents the number of ticks (100ns periods) in one hour. This field is constant.
        /// </summary>
        public const long TicksPerHour = TimeSpan.TicksPerHour;

        /// <summary>
        /// Represents the number of ticks (100ns periods) in one day. This field is constant.
        /// </summary>
        public const long TicksPerDay = TimeSpan.TicksPerDay;

        /// <summary>
        /// Represents the number of hours in one day (assuming no daylight savings adjustments). This field is constant.
        /// </summary>
        public const int HoursPerDay = 24;

        /// <summary>
        /// Represents the number of days assumed in one month if month justification or unjustifcation is performed.
        /// This is set to 30 for consistency with PostgreSQL. Note that this is means that month adjustments cause
        /// a year to be taken as 30 &#xd7; 12 = 360 rather than 356/366 days.
        /// </summary>
        public const int DaysPerMonth = 30;

        /// <summary>
        /// Represents the number of ticks (100ns periods) in one day, assuming 30 days per month. <seealso cref="DaysPerMonth"/>
        /// </summary>
        public const long TicksPerMonth = TicksPerDay*DaysPerMonth;

        /// <summary>
        /// Represents the number of months in a year. This field is constant.
        /// </summary>
        public const int MonthsPerYear = 12;

        /// <summary>
        /// Represents the maximum <see cref="EDBInterval"/>. This field is read-only.
        /// </summary>
        public static readonly EDBInterval MaxValue = new EDBInterval(long.MaxValue);

        /// <summary>
        /// Represents the minimum <see cref="EDBInterval"/>. This field is read-only.
        /// </summary>
        public static readonly EDBInterval MinValue = new EDBInterval(long.MinValue);

        /// <summary>
        /// Represents the zero <see cref="EDBInterval"/>. This field is read-only.
        /// </summary>
        public static readonly EDBInterval Zero = new EDBInterval(0);

        #endregion

        private readonly int _months;
        private readonly int _days;
        private readonly long _ticks;

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="EDBInterval"/> to the specified number of ticks.
        /// </summary>
        /// <param name="ticks">A time period expressed in 100ns units.</param>
        public EDBInterval(long ticks)
            : this(new TimeSpan(ticks))
        {
        }

        /// <summary>
        /// Initializes a new <see cref="EDBInterval"/> to hold the same time as a <see cref="TimeSpan"/>
        /// </summary>
        /// <param name="timespan">A time period expressed in a <see cref="TimeSpan"/></param>
        public EDBInterval(TimeSpan timespan)
            : this(0, timespan.Days, timespan.Ticks - (TicksPerDay * timespan.Days))
        {
        }

        /// <summary>
        /// Initializes a new <see cref="EDBInterval"/> to the specified number of months, days
        /// &amp; ticks.
        /// </summary>
        /// <param name="months">Number of months.</param>
        /// <param name="days">Number of days.</param>
        /// <param name="ticks">Number of 100ns units.</param>
        public EDBInterval(int months, int days, long ticks)
        {
            _months = months;
            _days = days;
            _ticks = ticks;
        }

        /// <summary>
        /// Initializes a new <see cref="EDBInterval"/> to the specified number of
        /// days, hours, minutes &amp; seconds.
        /// </summary>
        /// <param name="days">Number of days.</param>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        public EDBInterval(int days, int hours, int minutes, int seconds)
            : this(0, days, new TimeSpan(hours, minutes, seconds).Ticks)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="EDBInterval"/> to the specified number of
        /// days, hours, minutes, seconds &amp; milliseconds.
        /// </summary>
        /// <param name="days">Number of days.</param>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        public EDBInterval(int days, int hours, int minutes, int seconds, int milliseconds)
            : this(0, days, new TimeSpan(0, hours, minutes, seconds, milliseconds).Ticks)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="EDBInterval"/> to the specified number of
        /// months, days, hours, minutes, seconds &amp; milliseconds.
        /// </summary>
        /// <param name="months">Number of months.</param>
        /// <param name="days">Number of days.</param>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        public EDBInterval(int months, int days, int hours, int minutes, int seconds, int milliseconds)
            : this(months, days, new TimeSpan(0, hours, minutes, seconds, milliseconds).Ticks)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="EDBInterval"/> to the specified number of
        /// years, months, days, hours, minutes, seconds &amp; milliseconds.
        /// <para>Years are calculated exactly equivalent to 12 months.</para>
        /// </summary>
        /// <param name="years">Number of years.</param>
        /// <param name="months">Number of months.</param>
        /// <param name="days">Number of days.</param>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        public EDBInterval(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
            : this(years*12 + months, days, new TimeSpan(0, hours, minutes, seconds, milliseconds).Ticks)
        {
        }

        #endregion

        #region Whole Parts

        /// <summary>
        /// The total number of ticks(100ns units) contained. This is the resolution of the
        /// <see cref="EDBInterval"/>  type. This ignores the number of days and
        /// months held. If you want them included use <see cref="UnjustifyInterval()"/> first.
        /// <remarks>The resolution of the PostgreSQL
        /// interval type is by default 1&#xb5;s = 1,000 ns. It may be smaller as follows:
        /// <list type="number">
        /// <item>
        /// <term>interval(0)</term>
        /// <description>resolution of 1s (1 second)</description>
        /// </item>
        /// <item>
        /// <term>interval(1)</term>
        /// <description>resolution of 100ms = 0.1s (100 milliseconds)</description>
        /// </item>
        /// <item>
        /// <term>interval(2)</term>
        /// <description>resolution of 10ms = 0.01s (10 milliseconds)</description>
        /// </item>
        /// <item>
        /// <term>interval(3)</term>
        /// <description>resolution of 1ms = 0.001s (1 millisecond)</description>
        /// </item>
        /// <item>
        /// <term>interval(4)</term>
        /// <description>resolution of 100&#xb5;s = 0.0001s (100 microseconds)</description>
        /// </item>
        /// <item>
        /// <term>interval(5)</term>
        /// <description>resolution of 10&#xb5;s = 0.00001s (10 microseconds)</description>
        /// </item>
        /// <item>
        /// <term>interval(6) or interval</term>
        /// <description>resolution of 1&#xb5;s = 0.000001s (1 microsecond)</description>
        /// </item>
        /// </list>
        /// <para>As such, if the 100-nanosecond resolution is significant to an application, a PostgreSQL interval will
        /// not suffice for those purposes.</para>
        /// <para>In more frequent cases though, the resolution of the interval suffices.
        /// <see cref="EDBInterval"/> will always suffice to handle the resolution of any interval value, and upon
        /// writing to the database, will be rounded to the resolution used.</para>
        /// </remarks>
        /// <returns>The number of ticks in the instance.</returns>
        /// </summary>
        public long Ticks
        {
            get { return _ticks; }
        }

        /// <summary>
        /// Gets the number of whole microseconds held in the instance.
        /// <returns>An  in the range [-999999, 999999].</returns>
        /// </summary>
        public int Microseconds
        {
            get { return (int) ((_ticks/10)%1000000); }
        }

        /// <summary>
        /// Gets the number of whole milliseconds held in the instance.
        /// <returns>An  in the range [-999, 999].</returns>
        /// </summary>
        public int Milliseconds
        {
            get { return (int) ((_ticks/TicksPerMillsecond)%1000); }
        }

        /// <summary>
        /// Gets the number of whole seconds held in the instance.
        /// <returns>An  in the range [-59, 59].</returns>
        /// </summary>
        public int Seconds
        {
            get { return (int) ((_ticks/TicksPerSecond)%60); }
        }

        /// <summary>
        /// Gets the number of whole minutes held in the instance.
        /// <returns>An  in the range [-59, 59].</returns>
        /// </summary>
        public int Minutes
        {
            get { return (int) ((_ticks/TicksPerMinute)%60); }
        }

        /// <summary>
        /// Gets the number of whole hours held in the instance.
        /// <remarks>Note that this can be less than -23 or greater than 23 unless <see cref="JustifyDays()"/>
        /// has been used to produce this instance.</remarks>
        /// </summary>
        public int Hours
        {
            get { return (int) (_ticks/TicksPerHour); }
        }

        /// <summary>
        /// Gets the number of days held in the instance.
        /// <remarks>Note that this does not pay attention to a time component with -24 or less hours or
        /// 24 or more hours, unless <see cref="JustifyDays()"/> has been called to produce this instance.</remarks>
        /// </summary>
        public int Days
        {
            get { return _days; }
        }

        /// <summary>
        /// Gets the number of months held in the instance.
        /// <remarks>Note that this does not pay attention to a day component with -30 or less days or
        /// 30 or more days, unless <see cref="JustifyMonths()"/> has been called to produce this instance.</remarks>
        /// </summary>
        public int Months
        {
            get { return _months; }
        }

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> representing the time component of the instance.
        /// <remarks>Note that this may have a value beyond the range &#xb1;23:59:59.9999999 unless
        /// <see cref="JustifyDays()"/> has been called to produce this instance.</remarks>
        /// </summary>
        public TimeSpan Time
        {
            get { return new TimeSpan(_ticks); }
        }

        #endregion

        #region Total Parts

        /// <summary>
        /// The total number of ticks (100ns units) in the instance, assuming 24 hours in each day and
        /// 30 days in a month.
        /// </summary>
        public long TotalTicks
        {
            get { return Ticks + Days*TicksPerDay + Months*TicksPerMonth; }
        }

        /// <summary>
        /// The total number of microseconds in the instance, assuming 24 hours in each day and
        /// 30 days in a month.
        /// </summary>
        public double TotalMicroseconds
        {
            get { return TotalTicks/10d; }
        }

        /// <summary>
        /// The total number of milliseconds in the instance, assuming 24 hours in each day and
        /// 30 days in a month.
        /// </summary>
        public double TotalMilliseconds
        {
            get { return TotalTicks/(double) TicksPerMillsecond; }
        }

        /// <summary>
        /// The total number of seconds in the instance, assuming 24 hours in each day and
        /// 30 days in a month.
        /// </summary>
        public double TotalSeconds
        {
            get { return TotalTicks/(double) TicksPerSecond; }
        }

        /// <summary>
        /// The total number of minutes in the instance, assuming 24 hours in each day and
        /// 30 days in a month.
        /// </summary>
        public double TotalMinutes
        {
            get { return TotalTicks/(double) TicksPerMinute; }
        }

        /// <summary>
        /// The total number of hours in the instance, assuming 24 hours in each day and
        /// 30 days in a month.
        /// </summary>
        public double TotalHours
        {
            get { return TotalTicks/(double) TicksPerHour; }
        }

        /// <summary>
        /// The total number of days in the instance, assuming 24 hours in each day and
        /// 30 days in a month.
        /// </summary>
        public double TotalDays
        {
            get { return TotalTicks/(double) TicksPerDay; }
        }

        /// <summary>
        /// The total number of months in the instance, assuming 24 hours in each day and
        /// 30 days in a month.
        /// </summary>
        public double TotalMonths
        {
            get { return TotalTicks/(double) TicksPerMonth; }
        }

        #endregion

        #region Create From Part

        /// <summary>
        /// Creates an <see cref="EDBInterval"/> from a number of ticks.
        /// </summary>
        /// <param name="ticks">The number of ticks (100ns units) in the interval.</param>
        /// <returns>A <see cref="Canonicalize()"/>d <see cref="EDBInterval"/> with the given number of ticks.</returns>
        public static EDBInterval FromTicks(long ticks)
        {
            return new EDBInterval(ticks).Canonicalize();
        }

        /// <summary>
        /// Creates an <see cref="EDBInterval"/> from a number of microseconds.
        /// </summary>
        /// <param name="micro">The number of microseconds in the interval.</param>
        /// <returns>A <see cref="Canonicalize()"/>d <see cref="EDBInterval"/> with the given number of microseconds.</returns>
        public static EDBInterval FromMicroseconds(double micro)
        {
            return FromTicks((long) (micro*TicksPerMicrosecond));
        }

        /// <summary>
        /// Creates an <see cref="EDBInterval"/> from a number of milliseconds.
        /// </summary>
        /// <param name="milli">The number of milliseconds in the interval.</param>
        /// <returns>A <see cref="Canonicalize()"/>d <see cref="EDBInterval"/> with the given number of milliseconds.</returns>
        public static EDBInterval FromMilliseconds(double milli)
        {
            return FromTicks((long) (milli*TicksPerMillsecond));
        }

        /// <summary>
        /// Creates an <see cref="EDBInterval"/> from a number of seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds in the interval.</param>
        /// <returns>A <see cref="Canonicalize()"/>d <see cref="EDBInterval"/> with the given number of seconds.</returns>
        public static EDBInterval FromSeconds(double seconds)
        {
            return FromTicks((long) (seconds*TicksPerSecond));
        }

        /// <summary>
        /// Creates an <see cref="EDBInterval"/> from a number of minutes.
        /// </summary>
        /// <param name="minutes">The number of minutes in the interval.</param>
        /// <returns>A <see cref="Canonicalize()"/>d <see cref="EDBInterval"/> with the given number of minutes.</returns>
        public static EDBInterval FromMinutes(double minutes)
        {
            return FromTicks((long) (minutes*TicksPerMinute));
        }

        /// <summary>
        /// Creates an <see cref="EDBInterval"/> from a number of hours.
        /// </summary>
        /// <param name="hours">The number of hours in the interval.</param>
        /// <returns>A <see cref="Canonicalize()"/>d <see cref="EDBInterval"/> with the given number of hours.</returns>
        public static EDBInterval FromHours(double hours)
        {
            return FromTicks((long) (hours*TicksPerHour));
        }

        /// <summary>
        /// Creates an <see cref="EDBInterval"/> from a number of days.
        /// </summary>
        /// <param name="days">The number of days in the interval.</param>
        /// <returns>A <see cref="Canonicalize()"/>d <see cref="EDBInterval"/> with the given number of days.</returns>
        public static EDBInterval FromDays(double days)
        {
            return FromTicks((long) (days*TicksPerDay));
        }

        /// <summary>
        /// Creates an <see cref="EDBInterval"/> from a number of months.
        /// </summary>
        /// <param name="months">The number of months in the interval.</param>
        /// <returns>A <see cref="Canonicalize()"/>d <see cref="EDBInterval"/> with the given number of months.</returns>
        public static EDBInterval FromMonths(double months)
        {
            return FromTicks((long) (months*TicksPerMonth));
        }

        #endregion

        #region Arithmetic

        /// <summary>
        /// Adds another interval to this instance and returns the result.
        /// </summary>
        /// <param name="interval">An <see cref="EDBInterval"/> to add to this instance.</param>
        /// <returns>An <see cref="EDBInterval"></see> whose values are the sums of the two instances.</returns>
        public EDBInterval Add(EDBInterval interval)
        {
            return new EDBInterval(Months + interval.Months, Days + interval.Days, Ticks + interval.Ticks);
        }

        /// <summary>
        /// Subtracts another interval from this instance and returns the result.
        /// </summary>
        /// <param name="interval">An <see cref="EDBInterval"/> to subtract from this instance.</param>
        /// <returns>An <see cref="EDBInterval"></see> whose values are the differences of the two instances.</returns>
        public EDBInterval Subtract(EDBInterval interval)
        {
            return new EDBInterval(Months - interval.Months, Days - interval.Days, Ticks - interval.Ticks);
        }

        /// <summary>
        /// Returns an <see cref="EDBInterval"/> whose value is the negated value of this instance.
        /// </summary>
        /// <returns>An <see cref="EDBInterval"/> whose value is the negated value of this instance.</returns>
        public EDBInterval Negate()
        {
            return new EDBInterval(-Months, -Days, -Ticks);
        }

        /// <summary>
        /// This absolute value of this instance. In the case of some, but not all, components being negative,
        /// the rules used for justification are used to determine if the instance is positive or negative.
        /// </summary>
        /// <returns>An <see cref="EDBInterval"/> whose value is the absolute value of this instance.</returns>
        public EDBInterval Duration()
        {
            return UnjustifyInterval().Ticks < 0 ? Negate() : this;
        }

        #endregion

        #region Justification

        /// <summary>
        /// Equivalent to PostgreSQL's justify_days function.
        /// </summary>
        /// <returns>An <see cref="EDBInterval"/> based on this one, but with any hours outside of the range [-23, 23]
        /// converted into days.</returns>
        public EDBInterval JustifyDays()
        {
            return new EDBInterval(Months, Days + (int) (Ticks/TicksPerDay), Ticks%TicksPerDay);
        }

        /// <summary>
        /// Opposite to PostgreSQL's justify_days function.
        /// </summary>
        /// <returns>An <see cref="EDBInterval"/> based on this one, but with any days converted to multiples of &#xB1;24hours.</returns>
        public EDBInterval UnjustifyDays()
        {
            return new EDBInterval(Months, 0, Ticks + Days*TicksPerDay);
        }

        /// <summary>
        /// Equivalent to PostgreSQL's justify_months function.
        /// </summary>
        /// <returns>An <see cref="EDBInterval"/> based on this one, but with any days outside of the range [-30, 30]
        /// converted into months.</returns>
        public EDBInterval JustifyMonths()
        {
            return new EDBInterval(Months + Days/DaysPerMonth, Days%DaysPerMonth, Ticks);
        }

        /// <summary>
        /// Opposite to PostgreSQL's justify_months function.
        /// </summary>
        /// <returns>An <see cref="EDBInterval"/> based on this one, but with any months converted to multiples of &#xB1;30days.</returns>
        public EDBInterval UnjustifyMonths()
        {
            return new EDBInterval(0, Days + Months*DaysPerMonth, Ticks);
        }

        /// <summary>
        /// Equivalent to PostgreSQL's justify_interval function.
        /// </summary>
        /// <returns>An <see cref="EDBInterval"/> based on this one,
        /// but with any months converted to multiples of &#xB1;30days
        /// and then with any days converted to multiples of &#xB1;24hours</returns>
        public EDBInterval JustifyInterval()
        {
            return JustifyMonths().JustifyDays();
        }

        /// <summary>
        /// Opposite to PostgreSQL's justify_interval function.
        /// </summary>
        /// <returns>An <see cref="EDBInterval"/> based on this one, but with any months converted to multiples of &#xB1;30days and then any days converted to multiples of &#xB1;24hours;</returns>
        public EDBInterval UnjustifyInterval()
        {
            return new EDBInterval(Ticks + Days*TicksPerDay + Months*DaysPerMonth*TicksPerDay);
        }

        /// <summary>
        /// Produces a canonical NpgslInterval with 0 months and hours in the range of [-23, 23].
        /// <remarks>
        /// <para>
        /// While the fact that for many purposes, two different <see cref="EDBInterval"/> instances could be considered
        /// equivalent (e.g. one with 2days, 3hours and one with 1day 27hours) there are different possible canonical forms.
        /// </para><para>
        /// E.g. we could move all excess hours into days and all excess days into months and have the most readable form,
        /// or we could move everything into the ticks and have the form that allows for the easiest arithmetic) the form
        /// chosen has two important properties that make it the best choice.
        /// </para><para>First, it is closest two how
        /// <see cref="TimeSpan"/> objects are most often represented. Second, it is compatible with results of many
        /// PostgreSQL functions, particularly with age() and the results of subtracting one date, time or timestamp from
        /// another.
        /// </para>
        /// <para>Note that the results of casting a <see cref="TimeSpan"/> to <see cref="EDBInterval"/> is
        /// canonicalised.</para>
        /// </remarks>
        /// </summary>
        /// <returns>An <see cref="EDBTypes.EDBInterval"/> based on this one, but with months converted to multiples of &#xB1;30days and with any hours outside of the range [-23, 23]
        /// converted into days.</returns>
        public EDBInterval Canonicalize()
        {
            return new EDBInterval(0, Days + Months*DaysPerMonth + (int) (Ticks/TicksPerDay), Ticks%TicksPerDay);
        }

        #endregion

        #region Casts

        /// <summary>
        /// Implicit cast of a <see cref="TimeSpan"/> to an <see cref="EDBInterval"/>
        /// </summary>
        /// <param name="timespan">A <see cref="TimeSpan"/></param>
        /// <returns>An eqivalent, canonical, <see cref="EDBInterval"/>.</returns>
        public static implicit operator EDBInterval(TimeSpan timespan)
        {
            return new EDBInterval(timespan).Canonicalize();
        }

        /// <summary>
        /// Implicit cast of an <see cref="EDBInterval"/> to a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="interval">A <see cref="EDBInterval"/>.</param>
        /// <returns>An equivalent <see cref="TimeSpan"/>.</returns>
        public static explicit operator TimeSpan(EDBInterval interval)
        {
            return new TimeSpan(interval.Ticks + interval.Days*TicksPerDay + interval.Months*DaysPerMonth*TicksPerDay);
        }

        #endregion

        #region Comparison

        /// <summary>
        /// Returns true if another <see cref="EDBInterval"/> is exactly the same as this instance.
        /// </summary>
        /// <param name="other">An <see cref="EDBInterval"/> for comparison.</param>
        /// <returns>true if the two <see cref="EDBInterval"/> instances are exactly the same,
        /// false otherwise.</returns>
        public bool Equals(EDBInterval other)
        {
            return Ticks == other.Ticks && Days == other.Days && Months == other.Months;
        }

        /// <summary>
        /// Returns true if another object is an <see cref="EDBInterval"/>, that is exactly the same as
        /// this instance
        /// </summary>
        /// <param name="obj">An <see cref="Object"/> for comparison.</param>
        /// <returns>true if the argument is an <see cref="EDBInterval"/> and is exactly the same
        /// as this one, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is EDBInterval)
            {
                return Equals((EDBInterval) obj);
            }
            return false;
        }

        /// <summary>
        /// Compares two <see cref="EDBInterval"/> instances.
        /// </summary>
        /// <param name="x">The first <see cref="EDBInterval"/>.</param>
        /// <param name="y">The second <see cref="EDBInterval"/>.</param>
        /// <returns>0 if the two are equal or equivalent. A value greater than zero if x is greater than y,
        /// a value less than zero if x is less than y.</returns>
        public static int Compare(EDBInterval x, EDBInterval y)
        {
            return x.CompareTo(y);
        }

        int IComparer<EDBInterval>.Compare(EDBInterval x, EDBInterval y)
        {
            return x.CompareTo(y);
        }

        int IComparer.Compare(object x, object y)
        {
            if (x == null)
            {
                return y == null ? 0 : 1;
            }
            if (y == null)
            {
                return -1;
            }
            try
            {
                return ((IComparable) x).CompareTo(y);
            }
            catch (Exception)
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// A hash code suitable for uses with hashing algorithms.
        /// </summary>
        /// <returns>An signed integer.</returns>
        public override int GetHashCode()
        {
            return UnjustifyInterval().Ticks.GetHashCode();
        }

        /// <summary>
        /// Compares this instance with another/
        /// </summary>
        /// <param name="other">An <see cref="EDBInterval"/> to compare this with.</param>
        /// <returns>0 if the instances are equal or equivalent. A value less than zero if
        /// this instance is less than the argument. A value greater than zero if this instance
        /// is greater than the instance.</returns>
        public int CompareTo(EDBInterval other)
        {
            return UnjustifyInterval().Ticks.CompareTo(other.UnjustifyInterval().Ticks);
        }

        /// <summary>
        /// Compares this instance with another/
        /// </summary>
        /// <param name="other">An object to compare this with.</param>
        /// <returns>0 if the argument is an <see cref="EDBInterval"/> and the instances are equal or equivalent.
        /// A value less than zero if the argument is an <see cref="EDBInterval"/> and
        /// this instance is less than the argument.
        /// A value greater than zero if the argument is an <see cref="EDBInterval"/> and this instance
        /// is greater than the instance.</returns>
        /// A value greater than zero if the argument is null.
        /// <exception cref="ArgumentException">The argument is not an <see cref="EDBInterval"/>.</exception>
        public int CompareTo(object other)
        {
            if (other == null)
            {
                return 1;
            }
            else if (other is EDBInterval)
            {
                return CompareTo((EDBInterval) other);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        #endregion

        #region To And From Strings

        /// <summary>
        /// Parses a <see cref="String"/> and returns a <see cref="EDBInterval"/> instance.
        /// Designed to use the formats generally returned by PostgreSQL.
        /// </summary>
        /// <param name="str">The <see cref="String"/> to parse.</param>
        /// <returns>An <see cref="EDBInterval"/> represented by the argument.</returns>
        /// <exception cref="ArgumentNullException">The string was null.</exception>
        /// <exception cref="OverflowException">A value obtained from parsing the string exceeded the values allowed for the relevant component.</exception>
        /// <exception cref="FormatException">The string was not in a format that could be parsed to produce an <see cref="EDBInterval"/>.</exception>
        public static EDBInterval Parse(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            str = str.Replace('s', ' '); //Quick and easy way to catch plurals.
            try
            {
                int years = 0;
                int months = 0;
                int days = 0;
                int hours = 0;
                int minutes = 0;
                decimal seconds = 0m;
                int idx = str.IndexOf("year");
                if (idx > 0)
                {
                    years = int.Parse(str.Substring(0, idx));
                    str = SafeSubstring(str, idx + 5);
                }
                idx = str.IndexOf("mon");
                if (idx > 0)
                {
                    months = int.Parse(str.Substring(0, idx));
                    str = SafeSubstring(str, idx + 4);
                }
                idx = str.IndexOf("day");
                if (idx > 0)
                {
                    days = int.Parse(str.Substring(0, idx));
                    str = SafeSubstring(str, idx + 4).Trim();
                }
                if (str.Length > 0)
                {
                    bool isNegative = str[0] == '-';
                    string[] parts = str.Split(':');
                    switch (parts.Length) //One of those times that fall-through would actually be good.
                    {
                        case 1:
                            hours = int.Parse(parts[0]);
                            break;
                        case 2:
                            hours = int.Parse(parts[0]);
                            minutes = int.Parse(parts[1]);
                            break;
                        default:
                            hours = int.Parse(parts[0]);
                            minutes = int.Parse(parts[1]);
                            seconds = decimal.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            break;
                    }
                    if (isNegative)
                    {
                        minutes *= -1;
                        seconds *= -1;
                    }
                }
                long ticks = hours*TicksPerHour + minutes*TicksPerMinute + (long) (seconds*TicksPerSecond);
                return new EDBInterval(years*MonthsPerYear + months, days, ticks);
            }
            catch (OverflowException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }

        private static string SafeSubstring(string s, int startIndex)
        {
            if (startIndex >= s.Length)
                return string.Empty;
            else
                return s.Substring(startIndex);
        }

        /// <summary>
        /// Attempt to parse a <see cref="String"/> to produce an <see cref="EDBInterval"/>.
        /// </summary>
        /// <param name="str">The <see cref="String"/> to parse.</param>
        /// <param name="result">(out) The <see cref="EDBInterval"/> produced, or <see cref="Zero"/> if the parsing failed.</param>
        /// <returns>true if the parsing succeeded, false otherwise.</returns>
        public static bool TryParse(string str, out EDBInterval result)
        {
            try
            {
                result = Parse(str);
                return true;
            }
            catch (Exception)
            {
                result = Zero;
                return false;
            }
        }

        /// <summary>
        /// Create a <see cref="String"/> representation of the <see cref="EDBInterval"/> instance.
        /// The format returned is of the form:
        /// [M mon[s]] [d day[s]] [HH:mm:ss[.f[f[f[f[f[f[f[f[f]]]]]]]]]]
        /// A zero <see cref="EDBInterval"/> is represented as 00:00:00
        /// <remarks>
        /// Ticks are 100ns, Postgress resolution is only to 1&#xb5;s at most. Hence we lose 1 or more decimal
        /// precision in storing values in the database. Despite this, this method will output that extra
        /// digit of precision. It's forward-compatible with any future increases in resolution up to 100ns,
        /// and also makes this ToString() more applicable to any other use-case.
        /// </remarks>
        /// </summary>
        /// <returns>The <see cref="String"/> representation.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Months != 0)
            {
                sb.Append(Months).Append(Math.Abs(Months) == 1 ? " mon " : " mons ");
            }
            if (Days != 0)
            {
                if (Months < 0 && Days > 0)
                {
                    sb.Append('+');
                }
                sb.Append(Days).Append(Math.Abs(Days) == 1 ? " day " : " days ");
            }
            if (Ticks != 0 || sb.Length == 0)
            {
                if(Ticks < 0)
                {
                    sb.Append('-');
                }
                else if (Days < 0 || (Days == 0 && Months < 0))
                {
                    sb.Append('+');
                }
                // calculate total seconds and then subtract total whole minutes in seconds to get just the seconds and fractional part
                decimal seconds = _ticks / (decimal)TicksPerSecond - (_ticks / TicksPerMinute) * 60;
                sb.Append(Math.Abs(Hours).ToString("D2")).Append(':').Append(Math.Abs(Minutes).ToString("D2")).Append(':').Append(Math.Abs(seconds).ToString("0#.######", System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

            }
            if (sb[sb.Length - 1] == ' ')
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        #endregion

        #region Common Operators

        /// <summary>
        /// Adds two <see cref="EDBInterval"/> together.
        /// </summary>
        /// <param name="x">The first <see cref="EDBInterval"/> to add.</param>
        /// <param name="y">The second <see cref="EDBInterval"/> to add.</param>
        /// <returns>An <see cref="EDBInterval"/> whose values are the sum of the arguments.</returns>
        public static EDBInterval operator +(EDBInterval x, EDBInterval y)
        {
            return x.Add(y);
        }

        /// <summary>
        /// Subtracts one <see cref="EDBInterval"/> from another.
        /// </summary>
        /// <param name="x">The <see cref="EDBInterval"/> to subtract the other from.</param>
        /// <param name="y">The <see cref="EDBInterval"/> to subtract from the other.</param>
        /// <returns>An <see cref="EDBInterval"/> whose values are the difference of the arguments</returns>
        public static EDBInterval operator -(EDBInterval x, EDBInterval y)
        {
            return x.Subtract(y);
        }

        /// <summary>
        /// Returns true if two <see cref="EDBInterval"/> are exactly the same.
        /// </summary>
        /// <param name="x">The first <see cref="EDBInterval"/> to compare.</param>
        /// <param name="y">The second <see cref="EDBInterval"/> to compare.</param>
        /// <returns>true if the two arguments are exactly the same, false otherwise.</returns>
        public static bool operator ==(EDBInterval x, EDBInterval y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Returns false if two <see cref="EDBInterval"/> are exactly the same.
        /// </summary>
        /// <param name="x">The first <see cref="EDBInterval"/> to compare.</param>
        /// <param name="y">The second <see cref="EDBInterval"/> to compare.</param>
        /// <returns>false if the two arguments are exactly the same, true otherwise.</returns>
        public static bool operator !=(EDBInterval x, EDBInterval y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Compares two <see cref="EDBInterval"/> instances to see if the first is less than the second
        /// </summary>
        /// <param name="x">The first <see cref="EDBInterval"/> to compare.</param>
        /// <param name="y">The second <see cref="EDBInterval"/> to compare.</param>
        /// <returns>true if the first <see cref="EDBInterval"/> is less than second, false otherwise.</returns>
        public static bool operator <(EDBInterval x, EDBInterval y)
        {
            return x.UnjustifyInterval().Ticks < y.UnjustifyInterval().Ticks;
        }

        /// <summary>
        /// Compares two <see cref="EDBInterval"/> instances to see if the first is less than or equivalent to the second
        /// </summary>
        /// <param name="x">The first <see cref="EDBInterval"/> to compare.</param>
        /// <param name="y">The second <see cref="EDBInterval"/> to compare.</param>
        /// <returns>true if the first <see cref="EDBInterval"/> is less than or equivalent to second, false otherwise.</returns>
        public static bool operator <=(EDBInterval x, EDBInterval y)
        {
            return x.UnjustifyInterval().Ticks <= y.UnjustifyInterval().Ticks;
        }

        /// <summary>
        /// Compares two <see cref="EDBInterval"/> instances to see if the first is greater than the second
        /// </summary>
        /// <param name="x">The first <see cref="EDBInterval"/> to compare.</param>
        /// <param name="y">The second <see cref="EDBInterval"/> to compare.</param>
        /// <returns>true if the first <see cref="EDBInterval"/> is greater than second, false otherwise.</returns>
        public static bool operator >(EDBInterval x, EDBInterval y)
        {
            return !(x <= y);
        }

        /// <summary>
        /// Compares two <see cref="EDBInterval"/> instances to see if the first is greater than or equivalent the second
        /// </summary>
        /// <param name="x">The first <see cref="EDBInterval"/> to compare.</param>
        /// <param name="y">The second <see cref="EDBInterval"/> to compare.</param>
        /// <returns>true if the first <see cref="EDBInterval"/> is greater than or equivalent to the second, false otherwise.</returns>
        public static bool operator >=(EDBInterval x, EDBInterval y)
        {
            return !(x < y);
        }

        /// <summary>
        /// Returns the instance.
        /// </summary>
        /// <param name="x">An <see cref="EDBInterval"/>.</param>
        /// <returns>The argument.</returns>
        public static EDBInterval operator +(EDBInterval x)
        {
            return x;
        }

        /// <summary>
        /// Negates an <see cref="EDBInterval"/> instance.
        /// </summary>
        /// <param name="x">An <see cref="EDBInterval"/>.</param>
        /// <returns>The negation of the argument.</returns>
        public static EDBInterval operator -(EDBInterval x)
        {
            return x.Negate();
        }

        #endregion
    }

    [Serializable]
    public struct EDBDate : IEquatable<EDBDate>, IComparable<EDBDate>, IComparable, IComparer<EDBDate>,
                               IComparer
    {
        private static readonly int[] CommonYearDays = new int[] {0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365};
        private static readonly int[] LeapYearDays = new int[] {0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366};
        private static readonly int[] CommonYearMaxes = new int[] {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
        private static readonly int[] LeapYearMaxes = new int[] {31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
        public const int MaxYear = 5874897;
        public const int MinYear = -4714;
        public static readonly EDBDate Epoch = new EDBDate(1970, 1, 1);
        public static readonly EDBDate MaxCalculableValue = new EDBDate(MaxYear, 12, 31);
        public static readonly EDBDate MinCalculableValue = new EDBDate(MinYear, 11, 24);
        public static readonly EDBDate Era = new EDBDate(0);

        public static EDBDate Now
        {
            get { return new EDBDate(DateTime.Now); }
        }

        public static EDBDate Today
        {
            get { return Now; }
        }

        public static EDBDate Yesterday
        {
            get { return Now.AddDays(-1); }
        }

        public static EDBDate Tomorrow
        {
            get { return Now.AddDays(1); }
        }

        public static EDBDate Parse(string str)
        {

            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            // Handle -infinity and infinity special values.

            if (str == "-infinity")
                return new EDBDate(DateTime.MinValue);

            if (str == "infinity")
                return new EDBDate(DateTime.MaxValue);

            str = str.Trim();
            try
            {
                int idx = str.IndexOf('-');
                if (idx == -1)
                {
                    throw new FormatException();
                }
                int year = int.Parse(str.Substring(0, idx));
                int idxLast = idx + 1;
                if ((idx = str.IndexOf('-', idxLast)) == -1)
                {
                    throw new FormatException();
                }
                int month = int.Parse(str.Substring(idxLast, idx - idxLast));
                idxLast = idx + 1;
                if ((idx = str.IndexOf(' ', idxLast)) == -1)
                {
                    idx = str.Length;
                }
                int day = int.Parse(str.Substring(idxLast, idx - idxLast));
                if (str.Contains("BC"))
                {
                    year = -year;
                }
                return new EDBDate(year, month, day);
            }
            catch (OverflowException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }

        public static bool TryParse(string str, out EDBDate date)
        {
            try
            {
                date = Parse(str);
                return true;
            }
            catch
            {
                date = Era;
                return false;
            }
        }

        //Number of days since January 1st CE (January 1st EV). 1 Jan 1 CE = 0, 2 Jan 1 CE = 1, 31 Dec 1 BCE = -1, etc.
        private readonly int _daysSinceEra;

        public EDBDate(int days)
        {
            _daysSinceEra = days;
        }

        public EDBDate(DateTime dateTime)
            : this((int) (dateTime.Ticks/TimeSpan.TicksPerDay))
        {
        }

        public EDBDate(EDBDate copyFrom)
            : this(copyFrom._daysSinceEra)
        {
        }

        public EDBDate(int year, int month, int day)
        {
            if (year == 0 || year < MinYear || year > MaxYear || month < 1 || month > 12 || day < 1 ||
                (day > (IsLeap(year) ? 366 : 365)))
            {
                throw new ArgumentOutOfRangeException();
            }

            _daysSinceEra = DaysForYears(year) + (IsLeap(year) ? LeapYearDays : CommonYearDays)[month - 1] + day - 1;
        }

        private const int DaysInYear = 365; //Common years
        private const int DaysIn4Years = 4*DaysInYear + 1; //Leap year every 4 years.
        private const int DaysInCentury = 25*DaysIn4Years - 1; //Except no leap year every 100.
        private const int DaysIn4Centuries = 4*DaysInCentury + 1; //Except leap year every 400.

        private static int DaysForYears(int years)
        {
            //Number of years after 1CE (0 for 1CE, -1 for 1BCE, 1 for 2CE).
            int calcYear = years < 1 ? years : years - 1;

            return calcYear/400*DaysIn4Centuries //Blocks of 400 years with their leap and common years
                   + calcYear%400/100*DaysInCentury //Remaining blocks of 100 years with their leap and common years
                   + calcYear%100/4*DaysIn4Years //Remaining blocks of 4 years with their leap and common years
                   + calcYear%4*DaysInYear //Remaining years, all common
                   + (calcYear < 0 ? -1 : 0); //And 1BCE is leap.
        }

        public int DayOfYear
        {
            get { return _daysSinceEra - DaysForYears(Year) + 1; }
        }

        public int Year
        {
            get
            {
                int guess = (int) Math.Round(_daysSinceEra/365.2425);
                int test = guess - 1;
                while (DaysForYears(++test) <= _daysSinceEra)
                {
                    ;
                }
                return test - 1;
            }
        }

        public int Month
        {
            get
            {
                int i = 1;
                int target = DayOfYear;
                int[] array = IsLeapYear ? LeapYearDays : CommonYearDays;
                while (target > array[i])
                {
                    ++i;
                }
                return i;
            }
        }

        public int Day
        {
            get { return DayOfYear - (IsLeapYear ? LeapYearDays : CommonYearDays)[Month - 1]; }
        }

        public DayOfWeek DayOfWeek
        {
            get { return (DayOfWeek) ((_daysSinceEra + 1)%7); }
        }

        internal int DaysSinceEra
        {
            get { return _daysSinceEra; }
        }

        public bool IsLeapYear
        {
            get { return IsLeap(Year); }
        }

        private static bool IsLeap(int year)
        {
            //Every 4 years is a leap year
            //Except every 100 years isn't a leap year.
            //Except every 400 years is.
            if (year < 1)
            {
                year = year + 1;
            }
            return (year%4 == 0) && ((year%100 != 0) || (year%400 == 0));
        }

        public EDBDate AddDays(int days)
        {
            return new EDBDate(_daysSinceEra + days);
        }

        public EDBDate AddYears(int years)
        {
            int newYear = Year + years;
            if (newYear >= 0 && _daysSinceEra < 0) //cross 1CE/1BCE divide going up
            {
                ++newYear;
            }
            else if (newYear <= 0 && _daysSinceEra >= 0) //cross 1CE/1BCE divide going down
            {
                --newYear;
            }
            return new EDBDate(newYear, Month, Day);
        }

        public EDBDate AddMonths(int months)
        {
            int newYear = Year;
            int newMonth = Month + months;

            while (newMonth > 12)
            {
                newMonth -= 12;
                newYear += 1;
            };
            while (newMonth < 1)
            {
                newMonth += 12;
                newYear -= 1;
            };
            int maxDay = (IsLeap(newYear) ? LeapYearMaxes : CommonYearMaxes)[newMonth - 1];
            int newDay = Day > maxDay ? maxDay : Day;
            return new EDBDate(newYear, newMonth, newDay);

        }

        public EDBDate Add(EDBInterval interval)
        {
            return AddMonths(interval.Months).AddDays(interval.Days);
        }

        internal EDBDate Add(EDBInterval interval, int carriedOverflow)
        {
            return AddMonths(interval.Months).AddDays(interval.Days + carriedOverflow);
        }

        public int Compare(EDBDate x, EDBDate y)
        {
            return x.CompareTo(y);
        }

        public int Compare(object x, object y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }
            if (y == null)
            {
                return 1;
            }
            if (!(x is IComparable) || !(y is IComparable))
            {
                throw new ArgumentException();
            }
            return ((IComparable) x).CompareTo(y);
        }

        public bool Equals(EDBDate other)
        {
            return _daysSinceEra == other._daysSinceEra;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is EDBDate && Equals((EDBDate) obj);
        }

        public int CompareTo(EDBDate other)
        {
            return _daysSinceEra.CompareTo(other._daysSinceEra);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is EDBDate)
            {
                return CompareTo((EDBDate) obj);
            }
            throw new ArgumentException();
        }

        public override int GetHashCode()
        {
            return _daysSinceEra;
        }

        public override string ToString()
        {
            //Format of yyyy-MM-dd with " BC" for BCE and optional " AD" for CE which we omit here.
            return
                new StringBuilder(Math.Abs(Year).ToString("D4")).Append('-').Append(Month.ToString("D2")).Append('-').Append(
                    Day.ToString("D2")).Append(_daysSinceEra < 0 ? " BC" : "").ToString();
        }

        public static bool operator ==(EDBDate x, EDBDate y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBDate x, EDBDate y)
        {
            return !(x == y);
        }

        public static bool operator <(EDBDate x, EDBDate y)
        {
            return x._daysSinceEra < y._daysSinceEra;
        }

        public static bool operator >(EDBDate x, EDBDate y)
        {
            return x._daysSinceEra > y._daysSinceEra;
        }

        public static bool operator <=(EDBDate x, EDBDate y)
        {
            return x._daysSinceEra <= y._daysSinceEra;
        }

        public static bool operator >=(EDBDate x, EDBDate y)
        {
            return x._daysSinceEra >= y._daysSinceEra;
        }

        public static explicit operator DateTime(EDBDate date)
        {
            try
            {
                return new DateTime(date._daysSinceEra*EDBInterval.TicksPerDay);
            }
            catch
            {
                throw new InvalidCastException();
            }
        }

        public static explicit operator EDBDate(DateTime date)
        {
            return new EDBDate((int) (date.Ticks/EDBInterval.TicksPerDay));
        }

        public static EDBDate operator +(EDBDate date, EDBInterval interval)
        {
            return date.Add(interval);
        }

        public static EDBDate operator +(EDBInterval interval, EDBDate date)
        {
            return date.Add(interval);
        }

        public static EDBDate operator -(EDBDate date, EDBInterval interval)
        {
            return date.Add(-interval);
        }

        public static EDBInterval operator -(EDBDate dateX, EDBDate dateY)
        {
            return new EDBInterval(0, dateX._daysSinceEra - dateY._daysSinceEra, 0);
        }
    }

    [Serializable]
    public struct EDBTimeZone : IEquatable<EDBTimeZone>, IComparable<EDBTimeZone>, IComparable
    {
        public static EDBTimeZone UTC = new EDBTimeZone(0);
        private readonly int _totalSeconds;

        public EDBTimeZone(TimeSpan ts)
            : this(ts.Ticks)
        {
        }

        private EDBTimeZone(long ticks)
        {
            _totalSeconds = (int) (ticks/EDBInterval.TicksPerSecond);
        }

        public EDBTimeZone(EDBInterval ni)
            : this(ni.Ticks)
        {
        }

        public EDBTimeZone(EDBTimeZone copyFrom)
        {
            _totalSeconds = copyFrom._totalSeconds;
        }

        public EDBTimeZone(int hours, int minutes)
            : this(hours, minutes, 0)
        {
        }

        public EDBTimeZone(int hours, int minutes, int seconds)
        {
            _totalSeconds = hours*60*60 + minutes*60 + seconds;
        }

        public static implicit operator EDBTimeZone(EDBInterval interval)
        {
            return new EDBTimeZone(interval);
        }

        public static implicit operator EDBInterval(EDBTimeZone timeZone)
        {
            return new EDBInterval(timeZone._totalSeconds*EDBInterval.TicksPerSecond);
        }

        public static implicit operator EDBTimeZone(TimeSpan interval)
        {
            return new EDBTimeZone(interval);
        }

        public static implicit operator TimeSpan(EDBTimeZone timeZone)
        {
            return new TimeSpan(timeZone._totalSeconds*EDBInterval.TicksPerSecond);
        }

        public static EDBTimeZone SolarTimeZone(decimal longitude)
        {
            return new EDBTimeZone((long) (longitude/15m*EDBInterval.TicksPerHour));
        }

        public int Hours
        {
            get { return _totalSeconds/60/60; }
        }

        public int Minutes
        {
            get { return (_totalSeconds/60)%60; }
        }

        public int Seconds
        {
            get { return _totalSeconds%60; }
        }

        public static EDBTimeZone CurrentTimeZone
        {
            get { return new EDBTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)); }
        }

        public static EDBTimeZone LocalTimeZone(EDBDate date)
        {
            DateTime dt;
            if (date.Year >= 1902 && date.Year <= 2038)
            {
                dt = (DateTime) date;
            }
            else
            {
                dt = new DateTime(2000, date.Month, date.Day);
            }
            return new EDBTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(dt));
        }

        public bool Equals(EDBTimeZone other)
        {
            return _totalSeconds == other._totalSeconds;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is EDBTimeZone && Equals((EDBTimeZone) obj);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(_totalSeconds < 0 ? "-" : "+").Append(Math.Abs(Hours).ToString("D2"));
            if (Minutes != 0 || Seconds != 0)
            {
                sb.Append(':').Append(Math.Abs(Minutes).ToString("D2"));
                if (Seconds != 0)
                {
                    sb.Append(":").Append(Math.Abs(Seconds).ToString("D2"));
                }
            }
            return sb.ToString();
        }

        public static EDBTimeZone Parse(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException();
            }
            try
            {
                str = str.Trim();
                bool neg;
                switch (str[0])
                {
                    case '+':
                        neg = false;
                        break;
                    case '-':
                        neg = true;
                        break;
                    default:
                        throw new FormatException();
                }
                int hours;
                int minutes;
                int seconds;
                string[] parts = str.Substring(1).Split(':');
                switch (parts.Length) //One of those times that fall-through would actually be good.
                {
                    case 1:
                        hours = int.Parse(parts[0]);
                        minutes = seconds = 0;
                        break;
                    case 2:
                        hours = int.Parse(parts[0]);
                        minutes = int.Parse(parts[1]);
                        seconds = 0;
                        break;
                    default:
                        hours = int.Parse(parts[0]);
                        minutes = int.Parse(parts[1]);
                        seconds = int.Parse(parts[2]);
                        break;
                }
                int totalSeconds = (hours*60*60 + minutes*60 + seconds)*(neg ? -1 : 1);
                return new EDBTimeZone(totalSeconds*EDBInterval.TicksPerSecond);
            }
            catch (OverflowException)
            {
                throw;
            }
            catch
            {
                throw new FormatException();
            }
        }

        public static bool TryParse(string str, EDBTimeZone tz)
        {
            try
            {
                tz = Parse(str);
                return true;
            }
            catch
            {
                tz = UTC;
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _totalSeconds;
        }

        //Note, +01:00 is less than -01:00
        public int CompareTo(EDBTimeZone other)
        {
            return -(_totalSeconds.CompareTo(other._totalSeconds));
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is EDBTimeZone)
            {
                return CompareTo((EDBTimeZone) obj);
            }
            throw new ArgumentException();
        }

        public static EDBTimeZone operator -(EDBTimeZone tz)
        {
            return new EDBTimeZone(-tz._totalSeconds);
        }

        public static EDBTimeZone operator +(EDBTimeZone tz)
        {
            return tz;
        }

        public static bool operator ==(EDBTimeZone x, EDBTimeZone y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBTimeZone x, EDBTimeZone y)
        {
            return !(x == y);
        }

        public static bool operator <(EDBTimeZone x, EDBTimeZone y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator <=(EDBTimeZone x, EDBTimeZone y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >(EDBTimeZone x, EDBTimeZone y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator >=(EDBTimeZone x, EDBTimeZone y)
        {
            return x.CompareTo(y) >= 0;
        }
    }

    [Serializable]
    public struct EDBTime : IEquatable<EDBTime>, IComparable<EDBTime>, IComparable, IComparer<EDBTime>,
                               IComparer
    {
        public static readonly EDBTime AllBalls = new EDBTime(0);

        public static EDBTime Now
        {
            get { return new EDBTime(DateTime.Now.TimeOfDay); }
        }

        private readonly long _ticks;

        public EDBTime(long ticks)
        {
            if (ticks == EDBInterval.TicksPerDay)
            {
                _ticks = ticks;
            }
            else
            {
                ticks %= EDBInterval.TicksPerDay;
                _ticks = ticks < 0 ? ticks + EDBInterval.TicksPerDay : ticks;
            }
        }

        public EDBTime(TimeSpan time)
            : this(time.Ticks)
        {
        }

        public EDBTime(EDBInterval time)
            : this(time.Ticks)
        {
        }

        public EDBTime(EDBTime copyFrom)
            : this(copyFrom.Ticks)
        {
        }

        public EDBTime(int hours, int minutes, int seconds)
            : this(hours, minutes, seconds, 0)
        {
        }

        public EDBTime(int hours, int minutes, int seconds, int microseconds)
            : this(
                hours*EDBInterval.TicksPerHour + minutes*EDBInterval.TicksPerMinute + seconds*EDBInterval.TicksPerSecond +
                microseconds*EDBInterval.TicksPerMicrosecond)
        {
        }

        public EDBTime(int hours, int minutes, decimal seconds)
            : this(
                hours*EDBInterval.TicksPerHour + minutes*EDBInterval.TicksPerMinute +
                (long) (seconds*EDBInterval.TicksPerSecond))
        {
        }

        public EDBTime(int hours, int minutes, double seconds)
            : this(hours, minutes, (decimal) seconds)
        {
        }

        /// <summary>
        /// The total number of ticks(100ns units) contained. This is the resolution of the
        /// <see cref="EDBTime"/>  type.
        /// <remarks>The resolution of the PostgreSQL
        /// interval type is by default 1&#xb5;s = 1,000 ns. It may be smaller as follows:
        /// <list type="number">
        /// <item>
        /// <term>time(0)</term>
        /// <description>resolution of 1s (1 second)</description>
        /// </item>
        /// <item>
        /// <term>time(1)</term>
        /// <description>resolution of 100ms = 0.1s (100 milliseconds)</description>
        /// </item>
        /// <item>
        /// <term>time(2)</term>
        /// <description>resolution of 10ms = 0.01s (10 milliseconds)</description>
        /// </item>
        /// <item>
        /// <term>time(3)</term>
        /// <description>resolution of 1ms = 0.001s (1 millisecond)</description>
        /// </item>
        /// <item>
        /// <term>time(4)</term>
        /// <description>resolution of 100&#xb5;s = 0.0001s (100 microseconds)</description>
        /// </item>
        /// <item>
        /// <term>time(5)</term>
        /// <description>resolution of 10&#xb5;s = 0.00001s (10 microseconds)</description>
        /// </item>
        /// <item>
        /// <term>time(6) or interval</term>
        /// <description>resolution of 1&#xb5;s = 0.000001s (1 microsecond)</description>
        /// </item>
        /// </list>
        /// <para>As such, if the 100-nanosecond resolution is significant to an application, a PostgreSQL time will
        /// not suffice for those purposes.</para>
        /// <para>In more frequent cases though, the resolution of time suffices.
        /// <see cref="EDBTime"/> will always suffice to handle the resolution of any time value, and upon
        /// writing to the database, will be rounded to the resolution used.</para>
        /// </remarks>
        /// <returns>The number of ticks in the instance.</returns>
        /// </summary>
        public long Ticks
        {
            get { return _ticks; }
        }

        /// <summary>
        /// Gets the number of whole microseconds held in the instance.
        /// <returns>An integer in the range [0, 999999].</returns>
        /// </summary>
        public int Microseconds
        {
            get { return (int) ((_ticks/10)%1000000); }
        }

        /// <summary>
        /// Gets the number of whole milliseconds held in the instance.
        /// <returns>An integer in the range [0, 999].</returns>
        /// </summary>
        public int Milliseconds
        {
            get { return (int) ((_ticks/EDBInterval.TicksPerMillsecond)%1000); }
        }

        /// <summary>
        /// Gets the number of whole seconds held in the instance.
        /// <returns>An interger in the range [0, 59].</returns>
        /// </summary>
        public int Seconds
        {
            get { return (int) ((_ticks/EDBInterval.TicksPerSecond)%60); }
        }

        /// <summary>
        /// Gets the number of whole minutes held in the instance.
        /// <returns>An integer in the range [0, 59].</returns>
        /// </summary>
        public int Minutes
        {
            get { return (int) ((_ticks/EDBInterval.TicksPerMinute)%60); }
        }

        /// <summary>
        /// Gets the number of whole hours held in the instance.
        /// <remarks>Note that the time 24:00:00 can be stored for roundtrip compatibility. Any calculations on such a
        /// value will normalised it to 00:00:00.</remarks>
        /// </summary>
        public int Hours
        {
            get { return (int) (_ticks/EDBInterval.TicksPerHour); }
        }

        /// <summary>
        /// Normalise this time; if it is 24:00:00, convert it to 00:00:00
        /// </summary>
        /// <returns>This time, normalised</returns>
        public EDBTime Normalize()
        {
            return new EDBTime(_ticks%EDBInterval.TicksPerDay);
        }

        public bool Equals(EDBTime other)
        {
            return Ticks == other.Ticks;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is EDBTime && Equals((EDBTime) obj);
        }

        public override int GetHashCode()
        {
            return Ticks.GetHashCode();
        }

        public override string ToString()
        {
            // calculate total seconds and then subtract total whole minutes in seconds to get just the seconds and fractional part
            decimal seconds = _ticks / (decimal)EDBInterval.TicksPerSecond - (_ticks / EDBInterval.TicksPerMinute) * 60;
            StringBuilder sb =
                new StringBuilder(Hours.ToString("D2")).Append(':').Append(Minutes.ToString("D2")).Append(':').Append(
                    seconds.ToString("0#.######", System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            return sb.ToString();
        }

        public static EDBTime Parse(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException();
            }
            try
            {
                int hours = 0;
                int minutes = 0;
                decimal seconds = 0m;
                string[] parts = str.Split(':');
                switch (parts.Length) //One of those times that fall-through would actually be good.
                {
                    case 1:
                        hours = int.Parse(parts[0]);
                        break;
                    case 2:
                        hours = int.Parse(parts[0]);
                        minutes = int.Parse(parts[1]);
                        break;
                    default:
                        hours = int.Parse(parts[0]);
                        minutes = int.Parse(parts[1]);
                        seconds = decimal.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        break;
                }
                if (hours < 0 || hours > 24 || minutes < 0 || minutes > 59 || seconds < 0m || seconds >= 60 ||
                    (hours == 24 && (minutes != 0 || seconds != 0m)))
                {
                    throw new OverflowException();
                }
                return new EDBTime(hours, minutes, seconds);
            }
            catch (OverflowException)
            {
                throw;
            }
            catch
            {
                throw new FormatException();
            }
        }

        public static bool TryParse(string str, out EDBTime time)
        {
            try
            {
                time = Parse(str);
                return true;
            }
            catch
            {
                time = AllBalls;
                return false;
            }
        }

        public int CompareTo(EDBTime other)
        {
            return Ticks.CompareTo(other.Ticks);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is EDBTime)
            {
                return CompareTo((EDBTime) obj);
            }
            throw new ArgumentException();
        }

        public int Compare(EDBTime x, EDBTime y)
        {
            return x.CompareTo(y);
        }

        public int Compare(object x, object y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }
            if (y == null)
            {
                return 1;
            }
            if (!(x is IComparable) || !(y is IComparable))
            {
                throw new ArgumentException();
            }
            return ((IComparable) x).CompareTo(y);
        }

        public static bool operator ==(EDBTime x, EDBTime y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBTime x, EDBTime y)
        {
            return !(x == y);
        }

        public static bool operator <(EDBTime x, EDBTime y)
        {
            return x.Ticks < y.Ticks;
        }

        public static bool operator >(EDBTime x, EDBTime y)
        {
            return x.Ticks > y.Ticks;
        }

        public static bool operator <=(EDBTime x, EDBTime y)
        {
            return x.Ticks <= y.Ticks;
        }

        public static bool operator >=(EDBTime x, EDBTime y)
        {
            return x.Ticks >= y.Ticks;
        }

        public static explicit operator EDBInterval(EDBTime time)
        {
            return new EDBInterval(time.Ticks);
        }

        public static explicit operator EDBTime(EDBInterval interval)
        {
            return new EDBTime(interval);
        }

        public static explicit operator TimeSpan(EDBTime time)
        {
            return new TimeSpan(time.Ticks);
        }

        public static explicit operator DateTime(EDBTime time)
        {
            try
            {
                return new DateTime(time.Ticks, DateTimeKind.Unspecified);
            }
            catch
            {
                throw new InvalidCastException();
            }
        }

        public static explicit operator EDBTime(TimeSpan interval)
        {
            return new EDBTime(interval);
        }

        public EDBTime AddTicks(long ticksAdded)
        {
            return new EDBTime((Ticks + ticksAdded)%EDBInterval.TicksPerDay);
        }

        private EDBTime AddTicks(long ticksAdded, out int overflow)
        {
            long result = Ticks + ticksAdded;
            overflow = (int) (result/EDBInterval.TicksPerDay);
            result %= EDBInterval.TicksPerDay;
            if (result < 0)
            {
                --overflow; //"carry the one"
            }
            return new EDBTime(result);
        }

        public EDBTime Add(EDBInterval interval)
        {
            return AddTicks(interval.Ticks);
        }

        internal EDBTime Add(EDBInterval interval, out int overflow)
        {
            return AddTicks(interval.Ticks, out overflow);
        }

        public EDBTime Subtract(EDBInterval interval)
        {
            return AddTicks(-interval.Ticks);
        }

        public EDBInterval Subtract(EDBTime earlier)
        {
            return new EDBInterval(Ticks - earlier.Ticks);
        }

        public EDBTimeTZ AtTimeZone(EDBTimeZone timeZone)
        {
            return new EDBTimeTZ(this).AtTimeZone(timeZone);
        }

        public static EDBTime operator +(EDBTime time, EDBInterval interval)
        {
            return time.Add(interval);
        }

        public static EDBTime operator +(EDBInterval interval, EDBTime time)
        {
            return time + interval;
        }

        public static EDBTime operator -(EDBTime time, EDBInterval interval)
        {
            return time.Subtract(interval);
        }

        public static EDBInterval operator -(EDBTime later, EDBTime earlier)
        {
            return later.Subtract(earlier);
        }
    }

    [Serializable]
    public struct EDBTimeTZ : IEquatable<EDBTimeTZ>, IComparable<EDBTimeTZ>, IComparable, IComparer<EDBTimeTZ>,
                                 IComparer
    {
        public static readonly EDBTimeTZ AllBalls = new EDBTimeTZ(EDBTime.AllBalls, EDBTimeZone.UTC);

        public static EDBTimeTZ Now
        {
            get { return new EDBTimeTZ(EDBTime.Now); }
        }

        public static EDBTimeTZ LocalMidnight(EDBDate date)
        {
            return new EDBTimeTZ(EDBTime.AllBalls, EDBTimeZone.LocalTimeZone(date));
        }

        private readonly EDBTime _localTime;
        private readonly EDBTimeZone _timeZone;

        public EDBTimeTZ(EDBTime localTime, EDBTimeZone timeZone)
        {
            _localTime = localTime;
            _timeZone = timeZone;
        }

        public EDBTimeTZ(EDBTime localTime)
            : this(localTime, EDBTimeZone.CurrentTimeZone)
        {
        }

        public EDBTimeTZ(long ticks)
            : this(new EDBTime(ticks))
        {
        }

        public EDBTimeTZ(TimeSpan time)
            : this(new EDBTime(time))
        {
        }

        public EDBTimeTZ(EDBInterval time)
            : this(new EDBTime(time))
        {
        }

        public EDBTimeTZ(EDBTimeTZ copyFrom)
            : this(copyFrom._localTime, copyFrom._timeZone)
        {
        }

        public EDBTimeTZ(int hours, int minutes, int seconds)
            : this(new EDBTime(hours, minutes, seconds))
        {
        }

        public EDBTimeTZ(int hours, int minutes, int seconds, int microseconds)
            : this(new EDBTime(hours, minutes, seconds, microseconds))
        {
        }

        public EDBTimeTZ(int hours, int minutes, decimal seconds)
            : this(new EDBTime(hours, minutes, seconds))
        {
        }

        public EDBTimeTZ(int hours, int minutes, double seconds)
            : this(new EDBTime(hours, minutes, seconds))
        {
        }

        public EDBTimeTZ(long ticks, EDBTimeZone timeZone)
            : this(new EDBTime(ticks), timeZone)
        {
        }

        public EDBTimeTZ(TimeSpan time, EDBTimeZone timeZone)
            : this(new EDBTime(time), timeZone)
        {
        }

        public EDBTimeTZ(EDBInterval time, EDBTimeZone timeZone)
            : this(new EDBTime(time), timeZone)
        {
        }

        public EDBTimeTZ(int hours, int minutes, int seconds, EDBTimeZone timeZone)
            : this(new EDBTime(hours, minutes, seconds), timeZone)
        {
        }

        public EDBTimeTZ(int hours, int minutes, int seconds, int microseconds, EDBTimeZone timeZone)
            : this(new EDBTime(hours, minutes, seconds, microseconds), timeZone)
        {
        }

        public EDBTimeTZ(int hours, int minutes, decimal seconds, EDBTimeZone timeZone)
            : this(new EDBTime(hours, minutes, seconds), timeZone)
        {
        }

        public EDBTimeTZ(int hours, int minutes, double seconds, EDBTimeZone timeZone)
            : this(new EDBTime(hours, minutes, seconds), timeZone)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", _localTime, _timeZone);
        }

        public static EDBTimeTZ Parse(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException();
            }
            try
            {
                int idx = Math.Max(str.IndexOf('+'), str.IndexOf('-'));
                if (idx == -1)
                {
                    throw new FormatException();
                }
                return new EDBTimeTZ(EDBTime.Parse(str.Substring(0, idx)), EDBTimeZone.Parse(str.Substring(idx)));
            }
            catch (OverflowException)
            {
                throw;
            }
            catch
            {
                throw new FormatException();
            }
        }

        public EDBTime LocalTime
        {
            get { return _localTime; }
        }

        public EDBTimeZone TimeZone
        {
            get { return _timeZone; }
        }

        public EDBTime UTCTime
        {
            get { return AtTimeZone(EDBTimeZone.UTC).LocalTime; }
        }

        public EDBTimeTZ AtTimeZone(EDBTimeZone timeZone)
        {
            return new EDBTimeTZ(LocalTime - _timeZone + timeZone, timeZone);
        }

        internal EDBTimeTZ AtTimeZone(EDBTimeZone timeZone, out int overflow)
        {
            return
                new EDBTimeTZ(LocalTime.Add(timeZone - (EDBInterval) (_timeZone), out overflow), timeZone);
        }

        public long Ticks
        {
            get { return _localTime.Ticks; }
        }

        /// <summary>
        /// Gets the number of whole microseconds held in the instance.
        /// <returns>An integer in the range [0, 999999].</returns>
        /// </summary>
        public int Microseconds
        {
            get { return _localTime.Microseconds; }
        }

        /// <summary>
        /// Gets the number of whole milliseconds held in the instance.
        /// <returns>An integer in the range [0, 999].</returns>
        /// </summary>
        public int Milliseconds
        {
            get { return _localTime.Milliseconds; }
        }

        /// <summary>
        /// Gets the number of whole seconds held in the instance.
        /// <returns>An interger in the range [0, 59].</returns>
        /// </summary>
        public int Seconds
        {
            get { return _localTime.Seconds; }
        }

        /// <summary>
        /// Gets the number of whole minutes held in the instance.
        /// <returns>An integer in the range [0, 59].</returns>
        /// </summary>
        public int Minutes
        {
            get { return _localTime.Minutes; }
        }

        /// <summary>
        /// Gets the number of whole hours held in the instance.
        /// <remarks>Note that the time 24:00:00 can be stored for roundtrip compatibility. Any calculations on such a
        /// value will normalised it to 00:00:00.</remarks>
        /// </summary>
        public int Hours
        {
            get { return _localTime.Hours; }
        }

        /// <summary>
        /// Normalise this time; if it is 24:00:00, convert it to 00:00:00
        /// </summary>
        /// <returns>This time, normalised</returns>
        public EDBTimeTZ Normalize()
        {
            return new EDBTimeTZ(_localTime.Normalize(), _timeZone);
        }

        public bool Equals(EDBTimeTZ other)
        {
            return _localTime.Equals(other._localTime) && _timeZone.Equals(other._timeZone);
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is EDBTimeTZ && Equals((EDBTimeTZ) obj);
        }

        public override int GetHashCode()
        {
            return _localTime.GetHashCode() ^ PGUtil.RotateShift(_timeZone.GetHashCode(), 24);
        }

        /// <summary>
        /// Compares this with another <see cref="EDBTimeTZ"/>. As per postgres' rules,
        /// first the times are compared as if they were both in the same timezone. If they are equal then
        /// then timezones are compared (+01:00 being "smaller" than -01:00).
        /// </summary>
        /// <param name="other">the <see cref="EDBTimeTZ"/> to compare with.</param>
        /// <returns>An integer which is 0 if they are equal, &lt; 0 if this is the smaller and &gt; 0 if this is the larger.</returns>
        public int CompareTo(EDBTimeTZ other)
        {
            int cmp = AtTimeZone(EDBTimeZone.UTC).LocalTime.CompareTo(other.AtTimeZone(EDBTimeZone.UTC).LocalTime);
            return cmp == 0 ? _timeZone.CompareTo(other._timeZone) : cmp;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is EDBTimeTZ)
            {
                return CompareTo((EDBTimeTZ) obj);
            }
            throw new ArgumentException();
        }

        public int Compare(EDBTimeTZ x, EDBTimeTZ y)
        {
            return x.CompareTo(y);
        }

        public int Compare(object x, object y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }
            if (y == null)
            {
                return 1;
            }
            if (!(x is IComparable) || !(y is IComparable))
            {
                throw new ArgumentException();
            }
            return ((IComparable) x).CompareTo(y);
        }

        public static bool operator ==(EDBTimeTZ x, EDBTimeTZ y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBTimeTZ x, EDBTimeTZ y)
        {
            return !(x == y);
        }

        public static bool operator <(EDBTimeTZ x, EDBTimeTZ y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator >(EDBTimeTZ x, EDBTimeTZ y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <=(EDBTimeTZ x, EDBTimeTZ y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >=(EDBTimeTZ x, EDBTimeTZ y)
        {
            return x.CompareTo(y) >= 0;
        }

        public EDBTimeTZ Add(EDBInterval interval)
        {
            return new EDBTimeTZ(_localTime.Add(interval), _timeZone);
        }

        internal EDBTimeTZ Add(EDBInterval interval, out int overflow)
        {
            return new EDBTimeTZ(_localTime.Add(interval, out overflow), _timeZone);
        }

        public EDBTimeTZ Subtract(EDBInterval interval)
        {
            return new EDBTimeTZ(_localTime.Subtract(interval), _timeZone);
        }

        public EDBInterval Subtract(EDBTimeTZ earlier)
        {
            return _localTime.Subtract(earlier.AtTimeZone(_timeZone)._localTime);
        }

        public static EDBTimeTZ operator +(EDBTimeTZ time, EDBInterval interval)
        {
            return time.Add(interval);
        }

        public static EDBTimeTZ operator +(EDBInterval interval, EDBTimeTZ time)
        {
            return time + interval;
        }

        public static EDBTimeTZ operator -(EDBTimeTZ time, EDBInterval interval)
        {
            return time.Subtract(interval);
        }

        public static EDBInterval operator -(EDBTimeTZ later, EDBTimeTZ earlier)
        {
            return later.Subtract(earlier);
        }

        public static explicit operator EDBTimeTZ(TimeSpan time)
        {
            return new EDBTimeTZ(new EDBTime(time));
        }

        public static explicit operator TimeSpan(EDBTimeTZ time)
        {
            return (TimeSpan) time.LocalTime;
        }

        public static explicit operator DateTime(EDBTimeTZ time)
        {
            // LocalTime property is actually time local to TimeZone
            return new DateTime(time.AtTimeZone(EDBTimeZone.CurrentTimeZone).Ticks, DateTimeKind.Local);
        }
    }

    [Serializable]
    public struct EDBTimeStamp : IEquatable<EDBTimeStamp>, IComparable<EDBTimeStamp>, IComparable,
                                    IComparer<EDBTimeStamp>, IComparer
    {
        private enum TimeType
        {
            Finite,
            Infinity,
            MinusInfinity
        }

        public static readonly EDBTimeStamp Epoch = new EDBTimeStamp(EDBDate.Epoch);
        public static readonly EDBTimeStamp Era = new EDBTimeStamp(EDBDate.Era);

        public static readonly EDBTimeStamp Infinity =
            new EDBTimeStamp(TimeType.Infinity, EDBDate.Era, EDBTime.AllBalls);

        public static readonly EDBTimeStamp MinusInfinity =
            new EDBTimeStamp(TimeType.MinusInfinity, EDBDate.Era, EDBTime.AllBalls);

        public static EDBTimeStamp Now
        {
            get { return new EDBTimeStamp(EDBDate.Now, EDBTime.Now); }
        }

        public static EDBTimeStamp Today
        {
            get { return new EDBTimeStamp(EDBDate.Now); }
        }

        public static EDBTimeStamp Yesterday
        {
            get { return new EDBTimeStamp(EDBDate.Yesterday); }
        }

        public static EDBTimeStamp Tomorrow
        {
            get { return new EDBTimeStamp(EDBDate.Tomorrow); }
        }

        private readonly EDBDate _date;
        private readonly EDBTime _time;
        private readonly TimeType _type;

        private EDBTimeStamp(TimeType type, EDBDate date, EDBTime time)
        {
            _type = type;
            _date = date;
            _time = time;
        }

        public EDBTimeStamp(EDBDate date, EDBTime time)
            : this(TimeType.Finite, date, time)
        {
        }

        public EDBTimeStamp(EDBDate date)
            : this(date, EDBTime.AllBalls)
        {
        }

        public EDBTimeStamp(int year, int month, int day, int hours, int minutes, int seconds)
            : this(new EDBDate(year, month, day), new EDBTime(hours, minutes, seconds))
        {
        }

        public EDBDate Date
        {
            get { return _date; }
        }

        public EDBTime Time
        {
            get { return _time; }
        }

        public int DayOfYear
        {
            get { return _date.DayOfYear; }
        }

        public int Year
        {
            get { return _date.Year; }
        }

        public int Month
        {
            get { return _date.Month; }
        }

        public int Day
        {
            get { return _date.Day; }
        }

        public DayOfWeek DayOfWeek
        {
            get { return _date.DayOfWeek; }
        }

        public bool IsLeapYear
        {
            get { return _date.IsLeapYear; }
        }

        public EDBTimeStamp AddDays(int days)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    return this;
                default:
                    return new EDBTimeStamp(_date.AddDays(days), _time);
            }
        }

        public EDBTimeStamp AddYears(int years)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    return this;
                default:
                    return new EDBTimeStamp(_date.AddYears(years), _time);
            }
        }

        public EDBTimeStamp AddMonths(int months)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    return this;
                default:
                    return new EDBTimeStamp(_date.AddMonths(months), _time);
            }
        }

        public long Ticks
        {
            get { return _date.DaysSinceEra*EDBInterval.TicksPerDay + _time.Ticks; }
        }

        public int Microseconds
        {
            get { return _time.Microseconds; }
        }

        public int Milliseconds
        {
            get { return _time.Milliseconds; }
        }

        public int Seconds
        {
            get { return _time.Seconds; }
        }

        public int Minutes
        {
            get { return _time.Minutes; }
        }

        public int Hours
        {
            get { return _time.Hours; }
        }

        public bool IsFinite
        {
            get { return _type == TimeType.Finite; }
        }

        public bool IsInfinity
        {
            get { return _type == TimeType.Infinity; }
        }

        public bool IsMinusInfinity
        {
            get { return _type == TimeType.MinusInfinity; }
        }

        public EDBTimeStamp Normalize()
        {
            return Add(EDBInterval.Zero);
        }

        public override string ToString()
        {
            switch (_type)
            {
                case TimeType.Infinity:
                    return "infinity";
                case TimeType.MinusInfinity:
                    return "-infinity";
                default:
                    return string.Format("{0} {1}", _date, _time);
            }
        }

        public static EDBTimeStamp Parse(string str)
        {
            if (str == null)
            {
                throw new NullReferenceException();
            }
            switch (str = str.Trim().ToLowerInvariant())
            {
                case "infinity":
                    return Infinity;
                case "-infinity":
                    return MinusInfinity;
                default:
                    try
                    {
                        int idxSpace = str.IndexOf(' ');
                        string datePart = str.Substring(0, idxSpace);
                        if (str.Contains("bc"))
                        {
                            datePart += " BC";
                        }
                        int idxSecond = str.IndexOf(' ', idxSpace + 1);
                        if (idxSecond == -1)
                        {
                            idxSecond = str.Length;
                        }
                        string timePart = str.Substring(idxSpace + 1, idxSecond - idxSpace - 1);
                        return new EDBTimeStamp(EDBDate.Parse(datePart), EDBTime.Parse(timePart));
                    }
                    catch (OverflowException)
                    {
                        throw;
                    }
                    catch
                    {
                        throw new FormatException();
                    }
            }
        }

        public bool Equals(EDBTimeStamp other)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                    return other._type == TimeType.Infinity;
                case TimeType.MinusInfinity:
                    return other._type == TimeType.MinusInfinity;
                default:
                    return other._type == TimeType.Finite && _date.Equals(other._date) && _time.Equals(other._time);
            }
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is EDBTimeStamp && Equals((EDBTimeStamp) obj);
        }

        public override int GetHashCode()
        {
            switch (_type)
            {
                case TimeType.Infinity:
                    return int.MaxValue;
                case TimeType.MinusInfinity:
                    return int.MinValue;
                default:
                    return _date.GetHashCode() ^ PGUtil.RotateShift(_time.GetHashCode(), 16);
            }
        }

        public int CompareTo(EDBTimeStamp other)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                    return other._type == TimeType.Infinity ? 0 : 1;
                case TimeType.MinusInfinity:
                    return other._type == TimeType.MinusInfinity ? 0 : -1;
                default:
                    switch (other._type)
                    {
                        case TimeType.Infinity:
                            return -1;
                        case TimeType.MinusInfinity:
                            return 1;
                        default:
                            int cmp = _date.CompareTo(other._date);
                            return cmp == 0 ? _time.CompareTo(_time) : cmp;
                    }
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is EDBTimeStamp)
            {
                return CompareTo((EDBTimeStamp) obj);
            }
            throw new ArgumentException();
        }

        public int Compare(EDBTimeStamp x, EDBTimeStamp y)
        {
            return x.CompareTo(y);
        }

        public int Compare(object x, object y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }
            if (y == null)
            {
                return 1;
            }
            if (!(x is IComparable) || !(y is IComparable))
            {
                throw new ArgumentException();
            }
            return ((IComparable) x).CompareTo(y);
        }

        public EDBTimeStampTZ AtTimeZone(EDBTimeZone timeZoneFrom, EDBTimeZone timeZoneTo)
        {
            int overflow;
            EDBTimeTZ adjusted = new EDBTimeTZ(_time, timeZoneFrom).AtTimeZone(timeZoneTo, out overflow);
            return new EDBTimeStampTZ(_date.AddDays(overflow), adjusted);
        }

        public EDBTimeStampTZ AtTimeZone(EDBTimeZone timeZone)
        {
            return AtTimeZone(timeZone, EDBTimeZone.LocalTimeZone(_date));
        }

        public EDBTimeStamp Add(EDBInterval interval)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    return this;
                default:
                    int overflow;
                    EDBTime time = _time.Add(interval, out overflow);
                    return new EDBTimeStamp(_date.Add(interval, overflow), time);
            }
        }

        public EDBTimeStamp Subtract(EDBInterval interval)
        {
            return Add(-interval);
        }

        public EDBInterval Subtract(EDBTimeStamp timestamp)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    throw new ArgumentOutOfRangeException("this", "You cannot subtract infinity timestamps");
            }
            switch (timestamp._type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    throw new ArgumentOutOfRangeException("timestamp", "You cannot subtract infinity timestamps");
            }
            return new EDBInterval(0, _date.DaysSinceEra - timestamp._date.DaysSinceEra, _time.Ticks - timestamp._time.Ticks);
        }

        public static implicit operator EDBTimeStamp(DateTime datetime)
        {
            if (datetime == DateTime.MaxValue)
            {
                return Infinity;
            }
            else if (datetime == DateTime.MinValue)
            {
                return MinusInfinity;
            }
            else
            {
                return new EDBTimeStamp(new EDBDate(datetime), new EDBTime(datetime.TimeOfDay));
            }
        }

        public static implicit operator DateTime(EDBTimeStamp timestamp)
        {
            switch (timestamp._type)
            {
                case TimeType.Infinity:
                    return DateTime.MaxValue;
                case TimeType.MinusInfinity:
                    return DateTime.MinValue;
                default:
                    try
                    {
                        return
                            new DateTime(timestamp.Date.DaysSinceEra*EDBInterval.TicksPerDay + timestamp._time.Ticks,
                                         DateTimeKind.Unspecified);
                    }
                    catch
                    {
                        throw new InvalidCastException();
                    }
            }
        }

        public static EDBTimeStamp operator +(EDBTimeStamp timestamp, EDBInterval interval)
        {
            return timestamp.Add(interval);
        }

        public static EDBTimeStamp operator +(EDBInterval interval, EDBTimeStamp timestamp)
        {
            return timestamp.Add(interval);
        }

        public static EDBTimeStamp operator -(EDBTimeStamp timestamp, EDBInterval interval)
        {
            return timestamp.Subtract(interval);
        }

        public static EDBInterval operator -(EDBTimeStamp x, EDBTimeStamp y)
        {
            return x.Subtract(y);
        }

        public static bool operator ==(EDBTimeStamp x, EDBTimeStamp y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBTimeStamp x, EDBTimeStamp y)
        {
            return !(x == y);
        }

        public static bool operator <(EDBTimeStamp x, EDBTimeStamp y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator >(EDBTimeStamp x, EDBTimeStamp y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <=(EDBTimeStamp x, EDBTimeStamp y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >=(EDBTimeStamp x, EDBTimeStamp y)
        {
            return x.CompareTo(y) >= 0;
        }
    }

    [Serializable]
    public struct EDBTimeStampTZ : IEquatable<EDBTimeStampTZ>, IComparable<EDBTimeStampTZ>, IComparable,
                                      IComparer<EDBTimeStampTZ>, IComparer
    {
        private enum TimeType
        {
            Finite,
            Infinity,
            MinusInfinity
        }

        public static readonly EDBTimeStampTZ Epoch = new EDBTimeStampTZ(EDBDate.Epoch, EDBTimeTZ.AllBalls);
        public static readonly EDBTimeStampTZ Era = new EDBTimeStampTZ(EDBDate.Era, EDBTimeTZ.AllBalls);

        public static readonly EDBTimeStampTZ Infinity =
            new EDBTimeStampTZ(TimeType.Infinity, EDBDate.Era, EDBTimeTZ.AllBalls);

        public static readonly EDBTimeStampTZ MinusInfinity =
            new EDBTimeStampTZ(TimeType.MinusInfinity, EDBDate.Era, EDBTimeTZ.AllBalls);

        public static EDBTimeStampTZ Now
        {
            get { return new EDBTimeStampTZ(EDBDate.Now, EDBTimeTZ.Now); }
        }

        public static EDBTimeStampTZ Today
        {
            get { return new EDBTimeStampTZ(EDBDate.Now); }
        }

        public static EDBTimeStampTZ Yesterday
        {
            get { return new EDBTimeStampTZ(EDBDate.Yesterday); }
        }

        public static EDBTimeStampTZ Tomorrow
        {
            get { return new EDBTimeStampTZ(EDBDate.Tomorrow); }
        }

        private readonly EDBDate _date;
        private readonly EDBTimeTZ _time;
        private readonly TimeType _type;

        private EDBTimeStampTZ(TimeType type, EDBDate date, EDBTimeTZ time)
        {
            _type = type;
            _date = date;
            _time = time;
        }

        public EDBTimeStampTZ(EDBDate date, EDBTimeTZ time)
            : this(TimeType.Finite, date, time)
        {
        }

        public EDBTimeStampTZ(EDBDate date)
            : this(date, EDBTimeTZ.LocalMidnight(date))
        {
        }

        public EDBTimeStampTZ(int year, int month, int day, int hours, int minutes, int seconds, EDBTimeZone? timezone)
            : this(
                new EDBDate(year, month, day),
                new EDBTimeTZ(hours, minutes, seconds,
                                 timezone.HasValue ? timezone.Value : EDBTimeZone.LocalTimeZone(new EDBDate(year, month, day)))
                )
        {
        }

        public EDBDate Date
        {
            get { return _date; }
        }

        public EDBTimeTZ Time
        {
            get { return _time; }
        }

        public int DayOfYear
        {
            get { return _date.DayOfYear; }
        }

        public int Year
        {
            get { return _date.Year; }
        }

        public int Month
        {
            get { return _date.Month; }
        }

        public int Day
        {
            get { return _date.Day; }
        }

        public DayOfWeek DayOfWeek
        {
            get { return _date.DayOfWeek; }
        }

        public bool IsLeapYear
        {
            get { return _date.IsLeapYear; }
        }

        public EDBTimeStampTZ AddDays(int days)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    return this;
                default:
                    return new EDBTimeStampTZ(_date.AddDays(days), _time);
            }
        }

        public EDBTimeStampTZ AddYears(int years)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    return this;
                default:
                    return new EDBTimeStampTZ(_date.AddYears(years), _time);
            }
        }

        public EDBTimeStampTZ AddMonths(int months)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    return this;
                default:
                    return new EDBTimeStampTZ(_date.AddMonths(months), _time);
            }
        }

        public EDBTime LocalTime
        {
            get { return _time.LocalTime; }
        }

        public EDBTimeZone TimeZone
        {
            get { return _time.TimeZone; }
        }

        public EDBTime UTCTime
        {
            get { return _time.UTCTime; }
        }

        public long Ticks
        {
            get { return _date.DaysSinceEra*EDBInterval.TicksPerDay + _time.Ticks; }
        }

        public int Microseconds
        {
            get { return _time.Microseconds; }
        }

        public int Milliseconds
        {
            get { return _time.Milliseconds; }
        }

        public int Seconds
        {
            get { return _time.Seconds; }
        }

        public int Minutes
        {
            get { return _time.Minutes; }
        }

        public int Hours
        {
            get { return _time.Hours; }
        }

        public bool IsFinite
        {
            get { return _type == TimeType.Finite; }
        }

        public bool IsInfinity
        {
            get { return _type == TimeType.Infinity; }
        }

        public bool IsMinusInfinity
        {
            get { return _type == TimeType.MinusInfinity; }
        }

        public EDBTimeStampTZ Normalize()
        {
            return Add(EDBInterval.Zero);
        }

        public override string ToString()
        {
            switch (_type)
            {
                case TimeType.Infinity:
                    return "infinity";
                case TimeType.MinusInfinity:
                    return "-infinity";
                default:
                    return string.Format("{0} {1}", _date, _time);
            }
        }

        public static EDBTimeStampTZ Parse(string str)
        {
            if (str == null)
            {
                throw new NullReferenceException();
            }
            switch (str = str.Trim().ToLowerInvariant())
            {
                case "infinity":
                    return Infinity;
                case "-infinity":
                    return MinusInfinity;
                default:
                    try
                    {
                        int idxSpace = str.IndexOf(' ');
                        string datePart = str.Substring(0, idxSpace);
                        if (str.Contains("bc"))
                        {
                            datePart += " BC";
                        }
                        int idxSecond = str.IndexOf(' ', idxSpace + 1);
                        if (idxSecond == -1)
                        {
                            idxSecond = str.Length;
                        }
                        string timePart = str.Substring(idxSpace + 1, idxSecond - idxSpace - 1);
                        return new EDBTimeStampTZ(EDBDate.Parse(datePart), EDBTimeTZ.Parse(timePart));
                    }
                    catch (OverflowException)
                    {
                        throw;
                    }
                    catch
                    {
                        throw new FormatException();
                    }
            }
        }

        public bool Equals(EDBTimeStampTZ other)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                    return other._type == TimeType.Infinity;
                case TimeType.MinusInfinity:
                    return other._type == TimeType.MinusInfinity;
                default:
                    return other._type == TimeType.Finite && _date.Equals(other._date) && _time.Equals(other._time);
            }
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is EDBTimeStamp && Equals((EDBTimeStampTZ) obj);
        }

        public override int GetHashCode()
        {
            switch (_type)
            {
                case TimeType.Infinity:
                    return int.MaxValue;
                case TimeType.MinusInfinity:
                    return int.MinValue;
                default:
                    return _date.GetHashCode() ^ PGUtil.RotateShift(_time.GetHashCode(), 16);
            }
        }

        public int CompareTo(EDBTimeStampTZ other)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                    return other._type == TimeType.Infinity ? 0 : 1;
                case TimeType.MinusInfinity:
                    return other._type == TimeType.MinusInfinity ? 0 : -1;
                default:
                    switch (other._type)
                    {
                        case TimeType.Infinity:
                            return -1;
                        case TimeType.MinusInfinity:
                            return 1;
                        default:
                            int cmp = _date.CompareTo(other._date);
                            return cmp == 0 ? _time.CompareTo(_time) : cmp;
                    }
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is EDBTimeStamp)
            {
                return CompareTo((EDBTimeStamp) obj);
            }
            throw new ArgumentException();
        }

        public int Compare(EDBTimeStampTZ x, EDBTimeStampTZ y)
        {
            return x.CompareTo(y);
        }

        public int Compare(object x, object y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }
            if (y == null)
            {
                return 1;
            }
            if (!(x is IComparable) || !(y is IComparable))
            {
                throw new ArgumentException();
            }
            return ((IComparable) x).CompareTo(y);
        }

        public EDBTimeStamp AtTimeZone(EDBTimeZone timeZone)
        {
            int overflow;
            EDBTimeTZ adjusted = _time.AtTimeZone(timeZone, out overflow);
            return new EDBTimeStamp(_date.AddDays(overflow), adjusted.LocalTime);
        }

        public EDBTimeStampTZ Add(EDBInterval interval)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    return this;
                default:
                    int overflow;
                    EDBTimeTZ time = _time.Add(interval, out overflow);
                    return new EDBTimeStampTZ(_date.Add(interval, overflow), time);
            }
        }

        public EDBTimeStampTZ Subtract(EDBInterval interval)
        {
            return Add(-interval);
        }

        public EDBInterval Subtract(EDBTimeStampTZ timestamp)
        {
            switch (_type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    throw new ArgumentOutOfRangeException("this", "You cannot subtract infinity timestamps");
            }
            switch (timestamp._type)
            {
                case TimeType.Infinity:
                case TimeType.MinusInfinity:
                    throw new ArgumentOutOfRangeException("timestamp", "You cannot subtract infinity timestamps");
            }
            return new EDBInterval(0, _date.DaysSinceEra - timestamp._date.DaysSinceEra, (_time - timestamp._time).Ticks);
        }

        public static implicit operator EDBTimeStampTZ(DateTime datetime)
        {
            if (datetime == DateTime.MaxValue)
            {
                return Infinity;
            }
            else if (datetime == DateTime.MinValue)
            {
                return MinusInfinity;
            }
            else
            {
                EDBDate newDate = new EDBDate(datetime);
                return
                    new EDBTimeStampTZ(newDate,
                                          new EDBTimeTZ(datetime.TimeOfDay,
                                                           datetime.Kind == DateTimeKind.Utc
                                                               ? EDBTimeZone.UTC
                                                               : EDBTimeZone.LocalTimeZone(newDate)));
            }
        }

        public static explicit operator DateTime(EDBTimeStampTZ timestamp)
        {
            switch (timestamp._type)
            {
                case TimeType.Infinity:
                    return DateTime.MaxValue;
                case TimeType.MinusInfinity:
                    return DateTime.MinValue;
                default:
                    try
                    {
                        EDBTimeStamp utc = timestamp.AtTimeZone(EDBTimeZone.UTC);
                        return new DateTime(utc.Date.DaysSinceEra*EDBInterval.TicksPerDay + utc.Time.Ticks, DateTimeKind.Utc);
                    }
                    catch
                    {
                        throw new InvalidCastException();
                    }
            }
        }

        public static implicit operator EDBTimeStampTZ(DateTimeOffset datetimeoffset)
        {
            if (datetimeoffset == DateTimeOffset.MaxValue)
            {
                return Infinity;
            }
            else if (datetimeoffset == DateTimeOffset.MinValue)
            {
                return MinusInfinity;
            }
            else
            {
                EDBDate newDate = new EDBDate(datetimeoffset.Year,
                    datetimeoffset.Month, datetimeoffset.Day);
                return
                    new EDBTimeStampTZ(newDate, new EDBTimeTZ(datetimeoffset.TimeOfDay,
                        new EDBTimeZone(datetimeoffset.Offset)));
            }
        }
        public static explicit operator DateTimeOffset(EDBTimeStampTZ timestamp)
        {
            switch (timestamp._type)
            {
                case TimeType.Infinity:
                    return DateTimeOffset.MaxValue;
                case TimeType.MinusInfinity:
                    return DateTimeOffset.MinValue;
                default:
                    try
                    {
                        return new DateTimeOffset(timestamp.Date.DaysSinceEra * EDBInterval.TicksPerDay + timestamp.Time.Ticks, timestamp.TimeZone);
                    }
                    catch
                    {
                        throw new InvalidCastException();
                    }
            }
        }

        public static EDBTimeStampTZ operator +(EDBTimeStampTZ timestamp, EDBInterval interval)
        {
            return timestamp.Add(interval);
        }

        public static EDBTimeStampTZ operator +(EDBInterval interval, EDBTimeStampTZ timestamp)
        {
            return timestamp.Add(interval);
        }

        public static EDBTimeStampTZ operator -(EDBTimeStampTZ timestamp, EDBInterval interval)
        {
            return timestamp.Subtract(interval);
        }

        public static EDBInterval operator -(EDBTimeStampTZ x, EDBTimeStampTZ y)
        {
            return x.Subtract(y);
        }

        public static bool operator ==(EDBTimeStampTZ x, EDBTimeStampTZ y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBTimeStampTZ x, EDBTimeStampTZ y)
        {
            return !(x == y);
        }

        public static bool operator <(EDBTimeStampTZ x, EDBTimeStampTZ y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator >(EDBTimeStampTZ x, EDBTimeStampTZ y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <=(EDBTimeStampTZ x, EDBTimeStampTZ y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >=(EDBTimeStampTZ x, EDBTimeStampTZ y)
        {
            return x.CompareTo(y) >= 0;
        }
    }
}

#pragma warning restore 1591
