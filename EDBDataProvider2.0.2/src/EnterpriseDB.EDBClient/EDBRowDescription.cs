// created on 12/6/2002 at 20:29

// Npgsql.EDBRowDescription.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using EDBTypes;
using System.Globalization;

namespace EnterpriseDB.EDBClient
{
	/// <summary>
	/// This class represents a RowDescription message sent from
	/// the PostgreSQL.
	/// </summary>
	///
	internal abstract class EDBRowDescription : IServerResponseObject
	{

        private abstract class KanaWidthConverter
        {
            protected static readonly CompareInfo COMPARE_INFO = System.Globalization.CultureInfo.InvariantCulture.CompareInfo;
        }
        private sealed class KanaWidthInsensitiveComparer : KanaWidthConverter, IEqualityComparer<string>
        {
            public static readonly KanaWidthInsensitiveComparer INSTANCE = new KanaWidthInsensitiveComparer();
            private KanaWidthInsensitiveComparer() { }
            public bool Equals(string x, string y)
            {
                return COMPARE_INFO.Compare(x, y, CompareOptions.IgnoreWidth) == 0;
            }
            public int GetHashCode(string obj)
            {
                return COMPARE_INFO.GetSortKey(obj, CompareOptions.IgnoreWidth).GetHashCode();
            }
        }
        private sealed class KanaWidthCaseInsensitiveComparator : KanaWidthConverter, IEqualityComparer<string>
        {
            public static readonly KanaWidthCaseInsensitiveComparator INSTANCE = new KanaWidthCaseInsensitiveComparator();
            private KanaWidthCaseInsensitiveComparator() { }
            public bool Equals(string x, string y)
            {
                return COMPARE_INFO.Compare(x, y, CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase) == 0;
            }
            public int GetHashCode(string obj)
            {
                return COMPARE_INFO.GetSortKey(obj, CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase).GetHashCode();
            }
        }
		/// <summary>
		/// This struct represents the internal data of the RowDescription message.
		/// </summary>
		public abstract class FieldData
		{
			private string _name; // Protocol 2/3
			private int _typeOID; // Protocol 2/3
			private short _typeSize; // Protocol 2/3
			private int _typeModifier; // Protocol 2/3
			private int _tableOID; // Protocol 3
			private short _columnAttributeNumber; // Protocol 3
			private FormatCode _formatCode; // Protocol 3. 0 text, 1 binary
			private EDBBackendTypeInfo _typeInfo; // everything we know about this field type
            private Int16 _returingIndex;			   //Returning Index : EDB team

			public string Name
			{
				get { return _name; }
				protected set { _name = value; }
			}

			public int TypeOID
			{
				get { return _typeOID; }
				protected set { _typeOID = value; }
			}

			public short TypeSize
			{
				get { return _typeSize; }
				protected set { _typeSize = value; }
			}

			public int TypeModifier
			{
				get { return _typeModifier; }
				protected set { _typeModifier = value; }
			}

			public int TableOID
			{
				get { return _tableOID; }
				protected set { _tableOID = value; }
			}

			public short ColumnAttributeNumber
			{
				get { return _columnAttributeNumber; }
				protected set { _columnAttributeNumber = value; }
			}

            public FormatCode FormatCode
            {
                get { return _formatCode; }
                internal set { _formatCode = value; }
            }

			public EDBBackendTypeInfo TypeInfo
			{
				get { return _typeInfo; }
				protected set { _typeInfo = value; }
			}
            public Int16 ReturningIndex
            {
                get { return _returingIndex; }
                protected set { _returingIndex = value; }
            }
		}

        private  FieldData[] fields_data;
        private readonly Dictionary<string, int> field_name_index_table;
        private readonly Dictionary<string, int> caseInsensitiveNameIndexTable;
        private readonly Version _compatVersion;

        private readonly static Version KANA_FIX_VERSION = new Version(2, 0, 2, 1);
        private readonly static Version GET_ORDINAL_THROW_EXCEPTION = KANA_FIX_VERSION;
		protected EDBRowDescription(Stream stream, EDBBackendTypeMapping type_mapping,Version compatVersion)
		{
            int num = ReadNumFields(stream);
            fields_data = new FieldData[num];
            if ((_compatVersion = compatVersion) < KANA_FIX_VERSION)
            {
                field_name_index_table = new Dictionary<string, int>(num, StringComparer.InvariantCulture);
                caseInsensitiveNameIndexTable = new Dictionary<string, int>(num, StringComparer.InvariantCultureIgnoreCase);
            }
            else
            {
                field_name_index_table = new Dictionary<string, int>(num, KanaWidthInsensitiveComparer.INSTANCE);
                caseInsensitiveNameIndexTable = new Dictionary<string, int>(num, KanaWidthCaseInsensitiveComparator.INSTANCE);
            }
            for (int i = 0; i != num; ++i)
            {
                FieldData fd = BuildFieldData(stream, type_mapping);
                fields_data[i] = fd;
                if (!field_name_index_table.ContainsKey(fd.Name))
                {
                    field_name_index_table.Add(fd.Name, i);
                    if (!caseInsensitiveNameIndexTable.ContainsKey(fd.Name))
                    {
                        caseInsensitiveNameIndexTable.Add(fd.Name, i);
                    }
                }
            }
		}

		protected abstract FieldData BuildFieldData(Stream stream, EDBBackendTypeMapping typeMapping);
		protected abstract int ReadNumFields(Stream stream);

		public FieldData this[int index]
		{
			get { return fields_data[index]; }
		}

		public int NumFields
		{
			get { return (Int16) fields_data.Length; }
		}

        public bool HasOrdinal(string fieldName)
        {
            return caseInsensitiveNameIndexTable.ContainsKey(fieldName);
        }
        public int TryFieldIndex(string fieldName)
        {
            return HasOrdinal(fieldName) ? FieldIndex(fieldName) : -1;
        }
        public void AddReturnData(FieldData fd) 
        {

            FieldData[] fdData = new FieldData[fields_data.Length + 1];
            fdData[fields_data.Length] = fd;
            field_name_index_table.Clear();
            field_name_index_table.Add(fd.Name, fields_data.Length);
            for (int i = 0; i < fields_data.Length; i++)
            {
                fdData[i] = fields_data[i];
                if (!field_name_index_table.ContainsKey(fields_data[i].Name))
                     field_name_index_table.Add(fields_data[i].Name, i);
            }
            fields_data = fdData;
        }
        public int FieldIndex(String fieldName)
		{
            int ret = -1;
            if (field_name_index_table.TryGetValue(fieldName, out ret) || caseInsensitiveNameIndexTable.TryGetValue(fieldName, out ret))
                return ret;
            else if (_compatVersion < GET_ORDINAL_THROW_EXCEPTION)
                return -1;
            else
                throw new IndexOutOfRangeException("Field not found");
		}
	}

	internal sealed class EDBRowDescriptionV2 : EDBRowDescription
	{
        public EDBRowDescriptionV2(Stream stream, EDBBackendTypeMapping typeMapping, Version compatVersion)
			: base(stream, typeMapping,compatVersion)
		{
		}

		private sealed class FieldDataV2 : FieldData
		{
			public FieldDataV2(Stream stream, EDBBackendTypeMapping typeMapping)
			{
				Name = PGUtil.ReadString(stream);
				TypeInfo = typeMapping[TypeOID = PGUtil.ReadInt32(stream)];
				TypeSize = PGUtil.ReadInt16(stream);
				TypeModifier = PGUtil.ReadInt32(stream);
			}
		}

		protected override FieldData BuildFieldData(Stream stream, EDBBackendTypeMapping type_mapping)
		{
			return new FieldDataV2(stream, type_mapping);
		}

		protected override int ReadNumFields(Stream stream)
		{
			return PGUtil.ReadInt16(stream);
		}
	}

	internal sealed class EDBRowDescriptionV3 : EDBRowDescription
	{
		private sealed class FieldDataV3 : FieldData
		{
			public FieldDataV3(Stream stream, EDBBackendTypeMapping typeMapping)
			{
				Name = PGUtil.ReadString(stream);
				TableOID = PGUtil.ReadInt32(stream);
				ColumnAttributeNumber = PGUtil.ReadInt16(stream);
				TypeInfo = typeMapping[TypeOID = PGUtil.ReadInt32(stream)];
				TypeSize = PGUtil.ReadInt16(stream);
				TypeModifier = PGUtil.ReadInt32(stream);
				FormatCode = (FormatCode) PGUtil.ReadInt16(stream);
			}
		}

        public EDBRowDescriptionV3(Stream stream, EDBBackendTypeMapping typeMapping, Version compatVersion)
			: base(stream, typeMapping,compatVersion)
		{
		}

		protected override FieldData BuildFieldData(Stream stream, EDBBackendTypeMapping typeMapping)
		{
			return new FieldDataV3(stream, typeMapping);
		}

		protected override int ReadNumFields(Stream stream)
		{
			PGUtil.EatStreamBytes(stream, 4);
			return PGUtil.ReadInt16(stream);
		}
	}
    internal sealed class EDBRowOutDescriptionV3 : EDBRowDescription
    {
        private sealed class FieldDataV3 : FieldData
        {
            public FieldDataV3(Stream stream, EDBBackendTypeMapping typeMapping)
            {
                
                /*fdout.name = PGUtil.ReadString(input_stream, encoding);
				fdout.ReturingIndex = PGUtil.ReadInt16(input_stream, input_buffer);
				fdout.type_oid = PGUtil.ReadInt32(input_stream, input_buffer);
				fdout.type_size = PGUtil.ReadInt16(input_stream, input_buffer);
				fdout.type_modifier = PGUtil.ReadInt32(input_stream, input_buffer);
				fdout.format_code = (FormatCode)PGUtil.ReadInt16(input_stream, input_buffer);
				fdout.type_info = type_mapping[fdout.type_oid];
				fields_data.Add(fdout);
				fields_index.Add(fdout.name);*/

                Name = PGUtil.ReadString(stream);
                ReturningIndex = PGUtil.ReadInt16(stream);
                TypeOID = PGUtil.ReadInt32(stream);
                TypeSize = PGUtil.ReadInt16(stream);
                TypeModifier = PGUtil.ReadInt32(stream);
                FormatCode = (FormatCode)PGUtil.ReadInt16(stream);
                TypeInfo = typeMapping[TypeOID];
                
            }
        }

        public EDBRowOutDescriptionV3(Stream stream, EDBBackendTypeMapping typeMapping, Version compatVersion)
            : base(stream, typeMapping,compatVersion)
        {
            
        }

        protected override FieldData BuildFieldData(Stream stream, EDBBackendTypeMapping typeMapping)
        {
            return new FieldDataV3(stream, typeMapping);
        }
        protected override int ReadNumFields(Stream stream)
        {
            PGUtil.EatStreamBytes(stream, 4);
            return PGUtil.ReadInt16(stream);
        }
    }
}