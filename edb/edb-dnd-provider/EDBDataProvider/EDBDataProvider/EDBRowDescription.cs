// created on 12/6/2002 at 20:29

// EDB.EDBRowDescription.cs
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
using System.Collections;
using System.IO;
using System.Text;
using System.Net;

using EDBTypes;

namespace EnterpriseDB.EDBClient
{


    /// <summary>
    /// This struct represents the internal data of the RowDescription message.
    /// </summary>
    ///
    // [FIXME] Is this name OK? Does it represent well the struct intent?
    // Should it be a struct or a class?
//    internal struct EDBRowDescriptionFieldData
//    {
//        public String                   name;                      // Protocol 2/3
//        public Int16					ReturingIndex;			   //Returning Index : EDB team
//		public Int32                    table_oid;                 // Protocol 3
//        public Int16                    column_attribute_number;   // Protocol 3
//        public Int32                    type_oid;                  // Protocol 2/3
//        public Int16                    type_size;                 // Protocol 2/3
//        public Int32                    type_modifier;		       // Protocol 2/3
//        public FormatCode               format_code;               // Protocol 3. 0 text, 1 binary
//        public EDBBackendTypeInfo    type_info;                 // everything we know about this field type
//    }

	internal class EDBRowDescriptionFieldData
	{
		public String                   name;                      // Protocol 2/3
		public Int16					ReturingIndex;			   //Returning Index : EDB team
		public Int32                    table_oid;                 // Protocol 3
		public Int16                    column_attribute_number;   // Protocol 3
		public Int32                    type_oid;                  // Protocol 2/3
		public Int16                    type_size;                 // Protocol 2/3
		public Int32                    type_modifier;		       // Protocol 2/3
		public FormatCode               format_code;               // Protocol 3. 0 text, 1 binary
		public EDBBackendTypeInfo    type_info;                 // everything we know about this field type
	}
    /// <summary>
    /// This class represents a RowDescription message sent from
    /// the PostgreSQL.
    /// </summary>
    ///
    internal sealed class EDBRowDescription
    {
        // Logging related values
        private static readonly String CLASSNAME = "EDBRowDescription";


        private ArrayList                fields_data = new ArrayList();
        private ArrayList                fields_index = new ArrayList();

        private ProtocolVersion          protocol_version;
		
        public EDBRowDescription(ProtocolVersion protocolVersion)
        {
            protocol_version = protocolVersion;
        }
		public void AddField(EDBRowDescriptionFieldData fld)
		{
			fields_data.Add(fld);
			fields_index.Add(fld.name);
		}
		public void clear()
		{
			fields_data.Clear();
			fields_index.Clear();
		}
		public EDBRowDescriptionFieldData GetField(int index)
		{
			//if(index<fields_data.Count)
				return (EDBRowDescriptionFieldData)(fields_data.ToArray()[index]);
					
		}
        public void ReadFromStream(Stream input_stream, Encoding encoding, EDBBackendTypeMapping type_mapping)
        {
            switch (protocol_version)
            {
            case ProtocolVersion.Version2 :
                ReadFromStream_Ver_2(input_stream, encoding, type_mapping);
                break;

            case ProtocolVersion.Version3 :
				
				//if(true)
                ReadFromStream_Ver_3(input_stream, encoding, type_mapping);
				//else
			//	ReadFromStreamOutDescription_Ver_3(input_stream, encoding, type_mapping);
                break;

            }
        }

		public void ReadFromStreamOutDescription(Stream input_stream, Encoding encoding, EDBBackendTypeMapping type_mapping)
		{
			switch (protocol_version)
			{
				case ProtocolVersion.Version2 :
					ReadFromStream_Ver_2(input_stream, encoding, type_mapping);
					break;

				case ProtocolVersion.Version3 :
				
					//if(true)
					//ReadFromStream_Ver_3(input_stream, encoding, type_mapping);
					//else
						ReadFromStreamOutDescription_Ver_3(input_stream, encoding, type_mapping);
					break;

			}
		}




        private void ReadFromStream_Ver_2(Stream input_stream, Encoding encoding, EDBBackendTypeMapping type_mapping)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_2");

            Byte[] input_buffer = new Byte[10]; // Max read will be 4 + 2 + 4

            // Read the number of fields.
            input_stream.Read(input_buffer, 0, 2);
            Int16 num_fields = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input_buffer, 0));


            // Temporary FieldData object to get data from stream and put in array.
            EDBRowDescriptionFieldData fd;

            // Now, iterate through each field getting its data.
            for (Int16 i = 0; i < num_fields; i++)
            {
                fd = new EDBRowDescriptionFieldData();

                // Set field name.
                fd.name = PGUtil.ReadString(input_stream, encoding);

                // Read type_oid(Int32), type_size(Int16), type_modifier(Int32)
                input_stream.Read(input_buffer, 0, 4 + 2 + 4);

                fd.type_oid = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
                fd.type_info = type_mapping[fd.type_oid];
                fd.type_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input_buffer, 4));
                fd.type_modifier = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 6));

                // Add field data to array.
                fields_data.Add(fd);

                fields_index.Add(fd.name);
            }
        }

        private void ReadFromStream_Ver_3(Stream input_stream, Encoding encoding, EDBBackendTypeMapping type_mapping)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_3");

            Byte[] input_buffer = new Byte[4]; // Max read will be 4 + 2 + 4 + 2 + 4 + 2

            // Read the length of message.
            // [TODO] Any use for now?
            PGUtil.ReadInt32(input_stream, input_buffer);
            Int16 num_fields = PGUtil.ReadInt16(input_stream, input_buffer);

            // Temporary FieldData object to get data from stream and put in array.
            EDBRowDescriptionFieldData fd;

            for (Int16 i = 0; i < num_fields; i++)
            {
                fd = new EDBRowDescriptionFieldData();

                fd.name = PGUtil.ReadString(input_stream, encoding);
                fd.table_oid = PGUtil.ReadInt32(input_stream, input_buffer);
                fd.column_attribute_number = PGUtil.ReadInt16(input_stream, input_buffer);
                fd.type_oid = PGUtil.ReadInt32(input_stream, input_buffer);
                fd.type_info = type_mapping[fd.type_oid];
                fd.type_size = PGUtil.ReadInt16(input_stream, input_buffer);
                fd.type_modifier = PGUtil.ReadInt32(input_stream, input_buffer);
                fd.format_code = (FormatCode)PGUtil.ReadInt16(input_stream, input_buffer);
				fd.ReturingIndex = -1;
				fields_data.Add(fd);
                fields_index.Add(fd.name);
			}
        }

		private void ReadFromStreamOutDescription_Ver_3(Stream input_stream, Encoding encoding, EDBBackendTypeMapping type_mapping)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_3");

			Byte[] input_buffer = new Byte[4]; // Max read will be 4 + 2 + 4 + 2 + 4 + 2

			// Read the length of message.
			// [TODO] Any use for now?
			PGUtil.ReadInt32(input_stream, input_buffer);
			Int16 num_fields = PGUtil.ReadInt16(input_stream, input_buffer);

			// Temporary FieldData object to get data from stream and put in array.
			EDBRowDescriptionFieldData fdout;

			for (Int16 i = 0; i < num_fields; i++)
			{
				fdout = new EDBRowDescriptionFieldData();

				fdout.name = PGUtil.ReadString(input_stream, encoding);
				fdout.ReturingIndex = PGUtil.ReadInt16(input_stream, input_buffer);
				fdout.type_oid = PGUtil.ReadInt32(input_stream, input_buffer);
				fdout.type_size = PGUtil.ReadInt16(input_stream, input_buffer);
				fdout.type_modifier = PGUtil.ReadInt32(input_stream, input_buffer);
				fdout.format_code = (FormatCode)PGUtil.ReadInt16(input_stream, input_buffer);
				fdout.type_info = type_mapping[fdout.type_oid];
				fields_data.Add(fdout);
				fields_index.Add(fdout.name);
			}
		}



        public EDBRowDescriptionFieldData this[Int32 index]
        {
            get
            {
                return (EDBRowDescriptionFieldData)fields_data[index];
            }
        }

        public Int16 NumFields
        {
            get
            {
                return (Int16)fields_data.Count;
            }
        }

        public Int16 FieldIndex(String fieldName)
        {
            Int16 result = -1;

            // First try to find the index with IndexOf (case-sensitive)
            result = (Int16)fields_index.IndexOf(fieldName);

            if (result > -1)
            {
                return result;
            }
            else
            {

                result = 0;
                foreach (String name in fields_index)
                {

                    if (name.ToLower().Equals(fieldName.ToLower()))
                    {
                        return result;
                    }
                    result++;
                }

            }
            
            throw new ArgumentOutOfRangeException("fieldName", fieldName, "Field name not found");

        }

    }
}
