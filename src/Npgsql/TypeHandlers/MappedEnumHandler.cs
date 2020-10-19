using System;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    interface IMappedEnumHandler
    {
        /// <summary>
        /// The CLR type mapped to the PostgreSQL composite type.
        /// </summary>
        Type EnumType { get; }
    }

    class MappedEnumHandler<T> : EDBTypeHandler<T>, IMappedEnumHandler where T : new()
    {
        readonly IEDBNameTranslator _nameTranslator;
        readonly EDBConnection _conn;
        readonly UnmappedEnumHandler _wrappedHandler;

        public Type EnumType => typeof(T);

        internal MappedEnumHandler(PostgresType pgType, IEDBNameTranslator nameTranslator, EDBConnection conn)
            : base(pgType)
        {
            _nameTranslator = nameTranslator;
            _conn = conn;
            _wrappedHandler = (UnmappedEnumHandler)new UnmappedEnumTypeHandlerFactory(_nameTranslator).Create(PostgresType, _conn);
        }

        public override ValueTask<T> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
            => _wrappedHandler.Read<T>(buf, len, async, fieldDescription);

        public override int ValidateAndGetLength(T value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => _wrappedHandler.ValidateAndGetLength(value, ref lengthCache, parameter);

        public override Task Write(T value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
           => _wrappedHandler.Write(value!, buf, lengthCache, parameter, async);
    }

    class MappedEnumTypeHandlerFactory<T> : EDBTypeHandlerFactory<T>
        where T : new()
    {
        readonly IEDBNameTranslator _nameTranslator;

        internal MappedEnumTypeHandlerFactory(IEDBNameTranslator nameTranslator)
        {
            _nameTranslator = nameTranslator;
        }

        public override EDBTypeHandler<T> Create(PostgresType pgType, EDBConnection conn)
            => new MappedEnumHandler<T>(pgType, _nameTranslator, conn);
    }
}
