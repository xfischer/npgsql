#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EDBTypes;

namespace EnterpriseDB.EDBClient
{
    ///<summary>
    /// This class represents a parameter to a command that will be sent to server
    ///</summary>
#if NETSTANDARD1_3
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
        Type _specificType;
        string _name = string.Empty;
        object _value;
        object _EDBValue;

        /// <summary>
        /// Can be used to communicate a value from the validation phase to the writing phase.
        /// </summary>
        internal object ConvertedValue { get; set; }

        [CanBeNull]
        internal LengthCache LengthCache { get; private set; }

        internal TypeHandler Handler { get; private set; }
        internal FormatCode FormatCode { get; private set; }

        internal bool AutoAssignedName;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBParameter">EDBParameter</see> class.
        /// </summary>
        public EDBParameter()
        {
            SourceColumn = string.Empty;
            Direction = ParameterDirection.Input;
#if !NETSTANDARD1_3
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
        public EDBParameter(string parameterName, object value) : this()
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

#if !NETSTANDARD1_3
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
#if !NETSTANDARD1_3
        [TypeConverter(typeof(StringConverter)), Category("Data")]
#endif
        public override object Value
        {
            get
            {
                return _value;
            }
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
        [Category("Data")]
        [TypeConverter(typeof(StringConverter))]
        public object EDBValue
        {
            get => _EDBValue;
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
        [Category("Data")]
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
        [DefaultValue((byte)0)]
        [Category("Data")]
#if NET45
// In mono .NET 4.5 is actually a later version, meaning that virtual Precision and Scale already exist in DbParameter
#pragma warning disable CS0114
        public byte Precision
#pragma warning restore CS0114
#else
        public override byte Precision
#endif
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
        [DefaultValue((byte)0)]
        [Category("Data")]
#if NET45
// In mono .NET 4.5 is actually a later version, meaning that virtual Precision and Scale already exist in DbParameter
#pragma warning disable CS0114
        public byte Scale
#pragma warning restore CS0114
#else
        public override byte Scale
#endif
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
        [Category("Data")]
        public override int Size
        {
            get => _size;
            set
            {
                if (value < -1)
                    throw new ArgumentException($"Invalid parameter Size value '{value}'. The value must be greater than or equal to 0.");

                _size = value;
                ClearBind();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DbType">DbType</see> of the parameter.
        /// </summary>
        /// <value>One of the <see cref="System.Data.DbType">DbType</see> values. The default is <b>Object</b>.</value>
        [DefaultValue(DbType.Object)]
        [Category("Data"), RefreshProperties(RefreshProperties.All)]
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
        [Category("Data"), RefreshProperties(RefreshProperties.All)]
        public EDBDbType EDBDbType
        {
            get
            {
                if (_EDBDbType.HasValue) {
                    return _EDBDbType.Value;
                }

                if (_value != null) {   // Infer from value
                    return TypeHandlerRegistry.ToEDBDbType(_value);
                }

                return EDBDbType.Unknown;
            }
            set
            {
                if (value == EDBDbType.Array)
                    throw new ArgumentOutOfRangeException(nameof(value), "Cannot set EDBDbType to just Array, Binary-Or with the element type (e.g. Array of Box is EDBDbType.Array | EDBDbType.Box).");
                if (value == EDBDbType.Range)
                    throw new ArgumentOutOfRangeException(nameof(value), "Cannot set EDBDbType to just Range, Binary-Or with the element type (e.g. Range of integer is EDBDbType.Range | EDBDbType.Integer)");

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
            get => _name;
            set
            {
                _name = value;
                if (value == null)
                {
                    _name = string.Empty;
                }
                // no longer prefix with : so that The name returned is The name set

                _name = _name.Trim();

                if (Collection != null)
                {
                    Collection.InvalidateHashLookups();
                    ClearBind();
                }
                AutoAssignedName = false;
            }
        }

        /// <summary>
        /// Gets or sets The name of the source column that is mapped to the
        /// DataSet and used for loading or
        /// returning the <see cref="Value">Value</see>.
        /// </summary>
        /// <value>The name of the source column that is mapped to the DataSet.
        /// The default is an empty string.</value>
        [DefaultValue("")]
        [Category("Data")]
        public override string SourceColumn { get; set; }

#if !NETSTANDARD1_3
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
        [Obsolete("Use the SpecificType property instead")]
        [PublicAPI]
        public Type EnumType
        {
            get => SpecificType;
            set => SpecificType = value;
        }

        /// <summary>
        /// Used in combination with EDBDbType.Enum or EDBDbType.Composite to indicate the specific enum or composite type.
        /// For other EDBDbTypes, this field is not used.
        /// </summary>
        [PublicAPI]
        public Type SpecificType
        {
            get {
                if (_specificType != null)
                    return _specificType;

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
            set => _specificType = value;
        }

        /// <summary>
        /// The collection to which this parameter belongs, if any.
        /// </summary>
#pragma warning disable CA2227
        [CanBeNull]
        public EDBParameterCollection Collection { get; set; }
#pragma warning restore CA2227

        #endregion

        #region Internals

        /// <summary>
        /// The name scrubbed of any optional marker
        /// </summary>
        internal string CleanName
        {
            get
            {
                var name = ParameterName;
                if (name.Length > 0 && (name[0] == ':' || name[0] == '@'))
                {
                    return name.Substring(1);
                }
                return name;

            }
        }

        /// <summary>
        /// Returns whether this parameter has had its type set explicitly via DbType or EDBDbType
        /// (and not via type inference)
        /// </summary>
        internal bool IsTypeExplicitlySet => _EDBDbType.HasValue || _dbType.HasValue;

        internal void ResolveHandler(TypeHandlerRegistry registry)
        {
            if (Handler != null) {
                return;
            }

            if (_EDBDbType.HasValue)
            {
                Handler = registry[_EDBDbType.Value, SpecificType];
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
                throw new InvalidOperationException($"Parameter '{ParameterName}' must have its value set");
            }
        }

        internal void Bind(TypeHandlerRegistry registry)
        {
            ResolveHandler(registry);

            Debug.Assert(Handler != null);
            FormatCode = Handler.PreferTextWrite ? FormatCode.Text : FormatCode.Binary;
        }

        internal int ValidateAndGetLength()
        {
            if (Direction == ParameterDirection.Input)//EnterpriseDB Team
                if (_value == null)
                throw new InvalidCastException($"Parameter {ParameterName} must be set");
            if (_value is DBNull)
                return 0;

            var lengthCache = LengthCache;
            var len = Handler.ValidateAndGetLength(Value, ref lengthCache, this);
            LengthCache = lengthCache;
            return len;
        }

        internal Task WriteWithLength(WriteBuffer buf, bool async, CancellationToken cancellationToken)
            => Handler.WriteWithLength(Value, buf, LengthCache, this, async, cancellationToken);

        void ClearBind()
        {
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
        /// Get param to OID
        /// </summary>
        public static EDBParameterOID ParamToOid(String param_name)//EnterpriseDB Team
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
                _dbType = _dbType,
                _EDBDbType = _EDBDbType,
                _specificType = _specificType,
                Direction = Direction,
                IsNullable = IsNullable,
                _name = _name,
                SourceColumn = SourceColumn,
#if !NETSTANDARD1_3
                SourceVersion = SourceVersion,
#endif
                _value = _value,
                _EDBValue = _EDBValue,
                SourceColumnNullMapping = SourceColumnNullMapping,
                AutoAssignedName = AutoAssignedName
            };
            return clone;
        }

#if !NETSTANDARD1_3
        object ICloneable.Clone()
        {
            return Clone();
        }
#endif
        #endregion
    }
}
