// created on 4/3/2003 at 19:45

// EDB.EDBBinaryRow.cs
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
    /// This is the abstract base class for EDBAsciiRow and EDBBinaryRow.
    /// </summary>
    internal abstract class EDBRow
    {
        // Logging related values
        private static readonly String CLASSNAME = "EDBRow";

        protected ArrayList                  data;
        protected EDBRowDescription       row_desc;
        protected ProtocolVersion            protocol_version;

        public EDBRow(EDBRowDescription rowDesc, ProtocolVersion protocolVersion)
        {
            data = new ArrayList();
            row_desc = rowDesc;
            protocol_version = protocolVersion;
        }
		public int size()
		{
			return data.Count;
		}
		public Object GetData(int index)
		{
			return data.ToArray()[index];
		}
		public void AddData(Object o)
		{
			data.Add(o);
		}
        public virtual void ReadFromStream(Stream inputStream, Encoding encoding)
        {
          throw new NotImplementedException("Abstract");
        }

        /// <summary>
        /// Provide access to the fields in this row.
        /// </summary>
        public virtual Object this[Int32 index]
        {
            get
            {
                EDBEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, index);
                if ((index < 0) || (index >= row_desc.NumFields)) {
                    throw new IndexOutOfRangeException("this[] index value");
                }

                return data[index];
            }
        }
    }

}
