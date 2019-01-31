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

using System.Data;
using EnterpriseDB.EDBClient.LegacyPostgis;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Extension adding the legacy PostGIS types to an EnterpriseDB.EDBClient type mapper.
    /// </summary>
    public static class EDBLegacyPostgisExtensions
    {
        /// <summary>
        /// Sets up the legacy PostGIS types to an EnterpriseDB.EDBClient type mapper.
        /// </summary>
        /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
        public static IEDBTypeMapper UseRawPostgis(this IEDBTypeMapper mapper)
            => mapper
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "geometry",
                    EDBDbType = EDBDbType.Geometry,
                    TypeHandlerFactory = new PostgisRawHandlerFactory()
                }.Build());
    }
}
