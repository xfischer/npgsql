using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.TypeHandling;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// The default, non-sequential reader, which buffers entire rows in memory.
    /// </summary>
#pragma warning disable CA1010
    sealed class EDBDefaultDataReader : EDBDataReader
#pragma warning restore CA1010
    {
        /// <summary>
        /// The number of columns in the current row
        /// </summary>
        int _column;
        List<(int Offset, int Length)> _columns = new List<(int Offset, int Length)>();//EnterpriseDB Team
        List<(int Offset, int Length)> _retColumns = new List<(int Offset, int Length)>();//EnterpriseDB Team
        int _dataMsgEnd;
        internal bool _isReturnRow = true; //EnterpriseDB Team
        bool isOutParamReceived;//EnterpriseDB Team
        internal int _InternalreadPosition;//EnterpriseDB Team
        internal int _InternalActaullReadPosition;//EnterpriseDB Team
        RowDescriptionMessage _callable_descrition; //EDB

        internal EDBDefaultDataReader(EDBConnector connector) : base(connector) {}

        internal override ValueTask<IBackendMessage> ReadMessage(bool async)
            => Connector.ReadMessage(async);

        protected override Task<bool> NextResult(bool async, bool isConsuming=false)
        {
            return (Command.Parameters.HasOutputParameters || Command.Parameters._hasReturnParam) && StatementIndex == -1
                ? NextResultWithOutputParams()
                : base.NextResult(async, isConsuming);

            async Task<bool> NextResultWithOutputParams()
            {
                var hasResultSet = await base.NextResult(async, isConsuming);

                if (Command.CommandType == CommandType.StoredProcedure && (Command.Parameters.HasOutputParameters || Command.Parameters._hasReturnParam))//EnterpriseDB Team
                {
                    PopulateOutputParameters();
                }

                    if (!hasResultSet || !HasRows)
                    return hasResultSet;

                // The first row in a stored procedure command that has output parameters needs to be traversed twice -
                // once for populating the output parameters and once for the actual result set traversal. So in this
                // case we can't be sequential.
                Debug.Assert(Command.Parameters.Any(p => p.IsOutputDirection));
                Debug.Assert(StatementIndex == 0);
                Debug.Assert(RowDescription != null);
                Debug.Assert(State == ReaderState.BeforeResult);

                // Temporarily set our state to InResult to allow us to read the values
                State = ReaderState.InResult;

                var pending = new Queue<EDBParameter>();
                var taken = new List<int>();
                foreach (var p in Command.Parameters.Where(p => p.IsOutputDirection))
                {
                    if (RowDescription.TryGetFieldIndex(p.TrimmedName, out var idx))
                    {
                        // TODO: Provider-specific check?
                        p.Value = GetValue(idx);
                        taken.Add(idx);
                    }
                    else
                        pending.Enqueue(p);
                }
                for (var i = 0; pending.Count != 0 && i != RowDescription.NumFields; ++i)
                {
                    // TODO: Need to get the provider-specific value based on the out param's type
                    if (!taken.Contains(i))
                        pending.Dequeue().Value = GetValue(i);
                }

                State = ReaderState.BeforeResult;  // Set the state back

                return hasResultSet;
            }
        }

        void PopulateOutputParameters()//EnterpriseDB Team
        {
            var paramdata = false;
            var retDataFetched = false;
            var done = false;
            isOutParamReceived = false;
            // TODO: Should we really use Contract here, instead of throwing an Exception?
            Debug.Assert(RowDescription != null);
            Debug.Assert(Command.Parameters.Any(p => p.IsOutputDirection) || Command.Parameters._hasReturnParam);

            //var asDataRow = _pendingMessage as DataRowMessage;
            //if (Command.CommandType != CommandType.StoredProcedure && asDataRow == null) // The first resultset was empty
              //  return;

            while (done != true)
            {
                var msg = Connector.ReadMessage(DataRowLoadingMode.NonSequential);
                if (msg.Code == BackendMessageCode.CompletedResponse && paramdata != true)
                    continue;
                else
                    switch (msg.Code)
                    {
                        case BackendMessageCode.DataRow:
                           // _pendingMessage = msg;
                            paramdata = true;

                            if (Command.CommandType == CommandType.StoredProcedure)
                            {


                                if (Command.Parameters._hasReturnParam && retDataFetched != true)
                                {
                                    //_tempDataRow = (DataRowNonSequentialMessage)msg;
                                    _isReturnRow = false;
                                    retDataFetched = true;
                                    paramdata = false;
                                    //ConsumeRow(false);
                                    continue;
                                }
                                else
                                {
                                    //_outRow = (DataRowNonSequentialMessage)msg;
                                    //_row = _outRow; // _tempDataRow; ZK 
                                    
                                    if (Command.Parameters.HasOutputParameters && !isOutParamReceived)
                                    {
                                        done = false;
                                    } else
                                    {
                                        done = true;
                                    }
                                    continue;

                                }
                            }
                            else
                            {
                                //_row = (DataRowNonSequentialMessage)msg;
                                _isReturnRow = false;
                                done = true;
                                break;
                            }
                        case BackendMessageCode.CompletedResponse:
                        case BackendMessageCode.EmptyQueryResponse:
                           // _pendingMessage = msg;
                           if (Command.Parameters.HasOutputParameters && !isOutParamReceived)
                            {
                                continue;
                            }
                            return;
                        case BackendMessageCode.OutDescription:
                        case BackendMessageCode.RowDescription:
                            if (Command.Parameters.Any(p => p.IsOutputDirection))
                            {
                                _callable_descrition = (RowDescriptionMessage)msg;
                            }
                            else
                            {
                                _callable_descrition = RowDescription;
                            }
                            continue;



                        //        var description = (RowDescriptionMessage)msg;
                        //   FixupRowDescription(description, _queryIndex == 0);
                        // _queries[_queryIndex].Description = description;
                        //   if ((behavior & CommandBehavior.SchemaOnly) != 0) {
                        //       _queryIndex++;
                        //}

                        case BackendMessageCode.BindComplete:
                        case BackendMessageCode.ParameterDescription:
                        case BackendMessageCode.NoData:
                            if (!Command.Parameters.Any(p => p.IsOutputDirection))
                            {
                                _callable_descrition = RowDescription;
                            }
                            continue;

                        default:
                            throw new ArgumentOutOfRangeException("Unexpected message type while populating output parameter: " + msg.Code);
                    }
            }
            var tmp = new byte[8500];
            if (Command.CommandType == CommandType.StoredProcedure)
            {
                RowDescription = _callable_descrition;
                if (Command.Parameters._hasReturnParam)
                {
                    Array.Copy(Buffer.Buffer, tmp, Buffer.Buffer.Length);
                    Command.Parameters.Insert(Command.Parameters.ReturnIndex, Command.Parameters.ReturnParam);
                    RowDescription.AddReturnData((FieldDescription)_callable_descrition[0]);
                    Add(RetRowBuffer); // ZK
                }
            }

            if (Command.Parameters.Any(p => p.IsOutputDirection))
            {
                //check Debug.Assert(RowDescription.NumFields == _row.NumColumns);
            }

            //      if (IsCaching) { _rowCache.Clear(); }

            var pending = new Queue<EDBParameter>();
            var taken = new List<int>();

            foreach (var p in Command.Parameters.Where(p => p.IsOutReturnDirection))
            {
                int idx;
                if (RowDescription.TryGetFieldIndex(p.TrimmedName, out idx))
                {
                    // TODO: Provider-specific check?
                    p.Value = GetValue(idx);
                    taken.Add(idx);
                }
                else
                {
                    pending.Enqueue(p);
                }
            }
            for (var i = 0; pending.Count != 0 && i != RowDescription.NumFields; ++i)
            {
                if (!taken.Contains(i))
                {
                    // TODO: Need to get the provider-specific value based on the out param's type
                    pending.Dequeue().Value = GetValue(i);
                    //   Console.WriteLine((string)pending.Dequeue().Value.ToString());
                }
            }


            if (Command.Parameters._hasReturnParam)
            {
                Buffer.Buffer = tmp;
                Buffer.Seek(_InternalActaullReadPosition, SeekOrigin.Begin);
                var msg = Connector.ReadMessage(DataRowLoadingMode.NonSequential);
                State = ReaderState.Consumed;
                //     if (msg.Code == BackendMessageCode.CompletedResponse )
                {
                    //       _state = ReaderState.Consumed;

                }
            }
            // _state = ReaderState.Consumed;
        }

        internal void Add(EDBReadBuffer retRowBuf)//EnterpriseDB Team 

        {
            _InternalActaullReadPosition = Buffer.ReadPosition;
            //   ReadBuffer buf_new = new ReadBuffer(Buffer.Connector, Buffer.Connector.Connection. + 100, Buffer.TextEncoding);
            var rd = new EDBReadBuffer(Buffer.Connector, Buffer.Underlying, 4096, Buffer.TextEncoding);
            var wt = new EDBWriteBuffer(Buffer.Connector, Buffer.Underlying, 4096, Buffer.TextEncoding);

            var buf_new_length = Buffer.Size;// retRow.Buffer.Size - sizeof(Int16);

            //TODO     retRow.Buffer.Seek(retRow._InternalreadPosition, SeekOrigin.Begin);

            retRowBuf.Seek(_InternalreadPosition, SeekOrigin.Begin);

            // ISSUE 1 - Starting Position = Buffer.Seek(0, SeekOrigin.Begin);

            var RetRowNumColumns = (int)retRowBuf.ReadInt16();
            var OutParamNumColumns = (int)_columns.Count;
            var TotalNumColumns = OutParamNumColumns + RetRowNumColumns;
            //NumColumns = TotalNumColumns;

            wt.WriteInt16(TotalNumColumns);

            // Get length of the buffer retRow
            _column = -1;
            ColumnLen = -1;
            PosInColumn = 0;

            // TODO: Recycle message objects rather than recreating for each row
            //_columnOffsets = new List<int>(TotalNumColumns);
            var AllColumnOffsets = new List<(int Offset, int Length)>();//EnterpriseDB Team

            for (var i = 0; i < OutParamNumColumns; i++)
            {

                var wp = wt.WritePosition;
                Buffer.Seek(_columns[i].Offset, SeekOrigin.Begin);
                var len = Buffer.ReadInt32();
                wt.WriteInt32(len);
                AllColumnOffsets.Add((wp, len));
                if (len != -1)
                {
                    var output_data = new byte[len + 1];

                    Buffer.ReadBytes(output_data, (int)SeekOrigin.Current, len);
                    wt.WriteBytes(output_data, (int)SeekOrigin.Current, len);
                }
            }

            for (var i = 0; i < RetRowNumColumns; i++)
            {
                var wp = wt.WritePosition;
                retRowBuf.Seek(_retColumns[i].Offset, SeekOrigin.Begin);
                var len = retRowBuf.ReadInt32();
                AllColumnOffsets.Add((wp, len));
                wt.WriteInt32(len);

                if (len != -1)
                {
                    var output_data = new byte[len + 1];

                    retRowBuf.ReadBytes(output_data, (int)SeekOrigin.Current, len);
                    wt.WriteBytes(output_data, (int)SeekOrigin.Current, len);
                }
            }

            //_endOffset = wt.WritePosition;
            //TODO   buf_new.SetFilledBytes = _endOffset;

            Buffer.Buffer = wt.Buffer;

            _columns = AllColumnOffsets;

            // Both buffers start at 108 bytes
            // We need to preserve _columnOffsets for outparams

            /*
                        retRow.Buffer.Seek(retRow._InternalreadPosition, SeekOrigin.Begin);

                        EDBBuffer buf_new = new EDBBuffer(Buffer.Underlying, Buffer.Size + 100, Buffer.TextEncoding);
                        int buf_new_length = Buffer.Size + 100;// retRow.Buffer.Size - sizeof(Int16);

                        int TotalNumColumns = NumColumns + 1;  // add 1 ret param
                        //  buf_new = buf_new.EnsureOrAllocateTemp(buf_new_length);
                        NumColumns = NumColumns + 1;

                        Column = -1;
                        ColumnLen = -1;
                        PosInColumn = 0;
                        // TODO: Recycle message objects rather than recreating for each row
                        _columnOffsets = new List<int>(TotalNumColumns);
                        for (var i = 0; i < 1 ; i++)
                        {
                            int tmpColumn = retRow.Buffer.ReadInt16();
                            _columnOffsets.Add(retRow.Buffer.ReadPosition);
                            var len = retRow.Buffer.ReadInt32();
                            Buffer.WriteInt32(len);

                            if (len != -1)
                            {
                                byte[] output_data = new byte[len+1];

                                retRow.Buffer.ReadBytes(output_data, (int)SeekOrigin.Current, len);
                                Buffer.WriteBytes(output_data, (int)SeekOrigin.Current, len);
                            }
                        }
                    //    Buffer = buf_new;
            */



            //check return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override Task ConsumeRow(bool async)
        {
            //checkDebug.Assert(State == ReaderState.InResult || State == ReaderState.BeforeResult);

            if (ColumnStream != null)
            {
                ColumnStream.Dispose();
                ColumnStream = null;
            }
            Buffer.ReadPosition = _dataMsgEnd;
            return PGUtil.CompletedTask;
        }

        internal override void ProcessDataMessage(DataRowMessage dataMsg)
        {
            // The connector's buffer can actually change between DataRows:
            // If a large DataRow exceeding the connector's current read buffer arrives, and we're
            // reading in non-sequential mode, a new oversize buffer is allocated. We thus have to
            // recapture the connector's buffer on each new DataRow.
            Buffer = Connector.ReadBuffer;
            _dataMsgEnd = Buffer.ReadPosition + dataMsg.Length;

            // We currently assume that the row's number of columns is identical to the description's
#if DEBUG
            var numColumns = Buffer.ReadInt16();
            Debug.Assert(RowDescription.NumFields == numColumns);
#else
            Buffer.ReadPosition += 2;
#endif
            _column = -1;
            _columns.Clear();

            // Initialize our columns array with the offset and length of the first column
            var len = Buffer.ReadInt32();
            _columns.Add((Buffer.ReadPosition, len));
        }

        internal override void ProcessDataRowMessage(EDBReadBuffer buf, bool isReturnRow)
        {
            if (Command.CommandType == CommandType.StoredProcedure && Command.IsPrepared)
            {
                if (isReturnRow)
                {
                    _InternalreadPosition = buf.ReadPosition;//EnterpriseDB Team
                } else
                {
                    isOutParamReceived = true;
                }
                
                var numColumns = buf.ReadInt16();
                if (isReturnRow)
                {
                    RetRowBuffer = buf;
                    _retColumns.Clear();
                }   
                else {
                    Buffer = buf;
                    _columns.Clear();
                }
                _column = -1;
                ColumnLen = -1;
                PosInColumn = 0;
                // TODO: One big row with many columns will make the DataRow's _columnOffsets big forever...
                
                for (var i = 0; i < numColumns; i++)
                {
                    var pos = buf.ReadPosition;
                    var len = buf.ReadInt32();
                    //_columns.Add(Buffer.ReadPosition, len);
                    
                        if (isReturnRow)
                            _retColumns.Add((pos, len));
                        else
                            _columns.Add((pos, len));
                    if (len != -1)
                    {
                        buf.Seek(len, SeekOrigin.Current);
                    }
                }
                // _endOffset = Buffer.ReadPosition;
            }

        }

        // We know the entire row is buffered in memory (non-sequential reader), so no I/O will be performed
        public override Task<T> GetFieldValueAsync<T>(int column, CancellationToken cancellationToken)
            => Task.FromResult(GetFieldValue<T>(column));

        public override T GetFieldValue<T>(int column)
        {
            CheckRowAndOrdinal(column);

            SeekToColumn(column);
            if (ColumnLen == -1)
            {
                if (NullableHandler<T>.Exists)
                    return default;
                else
                    throw new InvalidCastException("Column is null");
            }

            var fieldDescription = RowDescription[column];
            try
            {
                if (NullableHandler<T>.Exists)
                    return NullableHandler<T>.Read(Buffer, ColumnLen, fieldDescription);

                return typeof(T) == typeof(object)
                    ? (T)fieldDescription.Handler.ReadAsObject(Buffer, ColumnLen, fieldDescription)
                    : fieldDescription.Handler.Read<T>(Buffer, ColumnLen, fieldDescription);
            }
            catch (EDBSafeReadException e)
            {
                throw e.InnerException;
            }
            catch
            {
                Connector.Break();
                throw;
            }
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="object"/>.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override object GetValue(int ordinal)
        {
            CheckRowAndOrdinal(ordinal);

            
            if (Command.CommandType == CommandType.StoredProcedure)
                SeekToColumnStart(ordinal, false).GetAwaiter().GetResult();
            else
                SeekToColumn(ordinal);
            if (ColumnLen == -1)
                return DBNull.Value;

            var fieldDescription = RowDescription[ordinal];
            object result;
            try
            {
                result = fieldDescription.Handler.ReadAsObject(Buffer, ColumnLen, fieldDescription);
            }
            catch (EDBSafeReadException e)
            {
                throw e.InnerException;
            }
            catch
            {
                Connector.Break();
                throw;
            }

            // Used for Entity Framework <= 6 compability
            if (Command.ObjectResultTypes?[ordinal] != null)
            {
                var type = Command.ObjectResultTypes[ordinal];
                result = type == typeof(DateTimeOffset)
                    ? new DateTimeOffset((DateTime)result)
                    : Convert.ChangeType(result, type);
            }

            return result;
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="object"/>.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override object GetProviderSpecificValue(int ordinal)
        {
            CheckRowAndOrdinal(ordinal);

            SeekToColumn(ordinal);
            if (ColumnLen == -1)
                return DBNull.Value;

            var fieldDescription = RowDescription[ordinal];
            object result;
            try
            {
                // TODO: Maybe call a non-async method which would allow simple type handlers (and
                // maybe also text) to read without going through async...
                result = fieldDescription.Handler.ReadPsvAsObject(Buffer, ColumnLen, fieldDescription);
            }
            catch (EDBSafeReadException e)
            {
                throw e.InnerException;
            }
            catch
            {
                Connector.Break();
                throw;
            }

            return result;
        }

        /// <summary>
        /// Gets a value that indicates whether the column contains nonexistent or missing values.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns><b>true</b> if the specified column is equivalent to <see cref="DBNull"/>; otherwise <b>false</b>.</returns>
        public override bool IsDBNull(int ordinal)
        {
            CheckRowAndOrdinal(ordinal);

            SeekToColumn(ordinal);
            return ColumnLen == -1;
        }

        void SeekToColumn(int column)
        {
            // Shut down any streaming going on on the column
            if (ColumnStream != null)
            {
                ColumnStream.Dispose();
                ColumnStream = null;
            }

            for (var lastColumnRead = _columns.Count; column >= lastColumnRead; lastColumnRead++)
            {
                int lastColumnLen;
                (Buffer.ReadPosition, lastColumnLen) = _columns[lastColumnRead-1];
                if (lastColumnLen != -1)
                    Buffer.ReadPosition += lastColumnLen;
                var len = Buffer.ReadInt32();
                _columns.Add((Buffer.ReadPosition, len));
            }

            (Buffer.ReadPosition, ColumnLen) = _columns[column];
            _column = column;
            PosInColumn = 0;
        }

        internal override Task SeekToColumn(int column, bool async)
        {
            SeekToColumn(column);
            return PGUtil.CompletedTask;
        }

        internal override Task SeekInColumn(int posInColumn, bool async)
        {
            if (posInColumn > ColumnLen)
                posInColumn = ColumnLen;
            Buffer.ReadPosition = _columns[_column].Offset + posInColumn;
            PosInColumn = posInColumn;
            return PGUtil.CompletedTask;
        }

        internal async Task SeekToColumnStart(int column, bool async)
        {
            await SeekToColumnNew(column, async);
            if (PosInColumn != 0)
                await SeekInColumn(0, async);
        }

        internal Task SeekToColumnNew(int column, bool async)
        {
            CheckColumnIndex(column);

            if (_column != column)
            {
                Buffer.Seek(_columns[column].Offset, SeekOrigin.Begin);
                _column = column;
                ColumnLen = Buffer.ReadInt32();
                PosInColumn = 0;

            }

            return PGUtil.CompletedTask;
        }

        internal void CheckColumnIndex(int column)
        {
            if (column < 0 || column >= _columns.Count)
            {
                throw new IndexOutOfRangeException("Column index out of range");
            }
        }
    }
}
