// EDBTypes.EDBTypeMappings.cs
//
// Author:
//    Francisco Jr. (fxjrlists@yahoo.com.br)
//
//    Copyright (C) 2002 The EnterpriseDB.EDBClient Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Resources;
using System.Text;
using System.IO;
using EnterpriseDB.EDBClient;

namespace EDBTypes
{
    /// <summary>
    /// Provide mapping between type OID, type name, and a EDBBackendTypeInfo object that represents it.
    /// </summary>
    internal class EDBBackendTypeMapping
    {
        private readonly Dictionary<int, EDBBackendTypeInfo> OIDIndex;
        private readonly Dictionary<string, EDBBackendTypeInfo> NameIndex;

        /// <summary>
        /// Construct an empty mapping.
        /// </summary>
        public EDBBackendTypeMapping()
        {
            OIDIndex = new Dictionary<int, EDBBackendTypeInfo>();
            NameIndex = new Dictionary<string, EDBBackendTypeInfo>();
        }

        /// <summary>
        /// Copy constuctor.
        /// </summary>
        private EDBBackendTypeMapping(EDBBackendTypeMapping Other)
        {
            OIDIndex = new Dictionary<int, EDBBackendTypeInfo>(Other.OIDIndex);
            NameIndex = new Dictionary<string, EDBBackendTypeInfo>(Other.NameIndex);
        }

        /// <summary>
        /// Add the given EDBBackendTypeInfo to this mapping.
        /// </summary>
        public void AddType(EDBBackendTypeInfo T)
        {
            if (OIDIndex.ContainsKey(T.OID))
            {
                throw new Exception("Type already mapped");
            }

            OIDIndex[T.OID] = T;
            NameIndex[T.Name] = T;
        }

        /// <summary>
        /// Add a new EDBBackendTypeInfo with the given attributes and conversion handlers to this mapping.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="EDBDbType">EDBDbType</param>
        /// <param name="DbType">DbType</param>
        /// <param name="Type">System type to convert fields of this type to.</param>
        /// <param name="BackendTextConvert">Data conversion handler for text encoding.</param>
        /// <param name="BackendBinaryConvert">Data conversion handler for binary data.</param>
        public void AddType(Int32 OID, String Name, EDBDbType EDBDbType, DbType DbType, Type Type,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            ConvertBackendTextToNativeHandler BackendTextConvert = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            ConvertBackendBinaryToNativeHandler BackendBinaryConvert = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8604 // Possible null reference argument.
            AddType(new EDBBackendTypeInfo(OID, Name, EDBDbType, DbType, Type, BackendTextConvert = null, BackendBinaryConvert = null));
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        /// <summary>
        /// Get the number of type infos held.
        /// </summary>
        public Int32 Count
        {
            get { return NameIndex.Count; }
        }

        public bool TryGetValue(int oid, out EDBBackendTypeInfo value)
        {
            return OIDIndex.TryGetValue(oid, out value);
        }

        /// <summary>
        /// Retrieve the EDBBackendTypeInfo with the given backend type OID, or null if none found.
        /// </summary>
        public EDBBackendTypeInfo this[Int32 OID]
        {
            get
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                EDBBackendTypeInfo ret = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
                return TryGetValue(OID, out ret) ? ret : null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        /// <summary>
        /// Retrieve the EDBBackendTypeInfo with the given backend type name, or null if none found.
        /// </summary>
        public EDBBackendTypeInfo this[String Name]
        {
            get
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                EDBBackendTypeInfo ret = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
                return NameIndex.TryGetValue(Name, out ret) ? ret : null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        /// <summary>
        /// Make a shallow copy of this type mapping.
        /// </summary>
        public EDBBackendTypeMapping Clone()
        {
            return new EDBBackendTypeMapping(this);
        }

        /// <summary>
        /// Determine if a EDBBackendTypeInfo with the given backend type OID exists in this mapping.
        /// </summary>
        public Boolean ContainsOID(Int32 OID)
        {
            return OIDIndex.ContainsKey(OID);
        }

        /// <summary>
        /// Determine if a EDBBackendTypeInfo with the given backend type name exists in this mapping.
        /// </summary>
        public Boolean ContainsName(String Name)
        {
            return NameIndex.ContainsKey(Name);
        }
    }

    /// <summary>
    /// Provide mapping between type Type, EDBDbType and a EDBNativeTypeInfo object that represents it.
    /// </summary>
    internal class EDBNativeTypeMapping
    {
        private readonly Dictionary<string, EDBNativeTypeInfo> NameIndex = new Dictionary<string, EDBNativeTypeInfo>();

        private readonly Dictionary<EDBDbType, EDBNativeTypeInfo> EDBDbTypeIndex =
            new Dictionary<EDBDbType, EDBNativeTypeInfo>();

        private readonly Dictionary<DbType, EDBNativeTypeInfo> DbTypeIndex = new Dictionary<DbType, EDBNativeTypeInfo>();
        private readonly Dictionary<Type, EDBNativeTypeInfo> TypeIndex = new Dictionary<Type, EDBNativeTypeInfo>();

        /// <summary>
        /// Add the given EDBNativeTypeInfo to this mapping.
        /// </summary>
        public void AddType(EDBNativeTypeInfo T)
        {
            if (NameIndex.ContainsKey(T.Name))
            {
                throw new Exception("Type already mapped");
            }

            NameIndex[T.Name] = T;
            EDBDbTypeIndex[T.EDBDbType] = T;
            DbTypeIndex[T.DbType] = T;
            if (!T.IsArray)
            {
                EDBNativeTypeInfo arrayType = EDBNativeTypeInfo.ArrayOf(T);
                NameIndex[arrayType.Name] = arrayType;

                NameIndex[arrayType.CastName] = arrayType;
                EDBDbTypeIndex[arrayType.EDBDbType] = arrayType;
            }
        }

        /// <summary>
        /// Add a new EDBNativeTypeInfo with the given attributes and conversion handlers to this mapping.
        /// </summary>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="EDBDbType">EDBDbType</param>
        /// <param name="DbType">DbType</param>
        /// <param name="Quote">Quote</param>
        /// <param name="NativeTextConvert">Data conversion handler for text backend encoding.</param>
        /// <param name="NativeBinaryConvert">Data conversion handler for binary backend encoding (for extended query).</param>
        public void AddType(String Name, EDBDbType EDBDbType, DbType DbType, Boolean Quote,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            ConvertNativeToBackendTextHandler NativeTextConvert = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            ConvertNativeToBackendBinaryHandler NativeBinaryConvert = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            AddType(new EDBNativeTypeInfo(Name, EDBDbType, DbType, Quote, NativeTextConvert, NativeBinaryConvert));
        }

        public void AddEDBDbTypeAlias(String Name, EDBDbType EDBDbType)
        {
            if (EDBDbTypeIndex.ContainsKey(EDBDbType))
            {
                throw new Exception("EDBDbType already aliased");
            }

            EDBDbTypeIndex[EDBDbType] = NameIndex[Name];
        }

        public void AddDbTypeAlias(String Name, DbType DbType)
        {
            /*if (DbTypeIndex.ContainsKey(DbType))
            {
                throw new Exception("DbType already aliased");
            }*/

            DbTypeIndex[DbType] = NameIndex[Name];
        }

        public void AddTypeAlias(String Name, Type Type)
        {
            if (TypeIndex.ContainsKey(Type))
            {
                throw new Exception("Type already aliased");
            }

            TypeIndex[Type] = NameIndex[Name];
        }

        /// <summary>
        /// Get the number of type infos held.
        /// </summary>
        public Int32 Count
        {
            get { return NameIndex.Count; }
        }

        public bool TryGetValue(string name, out EDBNativeTypeInfo typeInfo)
        {
            return NameIndex.TryGetValue(name, out typeInfo);
        }

        /// <summary>
        /// Retrieve the EDBNativeTypeInfo with the given EDBDbType.
        /// </summary>
        public bool TryGetValue(EDBDbType dbType, out EDBNativeTypeInfo typeInfo)
        {
            return EDBDbTypeIndex.TryGetValue(dbType, out typeInfo);
        }

        /// <summary>
        /// Retrieve the EDBNativeTypeInfo with the given DbType.
        /// </summary>
        public bool TryGetValue(DbType dbType, out EDBNativeTypeInfo typeInfo)
        {
            return DbTypeIndex.TryGetValue(dbType, out typeInfo);
        }

        /// <summary>
        /// Retrieve the EDBNativeTypeInfo with the given Type.
        /// </summary>
        public bool TryGetValue(Type type, out EDBNativeTypeInfo typeInfo)
        {
            return TypeIndex.TryGetValue(type, out typeInfo);
        }

        /// <summary>
        /// Determine if a EDBNativeTypeInfo with the given backend type name exists in this mapping.
        /// </summary>
        public Boolean ContainsName(String Name)
        {
            return NameIndex.ContainsKey(Name);
        }

        /// <summary>
        /// Determine if a EDBNativeTypeInfo with the given EDBDbType exists in this mapping.
        /// </summary>
        public Boolean ContainsEDBDbType(EDBDbType EDBDbType)
        {
            return EDBDbTypeIndex.ContainsKey(EDBDbType);
        }

        /// <summary>
        /// Determine if a EDBNativeTypeInfo with the given Type name exists in this mapping.
        /// </summary>
        public Boolean ContainsType(Type Type)
        {
            return TypeIndex.ContainsKey(Type);
        }
    }
}
