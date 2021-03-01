using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.CompositeHandlers
{
    abstract class CompositeMemberHandler<TComposite>
    {
        public MemberInfo MemberInfo { get; }
        public PostgresType PostgresType { get; }

        protected CompositeMemberHandler(MemberInfo memberInfo, PostgresType postgresType)
        {
            MemberInfo = memberInfo;
            PostgresType = postgresType;
        }

        public abstract ValueTask Read(TComposite composite, EDBReadBuffer buffer, bool async);

        public abstract ValueTask Read(ByReference<TComposite> composite, EDBReadBuffer buffer, bool async);

        public abstract Task Write(TComposite composite, EDBWriteBuffer buffer, EDBLengthCache? lengthCache, bool async, CancellationToken cancellationToken = default);

        public abstract int ValidateAndGetLength(TComposite composite, ref EDBLengthCache? lengthCache);
    }
}
