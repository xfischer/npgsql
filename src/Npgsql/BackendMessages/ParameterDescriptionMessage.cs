using System.Collections.Generic;

namespace EnterpriseDB.EDBClient.BackendMessages
{
    class ParameterDescriptionMessage : IBackendMessage
    {
        // ReSharper disable once InconsistentNaming
        internal List<uint> TypeOIDs { get; }

        internal ParameterDescriptionMessage()
        {
            TypeOIDs = new List<uint>();
        }

        internal ParameterDescriptionMessage Load(EDBReadBuffer buf)
        {
            var numParams = buf.ReadInt16();
            TypeOIDs.Clear();
            for (var i = 0; i < numParams; i++)
                TypeOIDs.Add(buf.ReadUInt32());
            return this;
        }

        public BackendMessageCode Code => BackendMessageCode.ParameterDescription;
    }
}
