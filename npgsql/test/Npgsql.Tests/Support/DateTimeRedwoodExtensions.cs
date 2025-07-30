using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Npgsql.Tests.Support;

public static class DateTimeRedwoodExtensions
{
    public static DateTime RemoveSecondsFraction(this DateTime value) =>
        new DateTime(value.Year, value.Month, value.Day,
            value.Hour, value.Minute, value.Second,
            DateTimeKind.Unspecified);

    public static DateTimeOffset RemoveSecondsFraction(this DateTimeOffset value) =>
        new DateTimeOffset(value.Year, value.Month, value.Day,
            value.Hour, value.Minute, value.Second, value.Offset);

}
