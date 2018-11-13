// EDBTypes.EDBTypesHelper.cs
//
// Author:
//	Glen Parker <glenebob@nwlink.com>
//
//	Copyright (C) 2004 The EDB Development Team
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

// This file provides implementations of PostgreSQL specific data types that cannot
// be mapped to standard .NET classes.

using System;
using System.Collections;
using System.Globalization;
using System.Data;
using System.Net;
using System.Text;
using System.IO;
using System.Resources;
using System.Drawing;

namespace EDBTypes
{

    /// <summary>
    /// Represents a PostgreSQL Point type
    /// </summary>

    public struct EDBPoint
    {
        private Single _X;
        private Single _Y;

        public EDBPoint(Single X, Single Y)
        {
            _X = X;
            _Y = Y;
        }

        public Single X
        {
            get
            {
                return _X;
            }

            set
            {
                _X = value;
            }
        }


        public Single Y
        {
            get
            {
                return _Y;
            }

            set
            {
                _Y = value;
            }
        }
    }

    public struct EDBBox
    {
        private EDBPoint _UpperRight;
        private EDBPoint _LowerLeft;

        public EDBBox(EDBPoint UpperRight, EDBPoint LowerLeft)
        {
            _UpperRight = UpperRight;
            _LowerLeft = LowerLeft;
        }


        public EDBPoint UpperRight
        {
            get
            {
                return _UpperRight;
            }
            set
            {
                _UpperRight = value;
            }
        }

        public EDBPoint LowerLeft
        {
            get
            {
                return _LowerLeft;
            }
            set
            {
                _LowerLeft = value;
            }
        }

    }


    /// <summary>
    /// Represents a PostgreSQL Line Segment type.
    /// </summary>
    public struct EDBLSeg
    {
        public EDBPoint     Start;
        public EDBPoint     End;

        public EDBLSeg(EDBPoint Start, EDBPoint End)
        {
            this.Start = Start;
            this.End = End;
        }

        public override String ToString()
        {
            return String.Format("({0}, {1})", Start, End);
        }
    }

    /// <summary>
    /// Represents a PostgreSQL Path type.
    /// </summary>
    public struct EDBPath
    {
        internal EDBPoint[]	Points;

        internal Boolean 		IsOpen;

        public EDBPath(EDBPoint[] Points)
        {
            this.Points = Points;
            IsOpen = false;
        }

        public Int32 Count
        { get
          {
              return Points.Length;
          } }

        public EDBPoint this [Int32 Index]
        { get
          {
              return Points[Index];
          } }

        public Boolean Open
        {
            get
            {
                return IsOpen;
            }
        }
    }

    /// <summary>
    /// Represents a PostgreSQL Polygon type.
    /// </summary>
    public struct EDBPolygon
    {
        internal EDBPoint[]     Points;

        public EDBPolygon(EDBPoint[] Points)
        {
            this.Points = Points;
        }

        public Int32 Count
        { get
          {
              return Points.Length;
          } }

        public EDBPoint this [Int32 Index]
        { get
          {
              return Points[Index];
          } }
    }

    /// <summary>
    /// Represents a PostgreSQL Circle type.
    /// </summary>
    public struct EDBCircle
    {
        public EDBPoint   Center;
        public Double        Radius;

        public EDBCircle(EDBPoint Center, Double Radius)
        {
            this.Center = Center;
            this.Radius = Radius;
        }

        public override String ToString()
        {
            return string.Format("({0}), {1}", Center, Radius);
        }
    }
}
