#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The  EnterpriseDB.EDBClient Development Team
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

using EDBTypes;
using  EnterpriseDB.EDBClient.Logging;
using  EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers;

namespace  EnterpriseDB.EDBClient.TypeHandlers.InternalTypesHandlers
{
    /// <summary>
    /// An int2vector is simply a regular array of shorts, with the sole exception that its lower bound must
    /// be 0 (we send 1 for regular arrays).
    /// </summary>
    [TypeMapping("int2vector", EDBDbType.Int2Vector)]
    internal class Int2VectorHandler : ArrayHandler<short>
    {
        static readonly EDBLogger Log = EDBLogManager.GetCurrentClassLogger();

        public Int2VectorHandler(IBackendType backendType, TypeHandlerRegistry registry)
            : base(backendType, null, 0)
        {

            // The pg_type SQL query makes sure that the int2 type comes before int2vector, so we can
            // depend on it already being in the registry
            var shortHandler = registry[EDBDbType.Smallint];
            if (shortHandler == registry.UnrecognizedTypeHandler)
            {
                Log.Warn("smallint type not present when setting up int2vector type. int2vector will not work.");
                return;
            }
            ElementHandler = shortHandler;
        }
    }
}
