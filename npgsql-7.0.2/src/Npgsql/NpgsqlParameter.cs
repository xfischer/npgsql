using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeMapping;
using EnterpriseDB.EDBClient.Util;
using EDBTypes;
using static EnterpriseDB.EDBClient.Util.Statics;

namespace EnterpriseDB.EDBClient;

///<summary>
/// This class represents a parameter to a command that will be sent to server
///</summary>
public class EDBParameter : DbParameter, IDbDataParameter, ICloneable
{
    #region Fields and Properties

    private protected byte _precision;
    private protected byte _scale;
    private protected int _size;

    // ReSharper disable InconsistentNaming
    private protected EDBDbType? _npgsqlDbType;
    private protected string? _dataTypeName;
    // ReSharper restore InconsistentNaming

    private protected string _name = string.Empty;
    private protected object? _value;
    private protected string _sourceColumn;

    internal string TrimmedName { get; private protected set; } = PositionalName;
    internal const string PositionalName = "";

    /// <summary>
    /// Can be used to communicate a value from the validation phase to the writing phase.
    /// To be used by type handlers only.
    /// </summary>
    public object? ConvertedValue { get; set; }

    internal EDBLengthCache? LengthCache { get; set; }

    internal EDBTypeHandler? Handler { get; set; }

    internal FormatCode FormatCode { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBParameter"/> class.
    /// </summary>
    public EDBParameter()
    {
        _sourceColumn = string.Empty;
        Direction = ParameterDirection.Input;
        SourceVersion = DataRowVersion.Current;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBParameter"/> class with the parameter name and a value.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to map.</param>
    /// <param name="value">The value of the <see cref="EDBParameter"/>.</param>
    /// <remarks>
    /// <p>
    /// When you specify an <see cref="object"/> in the value parameter, the <see cref="System.Data.DbType"/> is
    /// inferred from the CLR type.
    /// </p>
    /// <p>
    /// When using this constructor, you must be aware of a possible misuse of the constructor which takes a <see cref="DbType"/>
    /// parameter. This happens when calling this constructor passing an int 0 and the compiler thinks you are passing a value of
    /// <see cref="DbType"/>. Use <see cref="Convert.ToInt32(object)"/> for example to have compiler calling the correct constructor.
    /// </p>
    /// </remarks>
    public EDBParameter(string? parameterName, object? value)
        : this()
    {
        ParameterName = parameterName;
        // ReSharper disable once VirtualMemberCallInConstructor
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBParameter"/> class with the parameter name and the data type.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to map.</param>
    /// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType"/> values.</param>
    public EDBParameter(string? parameterName, EDBDbType parameterType)
        : this(parameterName, parameterType, 0, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBParameter"/>.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to map.</param>
    /// <param name="parameterType">One of the <see cref="System.Data.DbType"/> values.</param>
    public EDBParameter(string? parameterName, DbType parameterType)
        : this(parameterName, parameterType, 0, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBParameter"/>.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to map.</param>
    /// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType"/> values.</param>
    /// <param name="size">The length of the parameter.</param>
    public EDBParameter(string? parameterName, EDBDbType parameterType, int size)
        : this(parameterName, parameterType, size, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBParameter"/>.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to map.</param>
    /// <param name="parameterType">One of the <see cref="System.Data.DbType"/> values.</param>
    /// <param name="size">The length of the parameter.</param>
    public EDBParameter(string? parameterName, DbType parameterType, int size)
        : this(parameterName, parameterType, size, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBParameter"/>
    /// </summary>
    /// <param name="parameterName">The name of the parameter to map.</param>
    /// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType"/> values.</param>
    /// <param name="size">The length of the parameter.</param>
    /// <param name="sourceColumn">The name of the source column.</param>
    public EDBParameter(string? parameterName, EDBDbType parameterType, int size, string? sourceColumn)
    {
        ParameterName = parameterName;
        EDBDbType = parameterType;
        _size = size;
        _sourceColumn = sourceColumn ?? string.Empty;
        Direction = ParameterDirection.Input;
        SourceVersion = DataRowVersion.Current;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBParameter"/>.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to map.</param>
    /// <param name="parameterType">One of the <see cref="System.Data.DbType"/> values.</param>
    /// <param name="size">The length of the parameter.</param>
    /// <param name="sourceColumn">The name of the source column.</param>
    public EDBParameter(string? parameterName, DbType parameterType, int size, string? sourceColumn)
    {
        ParameterName = parameterName;
        DbType = parameterType;
        _size = size;
        _sourceColumn = sourceColumn ?? string.Empty;
        Direction = ParameterDirection.Input;
        SourceVersion = DataRowVersion.Current;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBParameter"/>.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to map.</param>
    /// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType"/> values.</param>
    /// <param name="size">The length of the parameter.</param>
    /// <param name="sourceColumn">The name of the source column.</param>
    /// <param name="direction">One of the <see cref="System.Data.ParameterDirection"/> values.</param>
    /// <param name="isNullable">
    /// <see langword="true"/> if the value of the field can be <see langword="null"/>, otherwise <see langword="false"/>.
    /// </param>
    /// <param name="precision">
    /// The total number of digits to the left and right of the decimal point to which <see cref="Value"/> is resolved.
    /// </param>
    /// <param name="scale">The total number of decimal places to which <see cref="Value"/> is resolved.</param>
    /// <param name="sourceVersion">One of the <see cref="System.Data.DataRowVersion"/> values.</param>
    /// <param name="value">An <see cref="object"/> that is the value of the <see cref="EDBParameter"/>.</param>
    public EDBParameter(string parameterName, EDBDbType parameterType, int size, string? sourceColumn,
        ParameterDirection direction, bool isNullable, byte precision, byte scale,
        DataRowVersion sourceVersion, object value)
    {
        ParameterName = parameterName;
        Size = size;
        _sourceColumn = sourceColumn ?? string.Empty;
        Direction = direction;
        IsNullable = isNullable;
        Precision = precision;
        Scale = scale;
        SourceVersion = sourceVersion;
        // ReSharper disable once VirtualMemberCallInConstructor
        Value = value;

        EDBDbType = parameterType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBParameter"/>.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to map.</param>
    /// <param name="parameterType">One of the <see cref="System.Data.DbType"/> values.</param>
    /// <param name="size">The length of the parameter.</param>
    /// <param name="sourceColumn">The name of the source column.</param>
    /// <param name="direction">One of the <see cref="System.Data.ParameterDirection"/> values.</param>
    /// <param name="isNullable">
    /// <see langword="true"/> if the value of the field can be <see langword="null"/>, otherwise <see langword="false"/>.
    /// </param>
    /// <param name="precision">
    /// The total number of digits to the left and right of the decimal point to which <see cref="Value"/> is resolved.
    /// </param>
    /// <param name="scale">The total number of decimal places to which <see cref="Value"/> is resolved.</param>
    /// <param name="sourceVersion">One of the <see cref="System.Data.DataRowVersion"/> values.</param>
    /// <param name="value">An <see cref="object"/> that is the value of the <see cref="EDBParameter"/>.</param>
    public EDBParameter(string parameterName, DbType parameterType, int size, string? sourceColumn,
        ParameterDirection direction, bool isNullable, byte precision, byte scale,
        DataRowVersion sourceVersion, object value)
    {
        ParameterName = parameterName;
        Size = size;
        _sourceColumn = sourceColumn ?? string.Empty;
        Direction = direction;
        IsNullable = isNullable;
        Precision = precision;
        Scale = scale;
        SourceVersion = sourceVersion;
        // ReSharper disable once VirtualMemberCallInConstructor
        Value = value;
        DbType = parameterType;
    }
    #endregion

    #region Name

    /// <summary>
    /// Gets or sets The name of the <see cref="EDBParameter"/>.
    /// </summary>
    /// <value>The name of the <see cref="EDBParameter"/>.
    /// The default is an empty string.</value>
    [AllowNull, DefaultValue("")]
    public sealed override string ParameterName
    {
        get => _name;
        set
        {
            if (Collection is not null)
                Collection.ChangeParameterName(this, value);
            else
                ChangeParameterName(value);
        }
    }

    internal void ChangeParameterName(string? value)
    {
        if (value == null)
            _name = TrimmedName = PositionalName;
        else if (value.Length > 0 && (value[0] == ':' || value[0] == '@'))
            TrimmedName = (_name = value).Substring(1);
        else
            _name = TrimmedName = value;
    }

    internal bool IsPositional => ParameterName.Length == 0;

    #endregion Name

    #region Value

    /// <inheritdoc />
    [TypeConverter(typeof(StringConverter)), Category("Data")]
    public override object? Value
    {
        get => _value;
        set
        {
            if (_value == null || value == null || _value.GetType() != value.GetType())
                Handler = null;
            _value = value;
            ConvertedValue = null;
        }
    }

    /// <summary>
    /// Gets or sets the value of the parameter.
    /// </summary>
    /// <value>
    /// An <see cref="object" /> that is the value of the parameter.
    /// The default value is <see langword="null" />.
    /// </value>
    [Category("Data")]
    [TypeConverter(typeof(StringConverter))]
    public object? EDBValue
    {
        get => Value;
        set => Value = value;
    }

    #endregion Value

    #region Type

    /// <summary>
    /// Gets or sets the <see cref="System.Data.DbType"/> of the parameter.
    /// </summary>
    /// <value>One of the <see cref="System.Data.DbType"/> values. The default is <see cref="object"/>.</value>
    [DefaultValue(DbType.Object)]
    [Category("Data"), RefreshProperties(RefreshProperties.All)]
    public sealed override DbType DbType
    {
        get
        {
            if (_npgsqlDbType.HasValue)
                return GlobalTypeMapper.EDBDbTypeToDbType(_npgsqlDbType.Value);

            if (_dataTypeName is not null)
                return GlobalTypeMapper.EDBDbTypeToDbType(GlobalTypeMapper.DataTypeNameToEDBDbType(_dataTypeName));

            if (Value is not null) // Infer from value but don't cache
            {
                return GlobalTypeMapper.Instance.TryResolveMappingByValue(Value, out var mapping)
                    ? mapping.DbType
                    : DbType.Object;
            }

            return DbType.Object;
        }
        set
        {
            Handler = null;
            _npgsqlDbType = value == DbType.Object
                ? null
                : GlobalTypeMapper.DbTypeToEDBDbType(value)
                  ?? throw new NotSupportedException($"The parameter type DbType.{value} isn't supported by PostgreSQL or EDB");
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="EDBTypes.EDBDbType"/> of the parameter.
    /// </summary>
    /// <value>One of the <see cref="EDBTypes.EDBDbType"/> values. The default is <see cref="EDBTypes.EDBDbType"/>.</value>
    [DefaultValue(EDBDbType.Unknown)]
    [Category("Data"), RefreshProperties(RefreshProperties.All)]
    [DbProviderSpecificTypeProperty(true)]
    public EDBDbType EDBDbType
    {
        [RequiresUnreferencedCode("The EDBDbType getter isn't trimming-safe")]
        get
        {
            if (_npgsqlDbType.HasValue)
                return _npgsqlDbType.Value;

            if (_dataTypeName is not null)
                return GlobalTypeMapper.DataTypeNameToEDBDbType(_dataTypeName);

            if (Value is not null) // Infer from value
            {
                return GlobalTypeMapper.Instance.TryResolveMappingByValue(Value, out var mapping)
                    ? mapping.EDBDbType ?? EDBDbType.Unknown
                    : throw new NotSupportedException("Can't infer EDBDbType for type " + Value.GetType());
            }

            return EDBDbType.Unknown;
        }
        set
        {
            if (value == EDBDbType.Array)
                throw new ArgumentOutOfRangeException(nameof(value), "Cannot set EDBDbType to just Array, Binary-Or with the element type (e.g. Array of Box is EDBDbType.Array | EDBDbType.Box).");
            if (value == EDBDbType.Range)
                throw new ArgumentOutOfRangeException(nameof(value), "Cannot set EDBDbType to just Range, Binary-Or with the element type (e.g. Range of integer is EDBDbType.Range | EDBDbType.Integer)");

            Handler = null;
            _npgsqlDbType = value;
        }
    }

    /// <summary>
    /// Used to specify which PostgreSQL type will be sent to the database for this parameter.
    /// </summary>
    public string? DataTypeName
    {
        get
        {
            if (_dataTypeName != null)
                return _dataTypeName;
            //else
            //return null;//EnterpriseDB Team

            if (_npgsqlDbType.HasValue)
                return GlobalTypeMapper.EDBDbTypeToDataTypeName(_npgsqlDbType.Value);

            if (Value != null) // Infer from value
            {
                return GlobalTypeMapper.Instance.TryResolveMappingByValue(Value, out var mapping)
                    ? mapping.DataTypeName
                    : null;
            }

            return null;
        }
        set
        {
            _dataTypeName = value;
            Handler = null;
        }
    }

    #endregion Type

    #region Other Properties

    /// <inheritdoc />
    public sealed override bool IsNullable { get; set; }

    /// <inheritdoc />
    [DefaultValue(ParameterDirection.Input)]
    [Category("Data")]
    public sealed override ParameterDirection Direction { get; set; }

#pragma warning disable CS0109
    /// <summary>
    /// Gets or sets the maximum number of digits used to represent the <see cref="Value"/> property.
    /// </summary>
    /// <value>
    /// The maximum number of digits used to represent the <see cref="Value"/> property.
    /// The default value is 0, which indicates that the data provider sets the precision for <see cref="Value"/>.</value>
    [DefaultValue((byte)0)]
    [Category("Data")]
    public new byte Precision
    {
        get => _precision;
        set
        {
            _precision = value;
            Handler = null;
        }
    }

    /// <summary>
    /// Gets or sets the number of decimal places to which <see cref="Value"/> is resolved.
    /// </summary>
    /// <value>The number of decimal places to which <see cref="Value"/> is resolved. The default is 0.</value>
    [DefaultValue((byte)0)]
    [Category("Data")]
    public new byte Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            Handler = null;
        }
    }
#pragma warning restore CS0109

    /// <inheritdoc />
    [DefaultValue(0)]
    [Category("Data")]
    public sealed override int Size
    {
        get => _size;
        set
        {
            if (value < -1)
                throw new ArgumentException($"Invalid parameter Size value '{value}'. The value must be greater than or equal to 0.");

            _size = value;
            Handler = null;
        }
    }

    /// <inheritdoc />
    [AllowNull, DefaultValue("")]
    [Category("Data")]
    public sealed override string SourceColumn
    {
        get => _sourceColumn;
        set => _sourceColumn = value ?? string.Empty;
    }

    /// <inheritdoc />
    [Category("Data"), DefaultValue(DataRowVersion.Current)]
    public sealed override DataRowVersion SourceVersion { get; set; }

    /// <inheritdoc />
    public sealed override bool SourceColumnNullMapping { get; set; }

#pragma warning disable CA2227
    /// <summary>
    /// The collection to which this parameter belongs, if any.
    /// </summary>
    public EDBParameterCollection? Collection { get; set; }
#pragma warning restore CA2227

    /// <summary>
    /// The PostgreSQL data type, such as int4 or text, as discovered from pg_type.
    /// This property is automatically set if parameters have been derived via
    /// <see cref="EDBCommandBuilder.DeriveParameters"/> and can be used to
    /// acquire additional information about the parameters' data type.
    /// </summary>
    public PostgresType? PostgresType { get; internal set; }

    #endregion Other Properties

    #region Internals

    internal virtual void ResolveHandler(TypeMapper typeMapper)
    {
        Handler ??= GetResolvedHandler(typeMapper);

        if (Handler is not null)
            return;

        var parameterName = !string.IsNullOrEmpty(ParameterName) ? ParameterName : $"${Collection?.IndexOf(this) + 1}";
        throw new InvalidOperationException($"Parameter '{parameterName}' must have either its EDBDbType or its DataTypeName or its Value set");
    }

    internal EDBTypeHandler? GetResolvedHandler(TypeMapper typeMapper)
    {
        if (Handler is not null)
            return Handler;

        if (_npgsqlDbType.HasValue)
            return typeMapper.ResolveByEDBDbType(_npgsqlDbType.Value);
        else if (_dataTypeName is not null)
            return typeMapper.ResolveByDataTypeName(_dataTypeName);
        else if (_value is not null)
            return typeMapper.ResolveByValue(_value);
        else
            return null;
    }

    internal void Bind(TypeMapper typeMapper)
    {
        ResolveHandler(typeMapper);
        FormatCode = Handler!.PreferTextWrite ? FormatCode.Text : FormatCode.Binary;
    }

    internal void TryBind(TypeMapper typeMapper)
    {
        Handler = GetResolvedHandler(typeMapper);
        if (Handler is not null)
        {
            FormatCode = Handler!.PreferTextWrite ? FormatCode.Text : FormatCode.Binary;
        }
    }

    internal virtual int ValidateAndGetLength()
    {
        if (Direction == ParameterDirection.Input || Direction == ParameterDirection.InputOutput)//EnterpriseDB Team
            if (_value is DBNull)
                return 0;
        if (_value == null)
            throw new InvalidCastException($"Parameter {ParameterName} must be set");

        var lengthCache = LengthCache;
        var len = Handler!.ValidateObjectAndGetLength(_value, ref lengthCache, this);
        LengthCache = lengthCache;
        return len;
    }

    internal virtual Task WriteWithLength(EDBWriteBuffer buf, bool async, CancellationToken cancellationToken = default)
        => Handler!.WriteObjectWithLength(_value!, buf, LengthCache, this, async, cancellationToken);

    /// <inheritdoc />
    public override void ResetDbType()
    {
        _npgsqlDbType = null;
        _dataTypeName = null;
        Handler = null;
    }

    ///<summary>
    /// Get param direction
    /// </summary>
    public enum EDBParameterDirection //EnterpriseDB Team
    {
        /// <summary>
        /// unknown, Input 
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 
        /// </summary>
        Input = 1,
        /// <summary>
        /// /
        /// </summary>
        Output = 2,
        /// <summary>
        /// /
        /// </summary>
        InputOutput = 3
    }

    /// <summary>
    /// Get EDB param direction
    /// </summary>

    internal static EDBParameterDirection NetParamDirectionToEDBParamDirection(ParameterDirection direction)//EnterpriseDB Team
    {
        switch (direction)
        {
        case ParameterDirection.Input:
            return EDBParameterDirection.Input;
        case ParameterDirection.Output:
            return EDBParameterDirection.Output;
        case ParameterDirection.InputOutput:
            return EDBParameterDirection.InputOutput;
        default:
            return EDBParameterDirection.Unknown;
        }
    }


    /// <summary>
    /// Get param OID
    /// </summary>
    public enum EDBParameterOID //EnterpriseDB Team
    {
        /// <summary>
        /// 
        /// </summary>
        Int2 = 21,
        /// <summary>
        /// 
        /// </summary>
        Int4 = 23,
        /// <summary>
        /// 
        /// </summary>
        Int8 = 20,
        /// <summary>
        /// 
        /// </summary>
        Varchar = 1043,
        /// <summary>
        /// 
        /// </summary>
        Text = 25,
        /// <summary>
        /// 
        /// </summary>
        Boolean = 16,
        /// <summary>
        /// 
        /// </summary>
        Numeric = 1700,
        /// <summary>
        /// 
        /// </summary>
        Date = 1082,
        //DateTime on server
        /// <summary>
        /// 
        /// </summary>
        Time = 1083,
        /// <summary>
        /// 
        /// </summary>
        Timestamp = 1114,
        /// <summary>
        /// 
        /// </summary>
        Float4 = 700,
        /// <summary>
        /// 
        /// </summary>
        Float8 = 701,
        /// <summary>
        /// 
        /// </summary>
        Bytea = 17,
        /// <summary>
        /// 
        /// </summary>
        Varchar2 = 1043,
        /// <summary>
        /// 
        /// </summary>
        Datetime = 1082,
        /// <summary>
        /// 
        /// </summary>
        Currency = 790,    //PG compatible Money Type
        /// <summary>
        /// 
        /// </summary>
        Char = 1042,
        /// <summary>
        /// 
        /// </summary>
        Refcursor = 1790,
        /// <summary>
        /// 
        /// </summary>
        Int2Array = 1005,
        /// <summary>
        /// 
        /// </summary>
        Int4Array = 1007,
        /// <summary>
        /// 
        /// </summary>
        Int8Array = 1016,
        /// <summary>
        /// 
        /// </summary>
        Float4Array = 1021,
        /// <summary>
        /// 
        /// </summary>
        Float8Array = 1022,
        /// <summary>
        /// 
        /// </summary>
        CharArray = 1002,
        /// <summary>
        /// 
        /// </summary>
        BooleanArray = 1000,
        /// <summary>
        /// 
        /// </summary>
        StringArray = 1015,
        /// <summary>
        /// 
        /// </summary>
        Box = 603,
        /// <summary>
        /// 
        /// </summary>
        Circle = 718,
        /// <summary>
        /// 
        /// </summary>
        LSeg = 601,
        /// <summary>
        /// 
        /// </summary>
        Path = 602,
        /// <summary>
        /// 
        /// </summary>
        Point = 600,
        /// <summary>
        /// 
        /// </summary>
        Polygon = 604,
        // Refcursor = 1790,
        /// <summary>
        /// 
        /// </summary>
        Unknown = 0
    }


    /// <summary>
    /// Get param to OID
    /// </summary>
    internal static EDBParameterOID ParamToOid(string param_name)//EnterpriseDB Team
    {
        /* EDB Team
         * Function Returns OID of datatype
         * EnterpriseDB: Check the param name after converting it to lower case.
         * Change all the case names to lower case.
        */
        switch (param_name.ToLower())
        {
        case "int4":
        case "integer":
            return EDBParameterOID.Int4;
        case "varchar":
            return EDBParameterOID.Varchar;
        case "text":
            return EDBParameterOID.Text;
        case "bool":   /*	Fix F#2083 25-Jan-06	*/
            return EDBParameterOID.Boolean;
        case "numeric":
            return EDBParameterOID.Numeric;
        case "date":
            /*
             * Changed the OID of DATE to DATETIME as DATE datatype is not
             * supported on server side, but DATE is
             * being converted to DATETIME internally.
             */
            return EDBParameterOID.Date;
        case "time":
            return EDBParameterOID.Time;
        //EnterpriseDB:Type mismatch because of the case sensitivity in Timestamp. We change "Timestamp" to "timestamp".
        case "timestamp":
            return EDBParameterOID.Timestamp;
        case "float4":
            return EDBParameterOID.Float4;
        /*  
         * If parameter name is bytea, then return the OID of bytea (17) for EnterpriseDB Callable statement
         */
        case "bytea":
            return EDBParameterOID.Bytea;
        case "varchar2": /* 17 OCT 05.New support of varchar2     F#1185 */
            return EDBParameterOID.Varchar2;
        case "datetime":
            return EDBParameterOID.Datetime; /*	19 OCT 05.New Support of datetime:  F #1185 */
        /* EnterpriseDB Team : 28 DEC Support of Smallint: */
        case "int2":
            return EDBParameterOID.Int2;
        /*EnterpriseDB Team : 28 DEC Support of BigInt:*/
        case "int8":
            return EDBParameterOID.Int8;
        /* EnterpriseDB Team :F#2090.	*/
        case "currency":
            return EDBParameterOID.Currency;
        case "char":
            return EDBParameterOID.Char;
        /* Support of RefCursor */
        case "refcursor":
            return EDBParameterOID.Refcursor;
        /*
         * Array types and other missing
         */
        case "float8":
            return EDBParameterOID.Float8;
        case "_float8":
            return EDBParameterOID.Float8Array;
        case "_int4":
            return EDBParameterOID.Int4Array;
        case "_float4":
            return EDBParameterOID.Float4Array;
        case "_char":
            return EDBParameterOID.CharArray;
        case "_bool":
            return EDBParameterOID.BooleanArray;
        case "box":
            return EDBParameterOID.Box;
        case "circle":
            return EDBParameterOID.Circle;
        case "_int2":
            return EDBParameterOID.Int2Array;
        case "_int8":
            return EDBParameterOID.Int8Array;
        case "lseg":
            return EDBParameterOID.LSeg;
        case "path":
            return EDBParameterOID.Path;
        case "point":
            return EDBParameterOID.Point;
        case "polygon":
            return EDBParameterOID.Polygon;
        case "_varchar":
            return EDBParameterOID.StringArray;
        /*   
        * If OID does not exist/match then return 0 which indicates server to lookup for OID
        */
        default:
            return EDBParameterOID.Unknown;
        }
    }

    internal bool IsInputDirection => Direction == ParameterDirection.InputOutput || Direction == ParameterDirection.Input;

    internal bool IsOutputDirection => Direction == ParameterDirection.InputOutput || Direction == ParameterDirection.Output;

    /* EnterpriseDB Team */
    internal bool IsOutReturnDirection
    {
        get { return Direction == ParameterDirection.InputOutput || Direction == ParameterDirection.Output || Direction == ParameterDirection.ReturnValue; }
    }
    #endregion

    #region Clone

    /// <summary>
    /// Creates a new <see cref="EDBParameter"/> that is a copy of the current instance.
    /// </summary>
    /// <returns>A new <see cref="EDBParameter"/> that is a copy of this instance.</returns>
    public EDBParameter Clone() => CloneCore();

    private protected virtual EDBParameter CloneCore() =>
        // use fields instead of properties
        // to avoid auto-initializing something like type_info
        new()
        {
            _precision = _precision,
            _scale = _scale,
            _size = _size,
            _npgsqlDbType = _npgsqlDbType,
            _dataTypeName = _dataTypeName,
            Direction = Direction,
            IsNullable = IsNullable,
            _name = _name,
            TrimmedName = TrimmedName,
            SourceColumn = SourceColumn,
            SourceVersion = SourceVersion,
            _value = _value,
            SourceColumnNullMapping = SourceColumnNullMapping,
        };

    object ICloneable.Clone() => Clone();

    #endregion
}
