// EDBTypes.EDBDbType.cs
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
using EnterpriseDB.EDBClient;


	 
namespace EDBTypes
{
    public enum EDBDbType
    {
        Bigint,
        Boolean,
		BooleanArray,
        Box,
        Bytea,
        Circle,
        Char,
		CharArray,
        Date,
        Double,
		DoubleArray,
		FloatArray,
        Integer,
		IntegerArray,
        Line,
		LongArray,
        LSeg,
        Money,
        Numeric,
        Path,
        Point,
        Polygon,
//      Real,
        Smallint,
        SmallintArray,
		StringArray,
        Text,       
        Time,
		Varchar,
        Timestamp,
		Float,
		DateTime,	
		Varchar2,	
		RefCursor,
    }

}

