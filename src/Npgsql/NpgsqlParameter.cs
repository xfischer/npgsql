#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The  EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Reflection;
using EDBTypes;

#if WITHDESIGN
using  EnterpriseDB.EDBClient.Design;
#endif

namespace  EnterpriseDB.EDBClient
{
    ///<summary>
    /// This class represents a parameter to a command that will be sent to server
    ///</summary>
#if WITHDESIGN
    [TypeConverter(typeof(EDBParameterConverter))]
#endif
#if DNXCORE50
    public sealed class EDBParameter : DbParameter
#else
    public sealed class EDBParameter : DbParameter, ICloneable
#endif
    {
        #region Fields and Properties

        // Fields to implement IDbDataParameter interface.
        byte _precision;
        byte _scale;
        int _size;

        // Fields to implement IDataParameter
        EDBDbType? _EDBDbType;
        DbType? _dbType;
        Type _enumType;
        string _name = String.Empty;
        object _value;
        object _EDBValue;

        /// <summary>
        /// Can be used to communicate a value from the validation phase to the writing phase.
        /// </summary>
        internal object ConvertedValue { get; set; }

        EDBParameterCollection _collection;
        internal LengthCache LengthCache { get; private set; }

        internal bool IsBound { get; private set; }
        internal TypeHandler Handler { get; private set; }
        internal FormatCode FormatCode { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see> class.
        /// </summary>
        public EDBParameter()
        {
            SourceColumn = String.Empty;
            Direction = ParameterDirection.Input;
#if !DNXCORE50
            SourceVersion = DataRowVersion.Current;
#endif
        }

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
        public EDBParameter(String parameterName, object value) : this()
        {
            ParameterName = parameterName;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>
        /// class with the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        public EDBParameter(string parameterName, EDBDbType parameterType)
            : this(parameterName, parameterType, 0, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        public EDBParameter(string parameterName, DbType parameterType)
            : this(parameterName, parameterType, 0, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType">EDBDbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        public EDBParameter(string parameterName, EDBDbType parameterType, int size)
            : this(parameterName, parameterType, size, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        public EDBParameter(string parameterName, DbType parameterType, int size)
            : this(parameterName, parameterType, size, String.Empty)
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

#if !DNXCORE50
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
            Value = value;

            DbType = parameterType;
        }
#endif

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>An <see cref="System.Object">Object</see> that is the value of the parameter.
        /// The default value is null.</value>
#if !DNXCORE50
        [TypeConverter(typeof(StringConverter)), Category("Data")]
#endif
        public override object Value
        {
            get
            {
                return _value;
            } // [TODO] Check and validate data type.
            set
            {
                ClearBind();
                _value = value;
                _EDBValue = value;
                ConvertedValue = null;
            }
        }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>An <see cref="System.Object">Object</see> that is the value of the parameter.
        /// The default value is null.</value>
#if !DNXCORE50
        [Category("Data")]
#endif
        [TypeConverter(typeof(StringConverter))]
        public object EDBValue
        {
            get { return _EDBValue; }
            set {
                ClearBind();
                _value = value;
                _EDBValue = value;
                ConvertedValue = null;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parameter accepts null values.
        /// </summary>
        public override bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is input-only,
        /// output-only, bidirectional, or a stored procedure return value parameter.
        /// </summary>
        /// <value>One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see>
        /// values. The default is <b>Input</b>.</value>
        [DefaultValue(ParameterDirection.Input)]
#if !DNXCORE50
        [Category("Data")]
#endif
        public override ParameterDirection Direction { get; set; }

        // Implementation of IDbDataParameter
        /// <summary>
        /// Gets or sets the maximum number of digits used to represent the
        /// <see cref="EDBParameter.Value">Value</see> property.
        /// </summary>
        /// <value>The maximum number of digits used to represent the
        /// <see cref="EDBParameter.Value">Value</see> property.
        /// The default value is 0, which indicates that the data provider
        /// sets the precision for <b>Value</b>.</value>
        [DefaultValue((Byte)0)]
#if !DNXCORE50
        [Category("Data")]
#endif
        public byte Precision
        {
            get { return _precision; }
            set
            {
                _precision = value;
                ClearBind();
            }
        }

        /// <summary>
        /// Gets or sets the number of decimal places to which
        /// <see cref="EDBParameter.Value">Value</see> is resolved.
        /// </summary>
        /// <value>The number of decimal places to which
        /// <see cref="EDBParameter.Value">Value</see> is resolved. The default is 0.</value>
        [DefaultValue((Byte)0)]
#if !DNXCORE50
        [Category("Data")]
#endif
        public byte Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                ClearBind();
            }
        }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of the data within the column.
        /// </summary>
        /// <value>The maximum size, in bytes, of the data within the column.
        /// The default value is inferred from the parameter value.</value>
        [DefaultValue(0)]
#if !DNXCORE50
        [Category("Data")]
#endif
        public override int Size
        {
            get { return _size; }
            set
            {
                if (value < -1)
                    throw new ArgumentException(String.Format("Invalid parameter Size value '{0}'. The value must be greater than or equal to 0.", value));
                Contract.EndContractBlock();

                _size = value;
                ClearBind();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DbType">DbType</see> of the parameter.
        /// </summary>
        /// <value>One of the <see cref="System.Data.DbType">DbType</see> values. The default is <b>Object</b>.</value>
        [DefaultValue(DbType.Object)]
#if !DNXCORE50
        [Category("Data"), RefreshProperties(RefreshProperties.All)]
#endif
        public override DbType DbType
        {
            get
            {
                if (_dbType.HasValue) {
                    return _dbType.Value;
                }

                if (_value != null) {   // Infer from value
                    return TypeHandlerRegistry.ToDbType(_value.GetType());
                }

                return DbType.Object;
            }
            set
            {
                ClearBind();
                if (value == DbType.Object)
                {
                    _dbType = null;
                    _EDBDbType = null;
                }
                else
                {
                    _dbType = value;
                    _EDBDbType = TypeHandlerRegistry.ToEDBDbType(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="EDBTypes.EDBDbType">EDBDbType</see> of the parameter.
        /// </summary>
        /// <value>One of the <see cref="EDBTypes.EDBDbType">EDBDbType</see> values. The default is <b>Unknown</b>.</value>
        [DefaultValue(EDBDbType.Unknown)]
#if !DNXCORE50
        [Category("Data"), RefreshProperties(RefreshProperties.All)]
#endif
        public EDBDbType EDBDbType
        {
            get
            {
                if (_EDBDbType.HasValue) {
                    return _EDBDbType.Value;
                }

                if (_value != null) {   // Infer from value
                    return TypeHandlerRegistry.ToEDBDbType(_value.GetType());
                }

                return EDBDbType.Unknown;
            }
            set
            {
                if (value == EDBDbType.Array) {
                    throw new ArgumentOutOfRangeException("value", "Cannot set EDBDbType to just Array, Binary-Or with the element type (e.g. Array of Box is EDBDbType.Array | EDBDbType.Box).");
                }
                if (value == EDBDbType.Range) {
                    throw new ArgumentOutOfRangeException("value", "Cannot set EDBDbType to just Range, Binary-Or with the element type (e.g. Range of integer is EDBDbType.Range | EDBDbType.Integer)");
                }
                Contract.EndContractBlock();

                ClearBind();
                _EDBDbType = value;
                _dbType = TypeHandlerRegistry.ToDbType(value);
            }
        }

        /// <summary>
        /// Gets or sets The name of the <see cref="EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <value>The name of the <see cref="EDBParameter">EDBParameter</see>.
        /// The default is an empty string.</value>
        [DefaultValue("")]
        public override string ParameterName
        {
            get { return _name; }
            set
            {
                _name = value;
                if (value == null)
                {
                    _name = String.Empty;
                }
                // no longer prefix with : so that The name returned is The name set

                _name = _name.Trim();

                if (_collection != null)
                {
                    _collection.InvalidateHashLookups();
                    ClearBind();
                }
            }
        }

                /// <summary>
        /// Gets or sets The name of the source column that is mapped to the
        /// <see cref="System.Data.DataSet">DataSet</see> and used for loading or
        /// returning the <see cref="EDBParameter.Value">Value</see>.
        /// </summary>
        /// <value>The name of the source column that is mapped to the
        /// <see cref="System.Data.DataSet">DataSet</see>. The default is an empty string.</value>
        [DefaultValue("")]
#if !DNXCORE50
        [Category("Data")]
#endif
        public override String SourceColumn { get; set; }

#if !DNXCORE50
        /// <summary>
        /// Gets or sets the <see cref="System.Data.DataRowVersion">DataRowVersion</see>
        /// to use when loading <see cref="EDBParameter.Value">Value</see>.
        /// </summary>
        /// <value>One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.
        /// The default is <b>Current</b>.</value>
        [Category("Data"), DefaultValue(DataRowVersion.Current)]
        public override DataRowVersion SourceVersion { get; set; }
#endif

        /// <summary>
        /// Source column mapping.
        /// </summary>
        public override bool SourceColumnNullMapping { get; set; }

        /// <summary>
        /// Used in combination with EDBDbType.Enum or EDBDbType.Array | EDBDbType.Enum to indicate the enum type.
        /// For other EDBDbTypes, this field is not used.
        /// </summary>
        public Type EnumType
        {
            get
            {
                if (_enumType != null)
                    return _enumType;

                // Try to infer type if EDBDbType is Enum or has not been set
                if ((!_EDBDbType.HasValue || _EDBDbType == EDBDbType.Enum) && _value != null)
                {
                    var type = _value.GetType();
                    if (type.GetTypeInfo().IsEnum)
                        return type;
                    if (type.IsArray && type.GetElementType().GetTypeInfo().IsEnum)
                        return type.GetElementType();
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    if (!value.GetTypeInfo().IsEnum)
                        throw new ArgumentException("The type is not an enum type", "value");
                    _enumType = value;
                }
            }
        }

        /// <summary>
        /// The collection to which this parameter belongs, if any.
        /// </summary>
        public EDBParameterCollection Collection
        {
            get { return _collection; }

            internal set
            {
                _collection = value;
                ClearBind();
            }
        }

        #endregion

        #region Internals

        /// <summary>
        /// The name scrubbed of any optional marker
        /// </summary>
        internal string CleanName
        {
            get
            {
                string name = ParameterName;
                if (name[0] == ':' || name[0] == '@')
                {
                    return name.Length > 1 ? name.Substring(1) : string.Empty;
                }
                return name;

            }
        }

        /// <summary>
        /// Returns whether this parameter has had its type set explicitly via DbType or EDBDbType
        /// (and not via type inference)
        /// </summary>
        internal bool IsTypeExplicitlySet
        {
            get { return _EDBDbType.HasValue || _dbType.HasValue; }
        }

        internal void ResolveHandler(TypeHandlerRegistry registry)
        {
            if (Handler != null) {
                return;
            }

            if (_EDBDbType.HasValue)
            {
                Handler = registry[_EDBDbType.Value, EnumType];
            }
            else if (_dbType.HasValue)
            {
                Handler = registry[_dbType.Value];
            }
            else if (_value != null)
            {
                Handler = registry[_value];
            }
            else
            {
                throw new InvalidOperationException(string.Format("Parameter '{0}' must have its value set", ParameterName));
            }
        }

        internal void Bind(TypeHandlerRegistry registry)
        {
            ResolveHandler(registry);

          //ZK  Contract.Assert(Handler != null);
            FormatCode = Handler.PreferTextWrite ? FormatCode.Text : FormatCode.Binary;

            IsBound = true;
        }

        internal int ValidateAndGetLength()
        {
            if (Direction == ParameterDirection.Input)
            {
                if (_value == null)
                {
                    throw new InvalidCastException(string.Format("Parameter {0} must be set", ParameterName));
                }
            }
            if (_value is DBNull) {
                return 0;
            }

            // No length caching for simple types
            var asSimpleWriter = Handler as ISimpleTypeWriter;
            if (asSimpleWriter != null) {
                return asSimpleWriter.ValidateAndGetLength(Value, this);
            }

            var asChunkingWriter = Handler as IChunkingTypeWriter;
            Contract.Assert(asChunkingWriter != null, String.Format("Handler {0} doesn't implement either ISimpleTypeWriter or IChunkingTypeWriter", Handler.GetType().Name));
            var lengthCache = LengthCache;
            var len = asChunkingWriter.ValidateAndGetLength(Value, ref lengthCache, this);
            LengthCache = lengthCache;
            return len;
        }

        void ClearBind()
        {
            IsBound = false;
            Handler = null;
        }

        /// <summary>
        /// Reset DBType.
        /// </summary>
        public override void ResetDbType()
        {
            //type_info = EDBTypesHelper.GetNativeTypeInfo(typeof(String));
            _dbType = null;
            _EDBDbType = null;
            Value = Value;
            ClearBind();
        }

        internal bool IsInputDirection
        {
            get { return Direction == ParameterDirection.InputOutput || Direction == ParameterDirection.Input; }
        }

        internal bool IsOutputDirection
        {
            get { return Direction == ParameterDirection.InputOutput || Direction == ParameterDirection.Output; }
        }

        internal bool IsReturnDirection
        {
            get { return Direction == ParameterDirection.ReturnValue; }
        }
        

        internal bool IsOutReturnDirection
        {
            get { return Direction == ParameterDirection.InputOutput || Direction == ParameterDirection.Output || Direction == ParameterDirection.ReturnValue; }
        }
        /*
           * This enumeration describes the parameter direction as defined by the EDB server
           */
        /// <summary>
        /// Get param direction
        /// </summary>
        public enum EDBParameterDirection
        {
            Unknown = 0,
            Input = 1,
            Output = 2,
            InputOutput = 3
        }

        /// <summary>
        /// Get param OID
        /// </summary>
       public enum EDBParameterOID
        {
            Int2 = 21,
            Int4 = 23,
            Int8 = 20,
            Varchar = 1043,
            Text = 25,
            Boolean = 16,
            Numeric = 1700,
            Date = 1082,        //DateTime on server
            Time = 1083,
            Timestamp = 1114,
            Float4 = 700,
            Float8 = 701,
            Bytea = 17,
            Varchar2 = 1043,
            Datetime = 1082,
            Currency = 790,    //PG compatible Money Type
            Char = 1042,
            Refcursor = 1790,
            Int2Array = 1005,
            Int4Array = 1007,
            Int8Array = 1016,
            Float4Array = 1021,
            Float8Array = 1022,
            CharArray = 1002,
            BooleanArray = 1000,
            StringArray = 1015,
            Box = 603,
            Circle = 718,
            LSeg = 601,
            Path = 602,
            Point = 600,
            Polygon = 604,
           // Refcursor = 1790,
            Unknown = 0
        }

       /// <summary>
       /// Get EDB param direction
       /// </summary>

        public static EDBParameterDirection NetParamDirectionToEDBParamDirection(ParameterDirection direction)
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
        /// Get param to OID
        /// </summary>
        public static EDBParameterOID ParamToOid(String param_name)
        {
            /* EDB Team
             * Function Returns OID of datatype
             * EnterpriseDB: Check the param name after converting it to lower case.
             * Change all the case names to lower case.
            */
            switch (param_name.ToLower())
            {
                case "int4":
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
            var clone = new EDBParameter();
            clone._precision = _precision;
            clone._scale = _scale;
            clone._size = _size;
            clone._dbType = _dbType;
            clone._EDBDbType = _EDBDbType;
            clone._enumType = _enumType;
            clone.Direction = Direction;
            clone.IsNullable = IsNullable;
            clone._name = _name;
            clone.SourceColumn = SourceColumn;
#if !DNXCORE50
            clone.SourceVersion = SourceVersion;
#endif
            clone._value = _value;
            clone._EDBValue = _EDBValue;
            clone.SourceColumnNullMapping = SourceColumnNullMapping;

            return clone;
        }

#if !DNXCORE50
        object ICloneable.Clone()
        {
            return Clone();
        }
#endif
        #endregion
    }
}
