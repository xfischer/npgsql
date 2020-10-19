using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace EnterpriseDB.EDBClient
{
    sealed class EDBSqlEventSource : EventSource
    {
        static readonly EDBSqlEventSource Log = new EDBSqlEventSource();

        const string EventSourceName = "EnterpriseDB.EDBClient.Sql";

        const int CommandStartId = 3;
        const int CommandStopId = 4;

        internal EDBSqlEventSource() : base(EventSourceName) {}

        // NOTE
        // - The 'Start' and 'Stop' suffixes on the following event names have special meaning in EventSource. They
        //   enable creating 'activities'.
        //   For more information, take a look at the following blog post:
        //   https://blogs.msdn.microsoft.com/vancem/2015/09/14/exploring-eventsource-activity-correlation-and-causation-features/
        // - A stop event's event id must be next one after its start event.

        [Event(CommandStartId, Level = EventLevel.Informational)]
        public static void CommandStart(string sql) => Log.WriteEvent(CommandStartId, sql);

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Event(CommandStopId, Level = EventLevel.Informational)]
        public static void CommandStop() => Log.WriteEvent(CommandStopId);
    }
}
