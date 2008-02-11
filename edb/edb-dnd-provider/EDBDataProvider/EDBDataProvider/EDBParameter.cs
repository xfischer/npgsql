// created on 18/5/2002 at 01:25

// EDB.EDBParameter.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The EDB Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using System;
using System.Data;
using System.ComponentModel;
using EDBTypes;
#if WITHDESIGN
using EnterpriseDB.EDBClient.Design;
#endif


namespace EnterpriseDB.EDBClient
{
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

    /*
     * This enumeration describes the parameter's OID on the server
     */
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

    ///<summary>
    /// This class represents a parameter to a command that will be sent to server
    ///</summary>
    #if WITHDESIGN
    [TypeConverter(typeof(EDBParameterConverter))]
    #endif
	public sealed class EDBParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{

		// Logging related values
		private static readonly String CLASSNAME = "EDBParameter";

		// Fields to implement IDbDataParameter interface.
		private byte 				    precision = 0;
		private byte 				    scale = 0;
		private Int32				    size = 0;

		// Fields to implement IDataParameter
		//private EDBDbType				    npgsqldb_type = EDBDbType.Text;
		//private DbType                    db_type = DbType.String;
		private EDBNativeTypeInfo	type_info;
		private ParameterDirection	    direction = ParameterDirection.Input;
		private Boolean				    is_nullable = false;
		private String				    name = String.Empty;
		private String				    source_column = String.Empty;
		private DataRowVersion		    source_version = DataRowVersion.Current;
		private Object				    value = DBNull.Value;
		private System.Resources.ResourceManager resman;

		/// <summary>

		/// Initializes a new instance of the <see cref="EDB.EDBParameter">EDBParameter</see> class.
		/// </summary>
		public EDBParameter()
		{
			resman = new System.Resources.ResourceManager(this.GetType());
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EDB.EDBParameter">EDBParameter</see>
		/// class with the parameter name and a value of the new <b>EDBParameter</b>.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map.</param>
		/// <param name="value">An <see cref="System.Object">Object</see> that is the value of the <see cref="EDB.EDBParameter">EDBParameter</see>.</param>
		/// <remarks>
		/// <p>When you specify an <see cref="System.Object">Object</see>
		/// in the value parameter, the <see cref="EDBTypes.EDBDbType">DbType</see> is
		/// inferred from the .NET Framework type of the <b>Object</b>.</p>
		/// <p>When using this constructor, you must be aware of a possible misuse of the constructor which takes a DbType parameter.
		/// This happens when calling this constructor passing an int 0 and the compiler thinks you are passing a value of DbType.
		/// Use <code> Convert.ToInt32(value) </code> for example to have compiler calling the correct constructor.</p>
		/// </remarks>
		public EDBParameter(String parameterName, object value)
		{
			resman = new System.Resources.ResourceManager(this.GetType());
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME, parameterName, value);

			this.ParameterName = parameterName;
			this.value = value;

			if ((this.value == null) || (this.value == DBNull.Value) )
			{
				// don't really know what to do - leave default and do further exploration
				// Default type for null values is String.
				this.value = DBNull.Value;
				type_info = EDBTypesHelper.GetNativeTypeInfo(typeof(String));
				return;
			}
			else
			{
				type_info = EDBTypesHelper.GetNativeTypeInfo(value.GetType());
				if (type_info == null)
				{
					throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value.GetType()));
				}

			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EDB.EDBParameter">EDBParameter</see>
		/// class with the parameter name and the data type.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map.</param>
		/// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType">DbType</see> values.</param>
		public EDBParameter(String parameterName, EDBDbType parameterType) : this(parameterName, parameterType, 0, String.Empty)
		{}


		public EDBParameter(String parameterName, DbType parameterType) : this(parameterName, EDBTypesHelper.GetNativeTypeInfo(parameterType).EDBDbType, 0, String.Empty)
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="EDB.EDBParameter">EDBParameter</see>
		/// class with the parameter name, the <see cref="EDBTypes.EDBDbType">DbType</see>, and the size.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map.</param>
		/// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType">DbType</see> values.</param>
		/// <param name="size">The length of the parameter.</param>
		public EDBParameter(String parameterName, EDBDbType parameterType, Int32 size) : this(parameterName, parameterType, size, String.Empty)
		{}

		public EDBParameter(String parameterName, DbType parameterType, Int32 size) : this(parameterName, EDBTypesHelper.GetNativeTypeInfo(parameterType).EDBDbType, size, String.Empty)
		{}


		/// <summary>
		/// Initializes a new instance of the <see cref="EDB.EDBParameter">EDBParameter</see>
		/// class with the parameter name, the <see cref="EDBTypes.EDBDbType">DbType</see>, the size,
		/// and the source column name.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map.</param>
		/// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType">DbType</see> values.</param>
		/// <param name="size">The length of the parameter.</param>
		/// <param name="sourceColumn">The name of the source column.</param>
		public EDBParameter(String parameterName, EDBDbType parameterType, Int32 size, String sourceColumn)
		{

			resman = new System.Resources.ResourceManager(this.GetType());

			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME, parameterName, parameterType, size, source_column);

			this.ParameterName = parameterName;

			type_info = EDBTypesHelper.GetNativeTypeInfo(parameterType);
			if (type_info == null)
				throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), parameterType));

			this.size = size;
			source_column = sourceColumn;


		}

		public EDBParameter(String parameterName, DbType parameterType, Int32 size, String sourceColumn) : this(parameterName, EDBTypesHelper.GetNativeTypeInfo(parameterType).EDBDbType, size, sourceColumn)
		{}



		/// <summary>
		/// Initializes a new instance of the <see cref="EDB.EDBParameter">EDBParameter</see>
		/// class with the parameter name, the <see cref="EDBTypes.EDBDbType">DbType</see>, the size,
		/// the source column name, a <see cref="System.Data.ParameterDirection">ParameterDirection</see>,
		/// the precision of the parameter, the scale of the parameter, a
		/// <see cref="System.Data.DataRowVersion">DataRowVersion</see> to use, and the
		/// value of the parameter.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map.</param>
		/// <param name="parameterType">One of the <see cref="EDBTypes.EDBDbType">DbType</see> values.</param>
		/// <param name="size">The length of the parameter.</param>
		/// <param name="sourceColumn">The name of the source column.</param>
		/// <param name="direction">One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see> values.</param>
		/// <param name="isNullable"><b>true</b> if the value of the field can be null, otherwise <b>false</b>.</param>
		/// <param name="precision">The total number of digits to the left and right of the decimal point to which
		/// <see cref="EDB.EDBParameter.Value">Value</see> is resolved.</param>
		/// <param name="scale">The total number of decimal places to which
		/// <see cref="EDB.EDBParameter.Value">Value</see> is resolved.</param>
		/// <param name="sourceVersion">One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.</param>
		/// <param name="value">An <see cref="System.Object">Object</see> that is the value
		/// of the <see cref="EDB.EDBParameter">EDBParameter</see>.</param>
		public EDBParameter (String parameterName, EDBDbType parameterType, Int32 size, String sourceColumn, ParameterDirection direction, bool isNullable, byte precision, byte scale, DataRowVersion sourceVersion, object value)
		{

			resman = new System.Resources.ResourceManager(this.GetType());

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
			}
			else
			{
				type_info = EDBTypesHelper.GetNativeTypeInfo(parameterType);
				if (type_info == null)
					throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), parameterType));
			}

		}

		public EDBParameter (String parameterName, DbType parameterType, Int32 size, String sourceColumn, ParameterDirection direction, bool isNullable, byte precision, byte scale, DataRowVersion sourceVersion, object value) : this(parameterName, EDBTypesHelper.GetNativeTypeInfo(parameterType).EDBDbType, size, sourceColumn, direction, isNullable, precision, scale, sourceVersion, value)
		{}

        
		
		// Implementation of IDbDataParameter
		/// <summary>
		/// Gets or sets the maximum number of digits used to represent the
		/// <see cref="EDB.EDBParameter.Value">Value</see> property.
		/// </summary>
		/// <value>The maximum number of digits used to represent the
		/// <see cref="EDB.EDBParameter.Value">Value</see> property.
		/// The default value is 0, which indicates that the data provider
		/// sets the precision for <b>Value</b>.</value>
		[Category("Data"), DefaultValue((Byte)0)]
		public Byte Precision
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
			}
		}

		/// <summary>
		/// Gets or sets the number of decimal places to which
		/// <see cref="EDB.EDBParameter.Value">Value</see> is resolved.
		/// </summary>
		/// <value>The number of decimal places to which
		/// <see cref="EDB.EDBParameter.Value">Value</see> is resolved. The default is 0.</value>
		[Category("Data"), DefaultValue((Byte)0)]
		public Byte Scale
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
			}
		}

		/// <summary>
		/// Gets or sets the maximum size, in bytes, of the data within the column.
		/// </summary>
		/// <value>The maximum size, in bytes, of the data within the column.
		/// The default value is inferred from the parameter value.</value>
		[Category("Data"), DefaultValue(0)]
		public Int32 Size
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
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="EDBTypes.EDBDbType">DbType</see> of the parameter.
		/// </summary>
		/// <value>One of the <see cref="EDBTypes.EDBDbType">DbType</see> values. The default is <b>String</b>.</value>
		[Category("Data"), RefreshProperties(RefreshProperties.All), DefaultValue(DbType.String)]
		public DbType DbType
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "DbType");
				return TypeInfo.DbType;
			}

			// [TODO] Validate data type.
			set
			{
				EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "DbType", value);
				type_info = EDBTypesHelper.GetNativeTypeInfo(value);
				if (type_info == null)
					throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value));

			}
		}

		/// <summary>
		/// Gets or sets the <see cref="EDBTypes.EDBDbType">DbType</see> of the parameter.
		/// </summary>
		/// <value>One of the <see cref="EDBTypes.EDBDbType">DbType</see> values. The default is <b>String</b>.</value>
		[Category("Data"), RefreshProperties(RefreshProperties.All), DefaultValue(EDBDbType.Text)]
		public EDBDbType EDBDbType
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "DbType");

				return TypeInfo.EDBDbType;
			}

			// [TODO] Validate data type.
			set
			{
				EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "DbType", value);
				type_info = EDBTypesHelper.GetNativeTypeInfo(value);
				if (type_info == null)
					throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value));
			}
		}



		internal EDBNativeTypeInfo TypeInfo
		{
			get
			{
				
				return type_info;
			}
		}
			
		//		public static  EDBNativeTypeInfo TypeInfo
		//		{
		//			get
		//			{
		//				return type_info;
		//			}
		//		}

		/// <summary>
		/// Gets or sets a value indicating whether the parameter is input-only,
		/// output-only, bidirectional, or a stored procedure return value parameter.
		/// </summary>
		/// <value>One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see>
		/// values. The default is <b>Input</b>.</value>
		[Category("Data"), DefaultValue(ParameterDirection.Input)]
		public ParameterDirection Direction
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
		public Boolean IsNullable
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
		/// Gets or sets the name of the <see cref="EDB.EDBParameter">EDBParameter</see>.
		/// </summary>
		/// <value>The name of the <see cref="EDB.EDBParameter">EDBParameter</see>.
		/// The default is an empty string.</value>
		[DefaultValue("")]
		public String ParameterName
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "ParameterName");
				return name;
			}

			set
			{
				name = value;
				if (value == null)
					name = String.Empty;
				if ( (name.Equals(String.Empty)) || ((name[0] != ':') && (name[0] != '@')) )
					name = ':' + name;

				EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "ParameterName", value);
			}
		}

		/// <summary>
		/// Gets or sets the name of the source column that is mapped to the
		/// <see cref="System.Data.DataSet">DataSet</see> and used for loading or
		/// returning the <see cref="EDB.EDBParameter.Value">Value</see>.
		/// </summary>
		/// <value>The name of the source column that is mapped to the
		/// <see cref="System.Data.DataSet">DataSet</see>. The default is an empty string.</value>
		[Category("Data"), DefaultValue("")]
		public String SourceColumn
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
		/// to use when loading <see cref="EDB.EDBParameter.Value">Value</see>.
		/// </summary>
		/// <value>One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.
		/// The default is <b>Current</b>.</value>
		[Category("Data"), DefaultValue(DataRowVersion.Current)]
		public DataRowVersion SourceVersion
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
		public Object Value
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "Value");
				return value;
			}

			// [TODO] Check and validate data type.
			set
			{
				EDBEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Value", value);

				this.value = value;
				if ((this.value == null) || (this.value == DBNull.Value) )
				{
					// don't really know what to do - leave default and do further exploration
					// Default type for null values is String.
					this.value = DBNull.Value;
					if (type_info == null)
						type_info = EDBTypesHelper.GetNativeTypeInfo(typeof(String));

				}
				else
				{
					if (type_info == null)
					{
						type_info = EDBTypesHelper.GetNativeTypeInfo(value.GetType());
						if (type_info == null)
							throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value.GetType()));
                        
					}

				}
			}
		}

		/// <summary>
		/// Creates a new <see cref="EDB.EDBParameter">EDBParameter</see> that
		/// is a copy of the current instance.
		/// </summary>
		/// <returns>A new <see cref="EDB.EDBParameter">EDBParameter</see> that is a copy of this instance.</returns>
		object System.ICloneable.Clone()
		{
			return new EDBParameter(this.ParameterName, this.EDBDbType,	this.Size, this.SourceColumn, this.Direction, this.IsNullable, this.Precision, this.Scale, this.SourceVersion, this.Value);
		}

		public static EDBParameterOID ParamToOid(String param_name)
		{

			// EDB Team
			// Function Returns OID of datatype
			// EnterpriseDB: Check the param name after converting it to lower case.
			//Change all the case names to lower case.

			switch(param_name.ToLower())
			{
				case "int4":
					return EDBParameterOID.Int4;

				case "varchar":
					return  EDBParameterOID.Varchar;

				case "text" :
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
				case"int2" :  
					return EDBParameterOID.Int2;

					/*EnterpriseDB Team : 28 DEC Support of BigInt:*/
				case "int8" :
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
			switch(direction)
			{
				case ParameterDirection.Input:
					return EDBParameterDirection.Input;
				case ParameterDirection.Output:
					return EDBParameterDirection.Output;
				case ParameterDirection.InputOutput:
					return EDBParameterDirection.InputOutput;
				default :
					return EDBParameterDirection.Unknown;
			}
		}
	}
}
