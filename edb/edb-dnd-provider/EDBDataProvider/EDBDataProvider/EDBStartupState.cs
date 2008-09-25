// EDB.EDBStartupState.cs
//
// Author:
// 	Dave Joyner <d4ljoyn@yahoo.com>
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
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace EnterpriseDB.EDBClient
{


    internal sealed class EDBStartupState : EDBState
    {
        private static EDBStartupState _instance = null;

        private readonly String CLASSNAME = "EDBStartupState";

        private EDBStartupState() : base()
        { }

        public static EDBStartupState Instance
        {
            get
            {
                if ( _instance == null )
                {
                    _instance = new EDBStartupState();
                }
                return _instance;
            }
        }
        public override void Authenticate( EDBConnector context, string password)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Authenticate");
            EDBPasswordPacket pwpck = new EDBPasswordPacket(password, context.BackendProtocolVersion);
            BufferedStream stream = new BufferedStream(context.Stream);
            pwpck.WriteToStream(stream, context.Encoding);
            stream.Flush();

        }
    }
}
