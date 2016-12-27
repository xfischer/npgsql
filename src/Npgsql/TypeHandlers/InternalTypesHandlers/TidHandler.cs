using System;
using System.Diagnostics.Contracts;
using System.Net.NetworkInformation;
using  EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;

namespace  EnterpriseDB.EDBClient.TypeHandlers.InternalTypesHandlers
{
    [TypeMapping("tid", EDBDbType.Tid, typeof(EDBTid))]
    internal class TidHandler : SimpleTypeHandler<EDBTid>, ISimpleTypeHandler<string>
    {
        internal TidHandler(IBackendType backendType) : base(backendType) { }

        public override EDBTid Read(ReadBuffer buf, int len, FieldDescription fieldDescription)
        {
            Contract.Assume(len == 6);

            uint blockNumber = buf.ReadUInt32();
            ushort offsetNumber = buf.ReadUInt16();

            return new EDBTid(blockNumber, offsetNumber);
        }

        string ISimpleTypeHandler<string>.Read(ReadBuffer buf, int len, FieldDescription fieldDescription)
        {
            return Read(buf, len, fieldDescription).ToString();
        }

        public override int ValidateAndGetLength(object value, EDBParameter parameter)
        {
            if (!(value is EDBTid))
                throw CreateConversionException(value.GetType());
            return 6;
        }

        public override void Write(object value, WriteBuffer buf, EDBParameter parameter)
        {
            var tid = (EDBTid)value;
            buf.WriteUInt32(tid.BlockNumber);
            buf.WriteUInt16(tid.OffsetNumber);
        }
    }
}
