using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.TypeHandling;

namespace EnterpriseDB.EDBClient.TypeHandlers.CompositeHandlers
{
    sealed class CompositeParameterHandler<T> : CompositeParameterHandler
    {
        public CompositeParameterHandler(EDBTypeHandler handler, ParameterInfo parameterInfo)
            : base(handler, parameterInfo) { }

        public override ValueTask<object?> Read(EDBReadBuffer buffer, bool async)
        {
            var task = Read<T>(buffer, async);
            return task.IsCompleted
                ? new ValueTask<object?>(task.Result)
                : AwaitTask();

            async ValueTask<object?> AwaitTask() => await task;
        }
    }
}
