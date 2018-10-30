using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    interface IMappedCompositeHandler
    {
        /// <summary>
        /// The CLR type mapped to the PostgreSQL composite type.
        /// </summary>
        Type CompositeType { get; }
    }

    class MappedCompositeHandler<T> : EDBTypeHandler<T>, IMappedCompositeHandler where T : new()
    {
        readonly IEDBNameTranslator _nameTranslator;
        readonly EDBConnection _conn;
        readonly UnmappedCompositeHandler _wrappedHandler;

        public Type CompositeType => typeof(T);

        internal MappedCompositeHandler(IEDBNameTranslator nameTranslator, PostgresType pgType, EDBConnection conn)
        {
            PostgresType = pgType;
            _nameTranslator = nameTranslator;
            _conn = conn;
            _wrappedHandler = (UnmappedCompositeHandler)new UnmappedCompositeTypeHandlerFactory(_nameTranslator).Create(PostgresType, _conn);
        }

        public override ValueTask<T> Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription = null)
            => _wrappedHandler.Read<T>(buf, len, async, fieldDescription);

        public override int ValidateAndGetLength(T value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => _wrappedHandler.ValidateAndGetLength(value, ref lengthCache, parameter);

        public override Task Write(T value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
           => _wrappedHandler.Write(value, buf, lengthCache, parameter, async);
    }

    class MappedCompositeTypeHandlerFactory<T> : EDBTypeHandlerFactory<T>
        where T : new()
    {
        readonly IEDBNameTranslator _nameTranslator;

        internal MappedCompositeTypeHandlerFactory(IEDBNameTranslator nameTranslator)
        {
            _nameTranslator = nameTranslator;
        }

        internal override EDBTypeHandler Create(PostgresType pgType, EDBConnection conn)
            => new MappedCompositeHandler<T>(_nameTranslator, pgType, conn);

        protected override EDBTypeHandler<T> Create(EDBConnection conn)
            => throw new InvalidOperationException($"Expect {nameof(PostgresType)}");
    }
}
