// created on 18/5/2002 at 01:25

// EnterpriseDB.EDBClient.EDBParameter.cs
//
// Author:
//    Francisco Jr. (fxjrlists@yahoo.com.br)
//
//    Copyright (C) 2002 The EnterpriseDB.EDBClient Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Resources;
using EDBTypes;

#if WITHDESIGN
using EnterpriseDB.EDBClient.Design;
#endif

namespace EnterpriseDB.EDBClient
{
    ///<summary>
    /// This class represents a parameter to a command that will be sent to server
    ///</summary>
#if WITHDESIGN
    [TypeConverter(typeof(EDBParameterConverter))]
#endif

    public sealed class EDBParameter : DbParameter, ICloneable
    {
        // Logging related values
        private static readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;

        // Fields to implement IDbDataParameter interface.
        private byte precision = 0;
        private byte scale = 0;
        private Int32 size = 0;

        // Fields to implement IDataParameter
        //private EDBDbType                    npgsqldb_type = EDBDbType.Text;
        //private DbType                    db_type = DbType.String;
        private EDBNativeTypeInfo type_info;
        private EDBBackendTypeInfo backendTypeInfo;
        private ParameterDirection direction = ParameterDirection.Input;
        private Boolean is_nullable = false;
        private String m_Name = String.Empty;
        private String source_column = String.Empty;
        private DataRowVersion source_version = DataRowVersion.Current;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private Object value = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private Object npgsqlValue = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private Boolean sourceColumnNullMapping;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private EDBParameterCollection collection = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private static readonly ResourceManager resman = new ResourceManager(MethodBase.GetCurrentMethod().DeclaringType);

        private Boolean useCast = false;

        private static readonly EDBNativeTypeInfo defaultTypeInfo = EDBTypesHelper.GetNativeTypeInfo(typeof(String));

        private bool bound = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see> class.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EDBParameter()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
            //type_info = EDBTypesHelper.GetNativeTypeInfo(typeof(String));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>
        /// class with the parameter m_Name and a value of the new <b>EDBParameter</b>.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="value">An <see cref="System.Object">Object</see> that is the value of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.</param>
        /// <remarks>
        /// <p>When you specify an <see cref="System.Object">Object</see>
        /// in the value parameter, the <see cref="System.Data.DbType">DbType</see> is
        /// inferred from the .NET Framework type of the <b>Object</b>.</p>
        /// <p>When using this constructor, you must be aware of a possible misuse of the constructor which takes a DbType parameter.
        /// This happens when calling this constructor passing an int 0 and the compiler thinks you are passing a value of DbType.
        /// Use <code> Convert.ToInt32(value) </code> for example to have compiler calling the correct constructor.</p>
        /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EDBParameter(String parameterName, object value)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME, parameterName, value);

            this.ParameterName = parameterName;
            this.Value = value;

            /*if ((this.value == null) || (this.value == DBNull.Value))
            {
                // don't really know what to do - leave default and do further exploration
                // Default type for null values is String.
                this.value = DBNull.Value;
                type_info = EDBTypesHelper.GetNativeTypeInfo(typeof(String));
            }
            else if (!EDBTypesHelper.TryGetNativeTypeInfo(value.GetType(), out type_info))
            {
                throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value.GetType()));
            }*/

        }

        /// <summary>
        /// The collection to which this parameter belongs, if any.
        /// </summary>
        public EDBParameterCollection Collection
        {
            get { return collection; }

            internal set
            {
                collection = value;
                bound = false;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>
        /// class with the parameter m_Name and the data type.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        public EDBParameter(String parameterName, EDBDbType parameterType)
            : this(parameterName, parameterType, 0, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        public EDBParameter(String parameterName, DbType parameterType)
            : this(parameterName, EDBTypesHelper.GetNativeTypeInfo(parameterType).EDBDbType, 0, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="EDBTypes.EDBDbType">EDBDbType</see> values.</param>
        /// <param m_Name="size">The length of the parameter.</param>
        public EDBParameter(String parameterName, EDBDbType parameterType, Int32 size)
            : this(parameterName, parameterType, size, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param m_Name="size">The length of the parameter.</param>
        public EDBParameter(String parameterName, DbType parameterType, Int32 size)
            : this(parameterName, EDBTypesHelper.GetNativeTypeInfo(parameterType).EDBDbType, size, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="EDBTypes.EDBDbType">EDBDbType</see> values.</param>
        /// <param m_Name="size">The length of the parameter.</param>
        /// <param m_Name="sourceColumn">The m_Name of the source column.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EDBParameter(String parameterName, EDBDbType parameterType, Int32 size, String sourceColumn)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME, parameterName, parameterType, size, source_column);

            this.ParameterName = parameterName;

            EDBDbType = parameterType; //Allow the setter to catch any exceptions.

            this.size = size;
            source_column = sourceColumn;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param m_Name="size">The length of the parameter.</param>
        /// <param m_Name="sourceColumn">The m_Name of the source column.</param>
        public EDBParameter(String parameterName, DbType parameterType, Int32 size, String sourceColumn)
            : this(parameterName, EDBTypesHelper.GetNativeTypeInfo(parameterType).EDBDbType, size, sourceColumn)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="EDBTypes.EDBDbType">EDBDbType</see> values.</param>
        /// <param m_Name="size">The length of the parameter.</param>
        /// <param m_Name="sourceColumn">The m_Name of the source column.</param>
        /// <param m_Name="direction">One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see> values.</param>
        /// <param m_Name="isNullable"><b>true</b> if the value of the field can be null, otherwise <b>false</b>.</param>
        /// <param m_Name="precision">The total number of digits to the left and right of the decimal point to which
        /// <see cref="EnterpriseDB.EDBClient.EDBParameter.Value">Value</see> is resolved.</param>
        /// <param m_Name="scale">The total number of decimal places to which
        /// <see cref="EnterpriseDB.EDBClient.EDBParameter.Value">Value</see> is resolved.</param>
        /// <param m_Name="sourceVersion">One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.</param>
        /// <param m_Name="value">An <see cref="System.Object">Object</see> that is the value
        /// of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EDBParameter(String parameterName, EDBDbType parameterType, Int32 size, String sourceColumn,
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
                               ParameterDirection direction, bool isNullable, byte precision, byte scale,
                               DataRowVersion sourceVersion, object value)
        {
            this.ParameterName = parameterName;
            this.Size = size;
            this.SourceColumn = sourceColumn;
            this.Direction = direction;
            this.IsNullable = isNullable;
            this.Precision = precision;
            this.Scale = scale;
            this.SourceVersion = sourceVersion;
            this.Value = value;

            if (this.value == null)
            {
                this.value = DBNull.Value;
                type_info = EDBTypesHelper.GetNativeTypeInfo(typeof(String));
                EDBDbType = parameterType; /* We need it for invoking callable statements */ 
            }
            else
            {
                EDBDbType = parameterType; //allow the setter to catch exceptions if necessary.
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param m_Name="size">The length of the parameter.</param>
        /// <param m_Name="sourceColumn">The m_Name of the source column.</param>
        /// <param m_Name="direction">One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see> values.</param>
        /// <param m_Name="isNullable"><b>true</b> if the value of the field can be null, otherwise <b>false</b>.</param>
        /// <param m_Name="precision">The total number of digits to the left and right of the decimal point to which
        /// <see cref="EnterpriseDB.EDBClient.EDBParameter.Value">Value</see> is resolved.</param>
        /// <param m_Name="scale">The total number of decimal places to which
        /// <see cref="EnterpriseDB.EDBClient.EDBParameter.Value">Value</see> is resolved.</param>
        /// <param m_Name="sourceVersion">One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.</param>
        /// <param m_Name="value">An <see cref="System.Object">Object</see> that is the value
        /// of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.</param>
        public EDBParameter(String parameterName, DbType parameterType, Int32 size, String sourceColumn,
                               ParameterDirection direction, bool isNullable, byte precision, byte scale,
                               DataRowVersion sourceVersion, object value)
            : this(
                parameterName, EDBTypesHelper.GetNativeTypeInfo(parameterType).EDBDbType, size, sourceColumn, direction,
                isNullable, precision, scale, sourceVersion, value)
        {
        }

        // Implementation of IDbDataParameter
        /// <summary>
        /// Gets or sets the maximum number of digits used to represent the
        /// <see cref="EnterpriseDB.EDBClient.EDBParameter.Value">Value</see> property.
        /// </summary>
        /// <value>The maximum number of digits used to represent the
        /// <see cref="EnterpriseDB.EDBClient.EDBParameter.Value">Value</see> property.
        /// The default value is 0, which indicates that the data provider
        /// sets the precision for <b>Value</b>.</value>
        [Category("Data"), DefaultValue((Byte)0)]
        public override Byte Precision
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Precision");
                return precision;
            }

            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Precision", value);
                precision = value;
                bound = false;
            }
        }

        /// <summary>
        /// Whether to use an explicit cast when included in a query.
        /// </summary>
        public Boolean UseCast
        {
            get
            {

                // Prevents casts to be added for null values when they aren't needed.
                if (!useCast && (value == DBNull.Value || value == null))
                    return false;
                //return useCast; //&& (value != DBNull.Value);
                // This check for Datetime.minvalue and maxvalue is needed in order to
                // workaround a problem when comparing date values with infinity.
                // This is a known issue with postgresql and it is reported here:
                // http://archives.postgresql.org/pgsql-general/2008-10/msg00535.php
                // Josh's solution to add cast is documented here:
                // http://pgfoundry.org/forum/message.php?msg_id=1004118

                return useCast || DateTime.MinValue.Equals(value) || DateTime.MaxValue.Equals(value) || !EDBTypesHelper.DefinedType(Value);
            }
        }

        /// <summary>
        /// Gets or sets the number of decimal places to which
        /// <see cref="EnterpriseDB.EDBClient.EDBParameter.Value">Value</see> is resolved.
        /// </summary>
        /// <value>The number of decimal places to which
        /// <see cref="EnterpriseDB.EDBClient.EDBParameter.Value">Value</see> is resolved. The default is 0.</value>
        [Category("Data"), DefaultValue((Byte)0)]
        public override Byte Scale
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Scale");
                return scale;
            }

            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Scale", value);
                scale = value;
                bound = false;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of the data within the column.
        /// </summary>
        /// <value>The maximum size, in bytes, of the data within the column.
        /// The default value is inferred from the parameter value.</value>
        [Category("Data"), DefaultValue(0)]
        public override Int32 Size
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Size");
                return size;
            }

            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Size", value);
                size = value;
                bound = false;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DbType">DbType</see> of the parameter.
        /// </summary>
        /// <value>One of the <see cref="System.Data.DbType">DbType</see> values. The default is <b>String</b>.</value>
        [Category("Data"), RefreshProperties(RefreshProperties.All), DefaultValue(DbType.String)]
        public override DbType DbType
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "DbType");

                if (type_info == null)
                    return defaultTypeInfo.DbType;
                else
                    return TypeInfo.DbType;
            } // [TODO] Validate data type.
            set
            {

                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "DbType", value);

                useCast = value != DbType.Object;
                bound = false;

                if (!EDBTypesHelper.TryGetNativeTypeInfo(value, out type_info))
                {
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DbType">DbType</see> of the parameter.
        /// </summary>
        /// <value>One of the <see cref="System.Data.DbType">DbType</see> values. The default is <b>String</b>.</value>
        [Category("Data"), RefreshProperties(RefreshProperties.All), DefaultValue(EDBDbType.Text)]
        public EDBDbType EDBDbType
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "EDBDbType");

                if (type_info == null)
                    return defaultTypeInfo.EDBDbType;
                else
                    return TypeInfo.EDBDbType;
            } // [TODO] Validate data type.
            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "EDBDbType", value);
                useCast = true;
                bound = false;
                if (value == EDBDbType.Array)
                {
                    throw new ArgumentOutOfRangeException("value", resman.GetString("Exception_ParameterTypeIsOnlyArray"));
                }
                if (!EDBTypesHelper.TryGetNativeTypeInfo(value, out type_info))
                {
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value));
                }
            }
        }

        internal EDBNativeTypeInfo TypeInfo
        {
            get
            {
                if (type_info == null)
                {
                    //type_info = EDBTypesHelper.GetNativeTypeInfo(typeof(String));
                    return defaultTypeInfo;
                }
                return type_info;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is input-only,
        /// output-only, bidirectional, or a stored procedure return value parameter.
        /// </summary>
        /// <value>One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see>
        /// values. The default is <b>Input</b>.</value>
        [Category("Data"), DefaultValue(ParameterDirection.Input)]
        public override ParameterDirection Direction
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "Direction");
                return direction;
            }

            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Direction", value);
                direction = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter accepts null values.
        /// </summary>
        /// <value><b>true</b> if null values are accepted; otherwise, <b>false</b>. The default is <b>false</b>.</value>

#if WITHDESIGN
        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DefaultValue(false), DesignOnly(true)]
#endif

        public override Boolean IsNullable
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IsNullable");
                return is_nullable;
            }

            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "IsNullable", value);
                is_nullable = value;
            }
        }

        /// <summary>
        /// Gets or sets the m_Name of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.
        /// </summary>
        /// <value>The m_Name of the <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see>.
        /// The default is an empty string.</value>
        [DefaultValue("")]
        public override String ParameterName
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "ParameterName");
                return m_Name;
            }

            set
            {
                m_Name = value;
                if (value == null)
                {
                    m_Name = String.Empty;
                }
                // no longer prefix with : so that the m_Name returned is the m_Name set

                m_Name = m_Name.Trim();

                if (collection != null)
                {
                    collection.InvalidateHashLookups();
                    bound = false;
                }

                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "ParameterName", m_Name);
            }
        }

        /// <summary>
        /// The m_Name scrubbed of any optional marker
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
        /// Gets or sets the m_Name of the source column that is mapped to the
        /// <see cref="System.Data.DataSet">DataSet</see> and used for loading or
        /// returning the <see cref="EnterpriseDB.EDBClient.EDBParameter.Value">Value</see>.
        /// </summary>
        /// <value>The m_Name of the source column that is mapped to the
        /// <see cref="System.Data.DataSet">DataSet</see>. The default is an empty string.</value>
        [Category("Data"), DefaultValue("")]
        public override String SourceColumn
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "SourceColumn");
                return source_column;
            }

            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "SourceColumn", value);
                source_column = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DataRowVersion">DataRowVersion</see>
        /// to use when loading <see cref="EnterpriseDB.EDBClient.EDBParameter.Value">Value</see>.
        /// </summary>
        /// <value>One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.
        /// The default is <b>Current</b>.</value>
        [Category("Data"), DefaultValue(DataRowVersion.Current)]
        public override DataRowVersion SourceVersion
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "SourceVersion");
                return source_version;
            }

            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "SourceVersion", value);
                source_version = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>An <see cref="System.Object">Object</see> that is the value of the parameter.
        /// The default value is null.</value>
        [TypeConverter(typeof(StringConverter)), Category("Data")]
        public override Object Value
        {
            get
            {

                return this.value;

                /*
                EDBEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "Value");
                //return value;

                EDBBackendTypeInfo backendTypeInfo;

                if (EDBTypesHelper.TryGetBackendTypeInfo(type_info.Name, out backendTypeInfo))
                {
                    return backendTypeInfo.ConvertToFrameworkType(EDBValue);
                }

                throw new NotSupportedException();
                */

            } // [TODO] Check and validate data type.
            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Value", value);

                if ((value == null) || (value == DBNull.Value))
                {
                    // don't really know what to do - leave default and do further exploration
                    // Default type for null values is String.
#pragma warning disable CS8601 // Possible null reference assignment.
                    this.value = value;
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
                    this.npgsqlValue = value;
#pragma warning restore CS8601 // Possible null reference assignment.

                    bound = false;

                    //if (type_info == null)
                    //{
                    //    type_info = EDBTypesHelper.GetNativeTypeInfo(typeof(String));
                    //}
                    return;
                }

                if (type_info == null && !EDBTypesHelper.TryGetNativeTypeInfo(value.GetType(), out type_info))
                {
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value.GetType()));
                }

                if (backendTypeInfo == null && !EDBTypesHelper.TryGetBackendTypeInfo(type_info.Name, out backendTypeInfo))
                {
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value.GetType()));

                }
                else
                {
                    this.npgsqlValue = backendTypeInfo.ConvertToProviderType(value);
                    this.value = backendTypeInfo.ConvertToFrameworkType(npgsqlValue);

                    bound = false;
                }

            }
        }


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
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>An <see cref="System.Object">Object</see> that is the value of the parameter.
        /// The default value is null.</value>
        [TypeConverter(typeof(StringConverter)), Category("Data")]
        public Object EDBValue
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "EDBValue");
                return npgsqlValue;
            }

            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "EDBValue", value);

                Value = value;

                bound = false;
            }
        }

        /// <summary>
        /// Reset DBType.
        /// </summary>
        public override void ResetDbType()
        {
            //type_info = EDBTypesHelper.GetNativeTypeInfo(typeof(String));
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            type_info = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            this.Value = Value;
            bound = false;
        }

        /// <summary>
        /// Source column mapping.
        /// </summary>
        public override bool SourceColumnNullMapping
        {
            get { return sourceColumnNullMapping; }
            set { sourceColumnNullMapping = value; }
        }

        /// <summary>
        /// Creates a new <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see> that
        /// is a copy of the current instance.
        /// </summary>
        /// <returns>A new <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see> that is a copy of this instance.</returns>
        public EDBParameter Clone()
        {
            // use fields instead of properties
            // to avoid auto-initializing something like type_info
            EDBParameter clone = new EDBParameter();
            clone.precision = precision;
            clone.scale = scale;
            clone.size = size;
            clone.type_info = type_info;
            clone.direction = direction;
            clone.is_nullable = is_nullable;
            clone.m_Name = m_Name;
            clone.source_column = source_column;
            clone.source_version = source_version;
            clone.value = value;
            clone.npgsqlValue = npgsqlValue;
            clone.sourceColumnNullMapping = sourceColumnNullMapping;

            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        internal bool Bound
        {
            get { return bound; }
            set { bound = value; }
        }
    }
}


 /*
    * This enumeration describes the parameter direction as defined by the EDB server
    */
    public enum EDBParameterDirection
    {
        Unknown = 0,
        Input = 1,
        Output = 2,
        InputOutput = 3
    }
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
    Unknown = 0
}