// created on 30/09/2014 at 18:33

// EnterpriseDB.EDBClient.EDBParseOut.cs
//
// Author:
//   Zahid Khan 

using System;
using System.IO;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// This class represents the Parse message sent to PostgreSQL
    /// server.
    /// </summary>
    ///
    internal sealed class EDBParseOut : ClientMessage
    {
        // Logging related values
        //private static readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;

        private readonly byte[] _bPrepareName;
        private readonly byte[] _bQueryString;
        private readonly Int32[] _parameterIDs;

        /* EnterpriseDB Team */
        private EDBParameterCollection _parameters;
        private String _commandType;
    

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EDBParseOut(String prepareName, byte[] queryString, Int32[] parameterIDs)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _bPrepareName = BackendEncoding.UTF8Encoding.GetBytes(prepareName);
            _bQueryString = queryString;

            _parameterIDs = parameterIDs;
        }
        public EDBParseOut(String prepareName, string queryString, Int32[] parameterIDs, EDBParameterCollection parameters, EDBCommand command)
        {
            _bPrepareName = BackendEncoding.UTF8Encoding.GetBytes(prepareName);
            _bQueryString = BackendEncoding.UTF8Encoding.GetBytes(queryString); ;

            _parameterIDs = parameterIDs;
            _parameters = parameters;
            _commandType = command.CommandType.ToString();
        }
	
        public override void WriteToStream(Stream outputStream)
        {
            outputStream.WriteByte((byte)FrontEndMessageCode.ParseOut);
            // message length =
            // Int32 self
            // name of prepared statement + 1 null string terminator +
            // query string + 1 null string terminator
            // + Int16 (number of parameters)
            // + Int32 * number of parameters
            // + int16 * number of parameters
            Int32 messageLength = 4 + _bPrepareName.Length + 1 + _bQueryString.Length + 1 + 2 + (_parameters.Count * 6);

            outputStream
                .WriteInt32(messageLength)
                .WriteBytesNullTerminated(_bPrepareName)
                .WriteBytesNullTerminated(_bQueryString)
                .WriteInt16((Int16)_parameters.Count);

            //parameter OIDs
            //_parameters[i].TypeInfo.
           // TODO ZK Delme EDBTypes.EDBDbType OID = _parameters[0].EDBDbType;
            for (Int32 i = 0; i < _parameters.Count; i++)
                PGUtil.WriteInt32(outputStream, Convert.ToInt32(EDBParameter.ParamToOid(_parameters[i].TypeInfo.Name.ToString())));
            //EDBOids.ParamOid.int4
            for (Int32 i = 0; i < _parameters.Count; i++)
            {
                PGUtil.WriteInt16(outputStream, Convert.ToInt16(EDBParameter.NetParamDirectionToEDBParamDirection(_parameters[i].Direction)));

            }
        }
    }
}
