#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EDB Development Team
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
using EnterpriseDB.EDBClient.Json.NET;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Extension allowing adding the Json.NET plugin to an EDB type mapper.
    /// </summary>
    public static class EDBJsonNetExtensions
    {
        /// <summary>
        /// Sets up JSON.NET mappings for the PostgreSQL json and jsonb types.
        /// </summary>
        /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
        /// <param name="jsonbClrTypes">A list of CLR types to map to PostgreSQL jsonb (no need to specify EDBDbType.Jsonb)</param>
        /// <param name="jsonClrTypes">A list of CLR types to map to PostgreSQL json (no need to specify EDBDbType.Json)</param>
        /// <param name="settings">Optional settings to customize JSON serialization</param>
        public static IEDBTypeMapper UseJsonNet(
            this IEDBTypeMapper mapper,
            Type[] jsonbClrTypes = null,
            Type[] jsonClrTypes = null,
            JsonSerializerSettings settings = null
        )
        {
            mapper.AddMapping(new EDBTypeMappingBuilder
            {
                PgTypeName = "jsonb",
                EDBDbType = EDBDbType.Jsonb,
                ClrTypes = jsonbClrTypes,
                TypeHandlerFactory = new JsonbHandlerFactory(settings)
            }.Build());

            mapper.AddMapping(new EDBTypeMappingBuilder
            {
                PgTypeName = "json",
                EDBDbType = EDBDbType.Json,
                ClrTypes = jsonClrTypes,
                TypeHandlerFactory = new JsonHandlerFactory(settings)
            }.Build());

            return mapper;
        }
    }
}
