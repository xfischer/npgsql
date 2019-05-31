#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The EDB Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EDB DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EDB DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EDB DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EDB DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable 1591

namespace EnterpriseDB.EDBClient.Logging
{
    /// <summary>
    /// A generic interface for logging.
    /// </summary>
    public abstract class EDBLogger
    {
        public abstract bool IsEnabled(EDBLogLevel level);
        public abstract void Log(EDBLogLevel level, int connectorId, string msg, Exception exception = null);

        internal void Trace(string msg, int connectionId = 0) { Log(EDBLogLevel.Trace, connectionId, msg); }
        internal void Debug(string msg, int connectionId = 0) { Log(EDBLogLevel.Debug, connectionId, msg); }
        internal void Info(string msg,  int connectionId = 0) { Log(EDBLogLevel.Info,  connectionId, msg); }
        internal void Warn(string msg,  int connectionId = 0) { Log(EDBLogLevel.Warn,  connectionId, msg); }
        internal void Error(string msg, int connectionId = 0) { Log(EDBLogLevel.Error, connectionId, msg); }
        internal void Fatal(string msg, int connectionId = 0) { Log(EDBLogLevel.Fatal, connectionId, msg); }

        /*
        internal void Trace(string msg, int connectionId = 0, params object[] args) { Log(EDBLogLevel.Trace, String.Format(msg, args)); }
        internal void Debug(string msg, params object[] args) { Log(EDBLogLevel.Debug, String.Format(msg, args)); }
        internal void Info(string msg,  params object[] args) { Log(EDBLogLevel.Info,  String.Format(msg, args)); }
        internal void Warn(string msg,  params object[] args) { Log(EDBLogLevel.Warn,  String.Format(msg, args)); }
        internal void Error(string msg, params object[] args) { Log(EDBLogLevel.Error, String.Format(msg, args)); }
        internal void Fatal(string msg, params object[] args) { Log(EDBLogLevel.Fatal, String.Format(msg, args)); }
         */

        internal void Trace(string msg, Exception ex, int connectionId = 0) { Log(EDBLogLevel.Trace, connectionId, msg, ex); }
        internal void Debug(string msg, Exception ex, int connectionId = 0) { Log(EDBLogLevel.Debug, connectionId, msg, ex); }
        internal void Info(string msg,  Exception ex, int connectionId = 0) { Log(EDBLogLevel.Info,  connectionId, msg, ex); }
        internal void Warn(string msg,  Exception ex, int connectionId = 0) { Log(EDBLogLevel.Warn,  connectionId, msg, ex); }
        internal void Error(string msg, Exception ex, int connectionId = 0) { Log(EDBLogLevel.Error, connectionId, msg, ex); }
        internal void Fatal(string msg, Exception ex, int connectionId = 0) { Log(EDBLogLevel.Fatal, connectionId, msg, ex); }
    }
}
