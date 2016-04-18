#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The  EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;

namespace  EnterpriseDB.EDBClient.Logging
{
    /// <summary>
    /// Manages logging for  EnterpriseDB.EDBClient, used to set the loggging provider.
    /// </summary>
    public static class EDBLogManager
    {
        /// <summary>
        /// The logging provider used for logging in  EnterpriseDB.EDBClient.
        /// </summary>
        public static IEDBLoggingProvider Provider
        {
            get
            {
                _providerRetrieved = true;
                return _provider;
            }
            set
            {
                if (_providerRetrieved)
                    throw new InvalidOperationException("The logging provider must be set before any  EnterpriseDB.EDBClient action is taken");

                _provider = value;
            }
        }

        static IEDBLoggingProvider _provider;
        static bool _providerRetrieved;

        static internal EDBLogger CreateLogger(string name)
        {
            return Provider.CreateLogger(name);
        }

        static internal EDBLogger GetCurrentClassLogger()
        {
            return CreateLogger(GetClassFullName());
        }

        // Copied from NLog
        static string GetClassFullName()
        {
            string className;
            Type declaringType;
            int framesToSkip = 2;

            do {
#if SILVERLIGHT
                StackFrame frame = new StackTrace().GetFrame(framesToSkip);
#else
                StackFrame frame = new StackFrame(framesToSkip, false);
#endif
                MethodBase method = frame.GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null) {
                    className = method.Name;
                    break;
                }

                framesToSkip++;
                className = declaringType.FullName;
            } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return className;
        }

        static EDBLogManager()
        {
            Provider = new NoOpLoggingProvider();
        }
    }
}
