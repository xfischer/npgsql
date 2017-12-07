#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The  EnterpriseDB.EDBClient DEVELOPMENT Team
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

namespace  EnterpriseDB.EDBClient.PostgresTypes
{
    /// <summary>
    /// Represents a PostgreSQL data type that isn't known to  EnterpriseDB.EDBClient and cannot be handled.
    /// </summary>
    public class UnknownBackendType : PostgresType
    {
        internal static readonly PostgresType Instance = new UnknownBackendType();

        /// <summary>
        /// Constructs a the unknown backend type.
        /// </summary>
#pragma warning disable CA2222 // Do not decrease inherited member visibility
        UnknownBackendType() : base("", "<unknown>", 0) { }
#pragma warning restore CA2222 // Do not decrease inherited member visibility

        internal override TypeHandler Activate(TypeHandlerRegistry registry)
        {
            throw new NotSupportedException("Cannot activate the unknown type");
        }
    }
}
