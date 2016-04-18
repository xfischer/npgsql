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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using  EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using AsyncRewriter;

namespace  EnterpriseDB.EDBClient
{
    interface ITypeReader<T> {}

    #region Simple type handler

    interface ISimpleTypeWriter
    {
        int ValidateAndGetLength(object value, EDBParameter parameter);
        void Write(object value, EDBBuffer buf, EDBParameter parameter);
    }

    /// <summary>
    /// A handler which can read small, usually fixed-length values.
    /// </summary>
    /// <typeparam name="T">the type of the value returned by this type handler</typeparam>
    //[ContractClass(typeof(ITypeHandlerContract<>))]
    // ReSharper disable once TypeParameterCanBeVariant
    interface ISimpleTypeReader<T> : ITypeReader<T>
    {
        /// <summary>
        /// The entire data required to read the value is expected to be in the buffer.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="len"></param>
        /// <param name="fieldDescription"></param>
        /// <returns></returns>
        T Read(EDBBuffer buf, int len, FieldDescription fieldDescription=null);
    }

    #endregion

    [ContractClass(typeof(IChunkingTypeWriterContracts))]
    interface IChunkingTypeWriter
    {
        /// <param name="value">the value to be examined</param>
        /// <param name="lengthCache">a cache in which to store length(s) of values to be written</param>
        /// <param name="parameter">
        /// the <see cref="EDBParameter"/> containing <paramref name="value"/>. Consulted for settings
        /// which impact how to send the parameter, e.g. <see cref="EDBParameter.Size"/>. Can be null.
        /// </param>
        int ValidateAndGetLength(object value, ref LengthCache lengthCache, EDBParameter parameter);

        /// <param name="value">the value to be written</param>
        /// <param name="buf"></param>
        /// <param name="lengthCache">a cache in which to store length(s) of values to be written</param>
        /// <param name="parameter">
        /// the <see cref="EDBParameter"/> containing <paramref name="value"/>. Consulted for settings
        /// which impact how to send the parameter, e.g. <see cref="EDBParameter.Size"/>. Can be null.
        /// <see cref="EDBParameter.Size"/>.
        /// </param>
        void PrepareWrite(object value, EDBBuffer buf, LengthCache lengthCache, EDBParameter parameter);
        bool Write(ref DirectBuffer directBuf);
    }

    [ContractClassFor(typeof(IChunkingTypeWriter))]
    // ReSharper disable once InconsistentNaming
    class IChunkingTypeWriterContracts : IChunkingTypeWriter
    {
        public int ValidateAndGetLength(object value, ref LengthCache lengthCache, EDBParameter parameter=null)
        {
            Contract.Requires(value != null);
            return default(int);
        }

        public void PrepareWrite(object value, EDBBuffer buf, LengthCache lengthCache, EDBParameter parameter=null)
        {
            Contract.Requires(buf != null);
            Contract.Requires(value != null);
        }

        public bool Write(ref DirectBuffer directBuf)
        {
            Contract.Ensures(Contract.Result<bool>() == false || directBuf.Buffer == null);
            return default(bool);
        }
    }

    /// <summary>
    /// A type handler which handles values of totally arbitrary length, and therefore supports chunking them.
    /// </summary>
    [ContractClass(typeof(IChunkingTypeReaderContracts<>))]
    // ReSharper disable once TypeParameterCanBeVariant
    interface IChunkingTypeReader<T> : ITypeReader<T>
    {
        void PrepareRead(EDBBuffer buf, int len, FieldDescription fieldDescription=null);
        bool Read(out T result);
    }

    [ContractClassFor(typeof(IChunkingTypeReader<>))]
    // ReSharper disable once InconsistentNaming
    class IChunkingTypeReaderContracts<T> : IChunkingTypeReader<T>
    {
        public void PrepareRead(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            Contract.Requires(buf != null);
        }

        public bool Read(out T result)
        {
            //Contract.Ensures(!completed || Contract.ValueAtReturn(out result) == default(T));
            result = default(T);
            return default(bool);
        }
    }

    internal abstract partial class TypeHandler
    {
        internal string PgName { get; set; }
        internal uint OID { get; set; }
        internal EDBDbType EDBDbType { get; set; }
        internal abstract Type GetFieldType(FieldDescription fieldDescription=null);
        internal abstract Type GetProviderSpecificFieldType(FieldDescription fieldDescription=null);

        internal abstract object ReadValueAsObject(DataRowMessage row, FieldDescription fieldDescription);

        internal virtual object ReadPsvAsObject(DataRowMessage row, FieldDescription fieldDescription)
        {
            return ReadValueAsObject(row, fieldDescription);
        }

        public virtual bool PreferTextWrite { get { return false; } }

        [RewriteAsync]
        internal T Read<T>(DataRowMessage row, int len, FieldDescription fieldDescription = null)
        {
            Contract.Requires(row.PosInColumn == 0);
            Contract.Ensures(row.PosInColumn == row.ColumnLen);

            T result;
            try
            {
                result = Read<T>(row.Buffer, len, fieldDescription);
            }
            finally
            {
                // Important in case a SafeReadException was thrown, position must still be updated
                row.PosInColumn += row.ColumnLen;
            }
            return result;
        }

        [RewriteAsync]
        internal T Read<T>(EDBBuffer buf, int len, FieldDescription fieldDescription=null)
        {
            T result;

            var asSimpleReader = this as ISimpleTypeReader<T>;
            if (asSimpleReader != null)
            {
                buf.Ensure(len);
                result = asSimpleReader.Read(buf, len, fieldDescription);
            }
            else
            {
                var asChunkingReader = this as IChunkingTypeReader<T>;
                if (asChunkingReader == null) {
                    if (fieldDescription == null)
                        throw new InvalidCastException("Can't cast database type to " + typeof(T).Name);
                    throw new InvalidCastException(String.Format("Can't cast database type {0} to {1}", fieldDescription.Handler.PgName, typeof(T).Name));
                }

                asChunkingReader.PrepareRead(buf, len, fieldDescription);
                while (!asChunkingReader.Read(out result)) {
                    buf.ReadMore();
                }
            }

            return result;
        }

        protected Exception CreateConversionException(Type clrType)
        {
            return new InvalidCastException(string.Format("Can't convert .NET type {0} to PostgreSQL {1}", clrType, PgName));
        }

        protected Exception CreateConversionButNoParamException(Type clrType)
        {
            return new InvalidCastException(string.Format("Can't convert .NET type {0} to PostgreSQL {1} within an array", clrType, PgName));
        }

        [ContractInvariantMethod]
        void ObjectInvariants()
        {
            Contract.Invariant(!(this is IChunkingTypeWriter && this is ISimpleTypeWriter));
        }
    }

    internal abstract class TypeHandler<T> : TypeHandler
    {
        internal override Type GetFieldType(FieldDescription fieldDescription)
        {
            return typeof(T);
        }

        internal override Type GetProviderSpecificFieldType(FieldDescription fieldDescription)
        {
            return typeof(T);
        }

        internal override object ReadValueAsObject(DataRowMessage row, FieldDescription fieldDescription)
        {
            return Read<T>(row, row.ColumnLen, fieldDescription);
        }

        internal override object ReadPsvAsObject(DataRowMessage row, FieldDescription fieldDescription)
        {
            return Read<T>(row, row.ColumnLen, fieldDescription);
        }

        [ContractInvariantMethod]
        void ObjectInvariants()
        {
            Contract.Invariant(this is ISimpleTypeReader<T> || this is IChunkingTypeReader<T>);
        }
    }

    /// <summary>
    /// A marking interface to allow us to know whether a given type handler has a provider-specific type
    /// distinct from its regular type
    /// </summary>
    internal interface ITypeHandlerWithPsv {}

    /// <summary>
    /// A type handler that supports a provider-specific value which is different from the regular value (e.g.
    /// EDBDate and DateTime)
    /// </summary>
    /// <typeparam name="T">the regular value type returned by this type handler</typeparam>
    /// <typeparam name="TPsv">the type of the provider-specific value returned by this type handler</typeparam>
    internal abstract class TypeHandlerWithPsv<T, TPsv> : TypeHandler<T>, ITypeHandlerWithPsv
    {
        internal override Type GetProviderSpecificFieldType(FieldDescription fieldDescription)
        {
            return typeof (TPsv);
        }

        internal override object ReadPsvAsObject(DataRowMessage row, FieldDescription fieldDescription)
        {
            return Read<TPsv>(row, row.ColumnLen, fieldDescription);
        }
    }

    struct DirectBuffer
    {
        public byte[] Buffer;
        public int Offset;
        public int Size;
    }

    /// <summary>
    /// Can be thrown by readers to indicate that interpreting the value failed, but the value was read wholly
    /// and it is safe to continue reading. Any other exception is assumed to leave the row in an unknown state
    /// and the connector is therefore set to Broken.
    /// Note that an inner exception is mandatory, and will get thrown to the user instead of the SafeReadException.
    /// </summary>
    internal class SafeReadException : Exception
    {
        public SafeReadException(Exception innerException) : base("", innerException)
        {
            Contract.Requires(innerException != null);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    class TypeMappingAttribute : Attribute
    {
        /// <summary>
        /// Maps an  EnterpriseDB.EDBClient type handler to a PostgreSQL type.
        /// </summary>
        /// <param name="pgName">A PostgreSQL type name as it appears in the pg_type table.</param>
        /// <param name="edbDbType">
        /// A member of <see cref="EDBDbType"/> which represents this PostgreSQL type.
        /// An <see cref="EDBParameter"/> with <see cref="EDBParameter.EDBDbType"/> set to
        /// this value will be sent with the type handler mapped by this attribute.
        /// </param>
        /// <param name="dbTypes">
        /// All members of <see cref="DbType"/> which represent this PostgreSQL type.
        /// An <see cref="EDBParameter"/> with <see cref="EDBParameter.DbType"/> set to
        /// one of these values will be sent with the type handler mapped by this attribute.
        /// </param>
        /// <param name="types">
        /// Any .NET type which corresponds to this PostgreSQL type.
        /// An <see cref="EDBParameter"/> with <see cref="EDBParameter.Value"/> set to
        /// one of these values will be sent with the type handler mapped by this attribute.
        /// </param>
        /// <param name="inferredDbType">
        /// The "primary" <see cref="DbType"/> which best corresponds to this PostgreSQL type.
        /// When <see cref="EDBParameter.EDBDbType"/> or <see cref="EDBParameter.Value"/>
        /// set, <see cref="EDBParameter.DbType"/> will be set to this value.
        /// </param>
        internal TypeMappingAttribute(string pgName, EDBDbType? edbDbType, DbType[] dbTypes, Type[] types, DbType? inferredDbType)
        {
            if (String.IsNullOrWhiteSpace(pgName))
                throw new ArgumentException("pgName can't be empty", "pgName");
            Contract.EndContractBlock();

            PgName = pgName;
            EDBDbType = edbDbType;
            DbTypes = dbTypes ?? new DbType[0];
            Types = types ?? new Type[0];
            InferredDbType = inferredDbType;
        }

        internal TypeMappingAttribute(string pgName, EDBDbType edbDbType, DbType[] dbTypes, Type[] types, DbType inferredDbType)
            : this(pgName, (EDBDbType?)edbDbType, dbTypes, types, inferredDbType) {}

        //internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, DbType[] dbTypes=null, Type type=null)
        //    : this(pgName, EDBDbType, dbTypes, type == null ? null : new[] { type }) {}

        internal TypeMappingAttribute(string pgName, EDBDbType edbDbType)
            : this(pgName, edbDbType, new DbType[0], new Type[0], null) { }

        internal TypeMappingAttribute(string pgName, EDBDbType edbDbType, DbType inferredDbType)
            : this(pgName, edbDbType, new DbType[0], new Type[0], inferredDbType) { }

        internal TypeMappingAttribute(string pgName, EDBDbType edbDbType, DbType[] dbTypes, Type type, DbType inferredDbType)
            : this(pgName, edbDbType, dbTypes, new[] { type }, inferredDbType) { }

        internal TypeMappingAttribute(string pgName, EDBDbType edbDbType, DbType dbType, Type[] types)
            : this(pgName, edbDbType, new[] { dbType }, types, dbType) {}

        internal TypeMappingAttribute(string pgName, EDBDbType edbDbType, DbType dbType, Type type=null)
            : this(pgName, edbDbType, new[] { dbType }, type == null ? null : new[] { type }, dbType) {}

        internal TypeMappingAttribute(string pgName, EDBDbType edbDbType, Type[] types, DbType inferredDbType)
            : this(pgName, edbDbType, new DbType[0], types, inferredDbType) { }

        internal TypeMappingAttribute(string pgName, EDBDbType edbDbType, Type[] types)
            : this(pgName, edbDbType, new DbType[0], types, null) { }

        internal TypeMappingAttribute(string pgName, EDBDbType edbDbType, Type type, DbType inferredDbType)
            : this(pgName, edbDbType, new DbType[0], new[] { type }, inferredDbType) { }

        internal TypeMappingAttribute(string pgName, EDBDbType edbDbType, Type type)
            : this(pgName, edbDbType, new DbType[0], new[] { type }, null) {}

        /// <summary>
        /// Read-only parameter
        /// </summary>
        internal TypeMappingAttribute(string pgName)
            : this(pgName, null, null, null, null) {}

        internal string PgName { get; private set; }
        internal EDBDbType? EDBDbType { get; private set; }
        internal DbType[] DbTypes { get; private set; }
        internal Type[] Types { get; private set; }
        internal DbType? InferredDbType { get; private set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[{0} EDBDbType={1}", PgName, EDBDbType);
            if (DbTypes.Length > 0) {
                sb.Append(" DbTypes=");
                sb.Append(String.Join(",", DbTypes.Select(t => t.ToString())));
            }
            if (Types.Length > 0) {
                sb.Append(" Types=");
                sb.Append(String.Join(",", Types.Select(t => t.Name)));
            }
            sb.AppendFormat("]");
            return sb.ToString();
        }

        [ContractInvariantMethod]
        void ObjectInvariants()
        {
            Contract.Invariant(!String.IsNullOrWhiteSpace(PgName));
            Contract.Invariant(Types != null);
            Contract.Invariant(DbTypes != null);
        }
    }
}
