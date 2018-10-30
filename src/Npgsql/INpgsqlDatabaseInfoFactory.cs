#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System.Threading.Tasks;
using JetBrains.Annotations;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// A factory which get generate instances of <see cref="EDBDatabaseInfo"/>, which describe a database
    /// and the types it contains. When first connecting to a database, EnterpriseDB.EDBClient will attempt to load information
    /// about it via this factory.
    /// </summary>
    public interface IEDBDatabaseInfoFactory
    {
        /// <summary>
        /// Given a connection, loads all necessary information about the connected database, e.g. its types.
        /// A factory should only handle the exact database type it was meant for, and return null otherwise.
        /// </summary>
        /// <returns>
        /// An object describing the database to which <paramref name="conn"/> is connected, or null if the
        /// database isn't of the correct type and isn't handled by this factory.
        /// </returns>
        [ItemCanBeNull]
        Task<EDBDatabaseInfo> Load(EDBConnection conn, EDBTimeout timeout, bool async);
    }
}
