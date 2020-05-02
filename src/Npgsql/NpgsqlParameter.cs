using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EnterpriseDB.EDBClient.Util;
using EDBTypes;

namespace EnterpriseDB.EDBClient{
    ///<summary>
    /// This class represents a parameter to a command that will be sent to server
    ///</summary>
    public class EDBParameter : DbParameter, IDbDataParameter, ICloneable
    {
        #region Fields and Properties

        byte _precision;
        byte _scale;
        int _size;

        // ReSharper disable InconsistentNaming
        private protected EDBDbType? _EDBDbType;
        private protected string? _dataTypeName;
        // ReSharper restore InconsistentNaming

        DbType? _cachedDbType;
        string _name = string.Empty;
        object? _value;

        internal string TrimmedName { get; private set; } = string.Empty;

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
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see> class.
        /// </summary>
        public EDBParameter()
        {
            SourceColumn = string.Empty;
            Direction = ParameterDirection.Input;
            SourceVersion = DataRowVersion.Current;
        }

#nullable disable
        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>
        /// class with the parameter name and a value of the new <b>EDBParameter</b>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="value">An <see cref="System.Object">Object</see> that is the value of the <see cref="EDBParameter">EDBParameter</see>.</param>
        /// <remarks>
        /// <p>When you specify an <see cref="System.Object">Object</see>
        /// in the value parameter, the <see cref="System.Data.DbType">DbType</see> is
        /// inferred from the .NET Framework type of the <b>Object</b>.</p>
        /// <p>When using this constructor, you must be aware of a possible misuse of the constructor which takes a DbType parameter.
        /// This happens when calling this constructor passing an int 0 and the compiler thinks you are passing a value of DbType.
        /// Use <code> Convert.ToInt32(value) </code> for example to have compiler calling the correct constructor.</p>
        /// </remarks>
        public EDBParameter(string parameterName, object value) : this()
        {
            ParameterName = parameterName;
            // ReSharper disable once VirtualMemberCallInConstructor
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>
        /// class with the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        public EDBParameter(string parameterName, EDBDbType parameterType)
            : this(parameterName, parameterType, 0, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        public EDBParameter(string parameterName, DbType parameterType)
            : this(parameterName, parameterType, 0, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType">EDBDbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        public EDBParameter(string parameterName, EDBDbType parameterType, int size)
            : this(parameterName, parameterType, size, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        public EDBParameter(string parameterName, DbType parameterType, int size)
            : this(parameterName, parameterType, size, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType">EDBDbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        public EDBParameter(string parameterName, EDBDbType parameterType, int size, string sourceColumn)
            : this()
        {
            ParameterName = parameterName;
            EDBDbType = parameterType;
            _size = size;
            SourceColumn = sourceColumn;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        public EDBParameter(string parameterName, DbType parameterType, int size, string sourceColumn)
            : this()
        {
            ParameterName = parameterName;
            DbType = parameterType;
            _size = size;
            SourceColumn = sourceColumn;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType">EDBDbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <param name="direction">One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see> values.</param>
        /// <param name="isNullable"><b>true</b> if the value of the field can be null, otherwise <b>false</b>.</param>
        /// <param name="precision">The total number of digits to the left and right of the decimal point to which
        /// <see cref="EDBParameter.Value">Value</see> is resolved.</param>
        /// <param name="scale">The total number of decimal places to which
        /// <see cref="EDBParameter.Value">Value</see> is resolved.</param>
        /// <param name="sourceVersion">One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.</param>
        /// <param name="value">An <see cref="System.Object">Object</see> that is the value
        /// of the <see cref="EDBParameter">EDBParameter</see>.</param>
        public EDBParameter(string parameterName, EDBDbType parameterType, int size, string sourceColumn,
                               ParameterDirection direction, bool isNullable, byte precision, byte scale,
                               DataRowVersion sourceVersion, object value)
            : this()
        {
            ParameterName = parameterName;
            Size = size;
            SourceColumn = sourceColumn;
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
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <param name="direction">One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see> values.</param>
        /// <param name="isNullable"><b>true</b> if the value of the field can be null, otherwise <b>false</b>.</param>
        /// <param name="precision">The total number of digits to the left and right of the decimal point to which
        /// <see cref="EDBParameter.Value">Value</see> is resolved.</param>
        /// <param name="scale">The total number of decimal places to which
        /// <see cref="EDBParameter.Value">Value</see> is resolved.</param>
        /// <param name="sourceVersion">One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.</param>
        /// <param name="value">An <see cref="System.Object">Object</see> that is the value
        /// of the <see cref="EDBParameter">EDBParameter</see>.</param>
        public EDBParameter(string parameterName, DbType parameterType, int size, string sourceColumn,
                               ParameterDirection direction, bool isNullable, byte precision, byte scale,
                               DataRowVersion sourceVersion, object value)
            : this()
        {
            ParameterName = parameterName;
            Size = size;
            SourceColumn = sourceColumn;
            Direction = direction;
            IsNullable = isNullable;
            Precision = precision;
            Scale = scale;
            SourceVersion = sourceVersion;
            // ReSharper disable once VirtualMemberCallInConstructor
            Value = value;
            DbType = parameterType;
        }
#nullable restore
        #endregion

        #region Name

        /// <summary>
        /// Gets or sets The name of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <value>The name of the <see cref="EDBParameter">EDBParameter</see>.
        /// The default is an empty string.</value>
        [DefaultValue("")]
#nullable disable
        public sealed override string ParameterName
#nullable restore
        {
            get => _name;
            set
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (value == null)
                    _name = TrimmedName = string.Empty;
                else if (value.Length > 0 && (value[0] == ':' || value[0] == '@'))
                    TrimmedName = (_name = value).Substring(1);
                else
                    _name = TrimmedName = value;

                Collection?.InvalidateHashLookups();
            }
        }

        #endregion Name

        #region Value

        /// <inheritdoc />
        [TypeConverter(typeof(StringConverter)), Category("Data")]
#nullable disable
        public override object Value
#nullable restore
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
        /// <value>An <see cref="System.Object">Object</see> that is the value of the parameter.
        /// The default value is null.</value>
        [Category("Data")]
        [TypeConverter(typeof(StringConverter))]
#nullable disable
        public object EDBValue
#nullable restore
        {
            get => Value;
            set => Value = value;
        }

        #endregion Value

        #region Type

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DbType">DbType</see> of the parameter.
        /// </summary>
        /// <value>One of the <see cref="System.Data.DbType">DbType</see> values. The default is <b>Object</b>.</value>
        [DefaultValue(DbType.Object)]
        [Category("Data"), RefreshProperties(RefreshProperties.All)]
        public sealed override DbType DbType
        {
            get
            {
                if (_cachedDbType.HasValue)
                    return _cachedDbType.Value;
                if (_EDBDbType.HasValue)
                    return _cachedDbType ??= GlobalTypeMapper.Instance.ToDbType(_EDBDbType.Value);
                if (_value != null)   // Infer from value but don't cache
                    return GlobalTypeMapper.Instance.ToDbType(_value.GetType());

                return DbType.Object;
            }
            set
            {
                Handler = null;
                if (value == DbType.Object)
                {
                    _cachedDbType = null;
                    _EDBDbType = null;
                }
                else
                {
                    _cachedDbType = value;
                    _EDBDbType = GlobalTypeMapper.Instance.ToEDBDbType(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="EDBTypes.EDBDbType">EDBDbType</see> of the parameter.
        /// </summary>
        /// <value>One of the <see cref="EDBTypes.EDBDbType">EDBDbType</see> values. The default is <b>Unknown</b>.</value>
        [DefaultValue(EDBDbType.Unknown)]
        [Category("Data"), RefreshProperties(RefreshProperties.All)]
        [DbProviderSpecificTypeProperty(true)]
        public EDBDbType EDBDbType
        {
            get
            {
                if (_EDBDbType.HasValue)
                    return _EDBDbType.Value;
                if (_value != null)   // Infer from value
                    return GlobalTypeMapper.Instance.ToEDBDbType(_value.GetType());
                return EDBDbType.Unknown;
            }
            set
            {
                if (value == EDBDbType.Array)
                    throw new ArgumentOutOfRangeException(nameof(value), "Cannot set EDBDbType to just Array, Binary-Or with the element type (e.g. Array of Box is EDBDbType.Array | EDBDbType.Box).");
                if (value == EDBDbType.Range)
                    throw new ArgumentOutOfRangeException(nameof(value), "Cannot set EDBDbType to just Range, Binary-Or with the element type (e.g. Range of integer is EDBDbType.Range | EDBDbType.Integer)");

                Handler = null;
                _EDBDbType = value;
                _cachedDbType = null;
            }
        }

        /// <summary>
        /// Used to specify which PostgreSQL type will be sent to the database for this parameter.
        /// </summary>
        [PublicAPI]
        public string? DataTypeName
        {
            get
            {
                if (_dataTypeName != null)
                    return _dataTypeName;
                else
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
        /// Gets or sets the maximum number of digits used to represent the
        /// <see cref="EDBParameter.Value">Value</see> property.
        /// </summary>
        /// <value>The maximum number of digits used to represent the
        /// <see cref="EDBParameter.Value">Value</see> property.
        /// The default value is 0, which indicates that the data provider
        /// sets the precision for <b>Value</b>.</value>
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
        /// Gets or sets the number of decimal places to which
        /// <see cref="EDBParameter.Value">Value</see> is resolved.
        /// </summary>
        /// <value>The number of decimal places to which
        /// <see cref="EDBParameter.Value">Value</see> is resolved. The default is 0.</value>
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
        [DefaultValue("")]
        [Category("Data")]
        public sealed override string? SourceColumn { get; set; }

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

        internal virtual void ResolveHandler(ConnectorTypeMapper typeMapper)
        {
            if (Handler != null)
                return;

            if (_EDBDbType.HasValue)
                Handler = typeMapper.GetByEDBDbType(_EDBDbType.Value);
            else if (_dataTypeName != null)
                Handler = typeMapper.GetByDataTypeName(_dataTypeName);
            else if (_value != null)
                Handler = typeMapper.GetByClrType(_value.GetType());
            else
                throw new InvalidOperationException($"Parameter '{ParameterName}' must have its value set");
        }

        internal void Bind(ConnectorTypeMapper typeMapper)
        {
            ResolveHandler(typeMapper);
            FormatCode = Handler!.PreferTextWrite ? FormatCode.Text : FormatCode.Binary;
        }

        internal virtual int ValidateAndGetLength()
        {
            if (Direction == ParameterDirection.Input)//EnterpriseDB Team
                if (_value == null)
                throw new InvalidCastException($"Parameter {ParameterName} must be set");
            if (_value is DBNull)
                return 0;

            var lengthCache = LengthCache;
#pragma warning disable CS8604 // Possible null reference argument.
            var len = Handler!.ValidateObjectAndGetLength(_value, ref lengthCache, this);
#pragma warning restore CS8604 // Possible null reference argument.
            LengthCache = lengthCache;
            return len;
        }

        internal virtual Task WriteWithLength(EDBWriteBuffer buf, bool async)
            => Handler!.WriteObjectWithLength(_value!, buf, LengthCache, this, async);

        /// <inheritdoc />
        public override void ResetDbType()
        {
            _cachedDbType = null;
            _EDBDbType = null;
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

        public static EDBParameterDirection NetParamDirectionToEDBParamDirection(ParameterDirection direction)//EnterpriseDB Team
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
        public static EDBParameterOID ParamToOid(string param_name)//EnterpriseDB Team
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
        /// Creates a new <see cref="EDBParameter">EDBParameter</see> that
        /// is a copy of the current instance.
        /// </summary>
        /// <returns>A new <see cref="EDBParameter">EDBParameter</see> that is a copy of this instance.</returns>
        public EDBParameter Clone()
        {
            // use fields instead of properties
            // to avoid auto-initializing something like type_info
            var clone = new EDBParameter
            {
                _precision = _precision,
                _scale = _scale,
                _size = _size,
                _cachedDbType = _cachedDbType,
                _EDBDbType = _EDBDbType,
                Direction = Direction,
                IsNullable = IsNullable,
                _name = _name,
                TrimmedName = TrimmedName,
                SourceColumn = SourceColumn,
                SourceVersion = SourceVersion,
                _value = _value,
                SourceColumnNullMapping = SourceColumnNullMapping,
            };
            return clone;
        }

        object ICloneable.Clone() => Clone();

        #endregion
    }
}
