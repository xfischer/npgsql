#region License
// The PostgreSQL License
//
// Copyright (C) 2019 The EnterpriseDB.EDBClient Development Team
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable
namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Provides DequeueMode options available.
    /// </summary>
    public class EDBAQDequeueMode
    {
        /// <summary>
        /// Read the message without locking.
        /// </summary>
        /// <value>Read the message without locking.</value>
        public static readonly int BROWSE = 0;

        /// <summary>
        /// This constant is defined, but will return an error if used.
        /// </summary>
        /// <value>Reads and gets a write lock on the message. </value>
        public static readonly int LOCKED = 1;

        /// <summary>
        /// Delete the message after reading; the default.
        /// </summary>
        /// <value>Deletes the message after reading. This is the default value.</value>
        public static readonly int REMOVE = 2;

        /// <summary>
        /// This constant is defined, but will return an error if used.
        /// </summary>
        /// <value>Confirms receipt of the message.</value>
        public static readonly int REMOVE_NODATA = 3;
    }
}
#nullable restore