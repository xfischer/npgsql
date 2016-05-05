#pragma warning disable
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient;
using EnterpriseDB.EDBClient.TypeHandlers;
using System.Threading;
#pragma warning disable
using EnterpriseDB.EDBClient.FrontendMessages;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Net.Sockets;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Logging;
using System.Text.RegularExpressions;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using EDBTypes;
using System.Collections;
using System.Reflection;
using EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers;
using System.Diagnostics.CodeAnalysis;

namespace EnterpriseDB.EDBClient.BackendMessages
{
    partial class DataRowSequentialMessage
    {
        internal async override Task SeekToColumnAsync(int column)
        {
            CheckColumnIndex(column);
            if (column < Column)
            {
                throw new InvalidOperationException(string.Format("Invalid attempt to read from column ordinal '{0}'. With CommandBehavior.SequentialAccess, you may only read from column ordinal '{1}' or greater.", column, Column));
            }

            if (column == Column)
            {
                return;
            }

            // Skip to end of column if needed
            var remainingInColumn = (ColumnLen == -1 ? 0 : ColumnLen - PosInColumn);
            if (remainingInColumn > 0)
            {
                await Buffer.SkipAsync(remainingInColumn);
            }

            // Shut down any streaming going on on the colun
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            // Skip over unwanted fields
            for (; Column < column - 1; Column++)
            {
                await Buffer.EnsureAsync(4);
                var len = Buffer.ReadInt32();
                if (len != -1)
                {
                    await Buffer.SkipAsync(len);
                }
            }

            await Buffer.EnsureAsync(4);
            ColumnLen = Buffer.ReadInt32();
            PosInColumn = 0;
            Column = column;
        }

        internal async override Task ConsumeAsync()
        {
            // Skip to end of column if needed
            var remainingInColumn = (ColumnLen == -1 ? 0 : ColumnLen - PosInColumn);
            if (remainingInColumn > 0)
            {
                await Buffer.SkipAsync(remainingInColumn);
            }

            // Skip over the remaining columns in the row
            for (; Column < NumColumns - 1; Column++)
            {
                await Buffer.EnsureAsync(4);
                var len = Buffer.ReadInt32();
                if (len != -1)
                {
                    await Buffer.SkipAsync(len);
                }
            }
        }
    }
}

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Large object manager. This class can be used to store very large files in a PostgreSQL database.
    /// </summary>
    public partial class EDBLargeObjectManager
    {
        internal async Task<T> ExecuteFunctionAsync<T>(string function, params object[] arguments)
        {
            using (var command = new EDBCommand(function, _connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = function;
                foreach (var argument in arguments)
                {
                    command.Parameters.Add(new EDBParameter()
                    {Value = argument});
                }

                return (T)(await command.ExecuteScalarAsync());
            }
        }

        internal async Task<int> ExecuteFunctionGetBytesAsync(string function, byte[] buffer, int offset, int len, params object[] arguments)
        {
            using (var command = new EDBCommand(function, _connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                foreach (var argument in arguments)
                {
                    command.Parameters.Add(new EDBParameter()
                    {Value = argument});
                }

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    await reader.ReadAsync();
                    return (int)reader.GetBytes(0, 0, buffer, offset, len);
                }
            }
        }

        public async Task<uint> CreateAsync(uint preferredOid = 0)
        {
            return await ExecuteFunctionAsync<uint>("lo_create", (int)preferredOid);
        }

        public async Task<EDBLargeObjectStream> OpenReadAsync(uint oid)
        {
            var fd = (await ExecuteFunctionAsync<int>("lo_open", (int)oid, INV_READ));
            return new EDBLargeObjectStream(this, oid, fd, false);
        }

        public async Task<EDBLargeObjectStream> OpenReadWriteAsync(uint oid)
        {
            var fd = (await ExecuteFunctionAsync<int>("lo_open", (int)oid, INV_READ | INV_WRITE));
            return new EDBLargeObjectStream(this, oid, fd, true);
        }

        public async Task UnlinkAsync(uint oid)
        {
            await ExecuteFunctionAsync<object>("lo_unlink", (int)oid);
        }

        public async Task ExportRemoteAsync(uint oid, string path)
        {
            await ExecuteFunctionAsync<object>("lo_export", (int)oid, path);
        }

        public async Task ImportRemoteAsync(string path, uint oid = 0)
        {
            await ExecuteFunctionAsync<object>("lo_import", path, (int)oid);
        }
    }

    /// <summary>
    /// An interface to remotely control the seekable stream for an opened large object on a PostgreSQL server.
    /// Note that the OpenRead/OpenReadWrite method as well as all operations performed on this stream must be wrapped inside a database transaction.
    /// </summary>
    public partial class EDBLargeObjectStream
    {
        public async Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - offset < count)
                throw new ArgumentException("Invalid offset or count for this buffer");
            Contract.EndContractBlock();
            CheckDisposed();
            int chunkCount = Math.Min(count, _manager.MaxTransferBlockSize);
            int read = 0;
            while (read < count)
            {
                var bytesRead = (await _manager.ExecuteFunctionGetBytesAsync("loread", buffer, offset + read, count - read, _fd, chunkCount));
                _pos += bytesRead;
                read += bytesRead;
                if (bytesRead < chunkCount)
                {
                    return read;
                }
            }

            return read;
        }

        public async Task WriteAsync(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - offset < count)
                throw new ArgumentException("Invalid offset or count for this buffer");
            Contract.EndContractBlock();
            CheckDisposed();
            if (!_writeable)
                throw new NotSupportedException("Write cannot be called on a stream opened with no write permissions");
            int totalWritten = 0;
            while (totalWritten < count)
            {
                var chunkSize = Math.Min(count - totalWritten, _manager.MaxTransferBlockSize);
                var bytesWritten = (await _manager.ExecuteFunctionAsync<int>("lowrite", _fd, new ArraySegment<byte>(buffer, offset + totalWritten, chunkSize)));
                totalWritten += bytesWritten;
                if (bytesWritten != chunkSize)
                    throw PGUtil.ThrowIfReached();
                _pos += bytesWritten;
            }
        }

        async Task<long> GetLengthInternalAsync()
        {
            CheckDisposed();
            long old = _pos;
            long retval = (await SeekAsync(0, SeekOrigin.End));
            if (retval != old)
                await SeekAsync(old, SeekOrigin.Begin);
            return retval;
        }

        public async Task<long> SeekAsync(long offset, SeekOrigin origin)
        {
            if (origin < SeekOrigin.Begin || origin > SeekOrigin.End)
                throw new ArgumentException("Invalid origin");
            if (!Has64BitSupport && offset != (long)(int)offset)
                throw new ArgumentOutOfRangeException("offset", "offset must fit in 32 bits for PostgreSQL versions older than 9.3");
            Contract.EndContractBlock();
            CheckDisposed();
            if (_manager.Has64BitSupport)
                return _pos = (await _manager.ExecuteFunctionAsync<long>("lo_lseek64", _fd, offset, (int)origin));
            else
                return _pos = (await _manager.ExecuteFunctionAsync<int>("lo_lseek", _fd, (int)offset, (int)origin));
        }

        public async Task FlushAsync()
        {
        }

        public async Task SetLengthAsync(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("value");
            if (!Has64BitSupport && value != (long)(int)value)
                throw new ArgumentOutOfRangeException("value", "offset must fit in 32 bits for PostgreSQL versions older than 9.3");
            Contract.EndContractBlock();
            CheckDisposed();
            if (!_writeable)
                throw new NotSupportedException("SetLength cannot be called on a stream opened with no write permissions");
            if (_manager.Has64BitSupport)
                await _manager.ExecuteFunctionAsync<int>("lo_truncate64", _fd, value);
            else
                await _manager.ExecuteFunctionAsync<int>("lo_truncate", _fd, (int)value);
        }
    }

    internal partial class EDBBuffer
    {
        internal async Task EnsureAsync(int count)
        {
            Contract.Requires(count <= Size);
            count -= ReadBytesLeft;
            if (count <= 0)
            {
                return;
            }

            if (ReadPosition == _filledBytes)
            {
                Clear();
            }
            else if (count > Size - _filledBytes)
            {
                Array.Copy(_buf, ReadPosition, _buf, 0, ReadBytesLeft);
                _filledBytes = ReadBytesLeft;
                ReadPosition = 0;
            }

            while (count > 0)
            {
                var toRead = Size - _filledBytes;
                var read = (await Underlying.ReadAsync(_buf, _filledBytes, toRead));
                if (read == 0)
                {
                    throw new EndOfStreamException();
                }

                count -= read;
                _filledBytes += read;
            }
        }

        internal async Task ReadMoreAsync()
        {
            await EnsureAsync(ReadBytesLeft + 1);
        }

        internal async Task<EDBBuffer> EnsureOrAllocateTempAsync(int count)
        {
            if (count <= Size)
            {
                await EnsureAsync(count);
                return this;
            }

            // Worst case: our buffer isn't big enough. For now, allocate a new buffer
            // and copy into it
            // TODO: Optimize with a pool later?
            var tempBuf = new EDBBuffer(Underlying, count, TextEncoding);
            CopyTo(tempBuf);
            Clear();
            await tempBuf.EnsureAsync(count);
            return tempBuf;
        }

        internal async Task SkipAsync(long len)
        {
            Contract.Requires(len >= 0);
            if (len > ReadBytesLeft)
            {
                len -= ReadBytesLeft;
                while (len > Size)
                {
                    Clear();
                    await EnsureAsync(Size);
                    len -= Size;
                }

                Clear();
                await EnsureAsync((int)len);
            }

            ReadPosition += (int)len;
        }

        public async Task FlushAsync()
        {
            if (_writePosition != 0)
            {
                Contract.Assert(ReadBytesLeft == 0, "There cannot be read bytes buffered while a write operation is going on.");
                await Underlying.WriteAsync(_buf, 0, _writePosition);
                await Underlying.FlushAsync();
                TotalBytesFlushed += _writePosition;
                _writePosition = 0;
            }
        }

        internal async Task<int> ReadAllBytesAsync(byte[] output, int outputOffset, int len, bool readOnce)
        {
            if (len <= ReadBytesLeft)
            {
                Array.Copy(_buf, ReadPosition, output, outputOffset, len);
                ReadPosition += len;
                return len;
            }

            Array.Copy(_buf, ReadPosition, output, outputOffset, ReadBytesLeft);
            var offset = outputOffset + ReadBytesLeft;
            var totalRead = ReadBytesLeft;
            Clear();
            while (totalRead < len)
            {
                var read = (await Underlying.ReadAsync(output, offset, len - totalRead));
                if (read == 0)
                {
                    throw new EndOfStreamException();
                }

                totalRead += read;
                if (readOnce)
                {
                    return totalRead;
                }

                offset += read;
            }

            return len;
        }
    }

    public sealed partial class EDBCommand
    {
        internal async Task<EDBDataReader> ExecuteAsync(CommandBehavior behavior = CommandBehavior.Default)
        {
            State = CommandState.InProgress;
            try
            {
                _queryIndex = 0;
                await _connector.SendAllMessagesAsync();
                if (!IsPrepared)
                {
                    IBackendMessage msg;
                    do
                    {
                        msg = (await _connector.ReadSingleMessageAsync());
                    }
                    while (!ProcessMessageForUnprepared(msg, behavior));
                }

                var reader = new EDBDataReader(this, behavior, _queries);
                await reader.InitAsync();
                /*    
                if (_parameters != null)
                    if (_parameters.Count  != 0)
                    {
                        while (reader.Read()) ;
                        reader.Read();
                        reader.Close();

                        for (int i = 0; i < _parameters.Count; i++)
                        {


                            Console.WriteLine(_parameters[0].Value.ToString());

                          

                            string p = "fetch all in \"" + _parameters[i].Value + "\"";
                            EDBCommand command1 = new EDBCommand(p.ToString(), _connector.Connection);
                            EDBDataReader rd2 = command1.ExecuteReader(CommandBehavior.SingleResult);
                            int fieldcont = rd2.FieldCount;
                            Console.WriteLine(rd2.IsCaching.ToString());
                            
                            _parameters[i].Value = (EDBDataReader)rd2;
                          //  while (rd2.Read()) ;
                            //rd2.Close();
                            //EDBCommand cmd = new EDBCommand("select 1",_connector.Connection);
                            //EDBDataReader reader11 = cmd.ExecuteReader(CommandBehavior.SingleResult);
                            //reader11.Init();
                        }
                   
                }*/
                //   EDBDataReader = reader = new EDBDataReader("select 1 from dual", CommandBehavior.SingleResult, _queries[1]);
                _connector.CurrentReader = reader;
                return reader;
            }
            catch
            {
                State = CommandState.Idle;
                throw;
            }
        }

        async Task<int> ExecuteNonQueryInternalAsync()
        {
            Prechecks();
            Log.Debug("ExecuteNonQuery", Connection.Connector.Id);
            using (Connection.Connector.StartUserAction())
            {
                ValidateAndCreateMessages();
                EDBDataReader reader;
                using (reader = (await ExecuteAsync()))
                {
                    if (!_isPrepared)
                        while (await reader.NextResultAsync())
                            ;
                }

                return reader.RecordsAffected;
            }
        }

        async Task<object> ExecuteScalarInternalAsync()
        {
            Prechecks();
            Log.Debug("ExecuteNonScalar", Connection.Connector.Id);
            using (Connection.Connector.StartUserAction())
            {
                var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleRow;
                ValidateAndCreateMessages(behavior);
                using (var reader = Execute(behavior))
                {
                    return (await reader.ReadAsync()) && reader.FieldCount != 0 ? reader.GetValue(0) : null;
                }
            }
        }

        async Task<EDBDataReader> ExecuteDbDataReaderInternalAsync(CommandBehavior behavior)
        {
            Prechecks();
            Log.Debug("ExecuteReader", Connection.Connector.Id);
            Connection.Connector.StartUserAction();
            try
            {
                ValidateAndCreateMessages(behavior);
                return await ExecuteAsync(behavior);
            }
            catch
            {
                if (Connection.Connector != null)
                {
                    Connection.Connector.EndUserAction();
                }

                // Close connection if requested even when there is an error.
                if ((behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
                {
                    _connection.Close();
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Represents a connection to a PostgreSQL backend. Unlike EDBConnection objects, which are
    /// exposed to users, connectors are internal to  EnterpriseDB.EDBClient and are recycled by the connection pool.
    /// </summary>
    internal partial class EDBConnector
    {
        internal async Task SendAllMessagesAsync()
        {
            if (!_messagesToSend.Any())
            {
                return;
            }

            // If a cancellation is in progress, wait for it to "complete" before proceeding (#615)
            lock (_cancelLock)
            {
            }

            _sentRfqPrependedMessages = _pendingRfqPrependedMessages;
            _pendingRfqPrependedMessages = 0;
            try
            {
                foreach (var msg in _messagesToSend)
                {
                    await SendMessageAsync(msg);
                }

                await Buffer.FlushAsync();
            }
            catch
            {
                Break();
                throw;
            }
            finally
            {
                _messagesToSend.Clear();
            }
        }

        async Task SendMessageAsync(FrontendMessage msg)
        {
            Log.Trace(String.Format("Sending: {0}", msg), Id);
            var asSimple = msg as SimpleFrontendMessage;
            if (asSimple != null)
            {
                if (asSimple.Length > Buffer.WriteSpaceLeft)
                {
                    await Buffer.FlushAsync();
                }

                Contract.Assume(Buffer.WriteSpaceLeft >= asSimple.Length);
                asSimple.Write(Buffer);
                return;
            }

            var asComplex = msg as ChunkingFrontendMessage;
            if (asComplex != null)
            {
                var directBuf = new DirectBuffer();
                while (!asComplex.Write(Buffer, ref directBuf))
                {
                    await Buffer.FlushAsync();
                    // The following is an optimization hack for writing large byte arrays without passing
                    // through our buffer
                    if (directBuf.Buffer != null)
                    {
                        await Buffer.Underlying.WriteAsync(directBuf.Buffer, directBuf.Offset, directBuf.Size == 0 ? directBuf.Buffer.Length : directBuf.Size);
                        directBuf.Buffer = null;
                        directBuf.Size = 0;
                    }
                }

                return;
            }

            throw PGUtil.ThrowIfReached();
        }

        internal async Task<IBackendMessage> ReadSingleMessageAsync(DataRowLoadingMode dataRowLoadingMode = DataRowLoadingMode.NonSequential, bool returnNullForAsyncMessage = false)
        {
            // First read the responses of any prepended messages.
            // Exceptions shouldn't happen here, we break the connector if they do
            if (_sentRfqPrependedMessages > 0)
            {
                try
                {
                    SetFrontendTimeout(ActualInternalCommandTimeout);
                    while (_sentRfqPrependedMessages > 0)
                    {
                        var msg = (await DoReadSingleMessageAsync(DataRowLoadingMode.Skip));
                        if (msg is ReadyForQueryMessage)
                        {
                            _sentRfqPrependedMessages--;
                        }
                    }
                }
                catch
                {
                    Break();
                    throw;
                }
            }

            // Now read a non-prepended message
            try
            {
                SetFrontendTimeout(UserCommandFrontendTimeout);
                return await DoReadSingleMessageAsync(dataRowLoadingMode, returnNullForAsyncMessage);
            }
            catch (EDBException)
            {
                if (CurrentReader != null)
                {
                    // The reader cleanup will call EndUserAction
                    CurrentReader.Cleanup();
                }
                else
                {
                    EndUserAction();
                }

                throw;
            }
            catch
            {
                Break();
                throw;
            }
        }

        async Task<IBackendMessage> DoReadSingleMessageAsync(DataRowLoadingMode dataRowLoadingMode = DataRowLoadingMode.NonSequential, bool returnNullForAsyncMessage = false)
        {
            Contract.Ensures(returnNullForAsyncMessage || Contract.Result<IBackendMessage>() != null);
            EDBException error = null;
            while (true)
            {
                var buf = Buffer;
                await Buffer.EnsureAsync(5);
                var messageCode = (BackendMessageCode)Buffer.ReadByte();
                Contract.Assume(Enum.IsDefined(typeof (BackendMessageCode), messageCode), "Unknown message code: " + messageCode);
                var len = Buffer.ReadInt32() - 4; // Transmitted length includes itself
                if ((messageCode == BackendMessageCode.DataRow && dataRowLoadingMode != DataRowLoadingMode.NonSequential) || messageCode == BackendMessageCode.CopyData)
                {
                    if (dataRowLoadingMode == DataRowLoadingMode.Skip)
                    {
                        await Buffer.SkipAsync(len);
                        continue;
                    }
                }
                else if (len > Buffer.ReadBytesLeft)
                {
                    buf = (await buf.EnsureOrAllocateTempAsync(len));
                }

                var msg = ParseServerMessage(buf, messageCode, len, dataRowLoadingMode);
                switch (messageCode)
                {
                    case BackendMessageCode.ErrorResponse:
                        //ZK  Contract.Assert(msg == null);
                        // An ErrorResponse is (almost) always followed by a ReadyForQuery. Save the error
                        // and throw it as an exception when the ReadyForQuery is received (next).
                        error = new EDBException(buf);
                        if (State == ConnectorState.Connecting)
                        {
                            // During the startup/authentication phase, an ErrorResponse isn't followed by
                            // an RFQ. Instead, the server closes the connection immediately
                            throw error;
                        }

                        continue;
                    case BackendMessageCode.ReadyForQuery:
                        if (error != null)
                        {
                            throw error;
                        }

                        break;
                    // Asynchronous messages
                    case BackendMessageCode.NoticeResponse:
                    case BackendMessageCode.NotificationResponse:
                    case BackendMessageCode.ParameterStatus:
                        //ZK        Contract.Assert(msg == null);
                        if (!returnNullForAsyncMessage)
                        {
                            continue;
                        }

                        return null;
                }

                Contract.Assert(msg != null, "Message is null for code: " + messageCode);
                return msg;
            }
        }

        internal async Task<IBackendMessage> SkipUntilAsync(BackendMessageCode stopAt)
        {
            Contract.Requires(stopAt != BackendMessageCode.DataRow, "Shouldn't be used for rows, doesn't know about sequential");
            while (true)
            {
                var msg = (await ReadSingleMessageAsync(DataRowLoadingMode.Skip));
                //ZK        Contract.Assert(!(msg is DataRowMessage));
                if (msg.Code == stopAt)
                {
                    return msg;
                }
            }
        }

        internal async Task<IBackendMessage> SkipUntilAsync(BackendMessageCode stopAt1, BackendMessageCode stopAt2)
        {
            Contract.Requires(stopAt1 != BackendMessageCode.DataRow, "Shouldn't be used for rows, doesn't know about sequential");
            Contract.Requires(stopAt2 != BackendMessageCode.DataRow, "Shouldn't be used for rows, doesn't know about sequential");
            while (true)
            {
                var msg = (await ReadSingleMessageAsync(DataRowLoadingMode.Skip));
                //ZK TODO:Do we really need Assestion here?
                //Contract.Assert(!(msg is DataRowMessage));
                if (msg.Code == stopAt1 || msg.Code == stopAt2)
                {
                    return msg;
                }
            }
        }
    }

    /// <summary>
    /// Reads a forward-only stream of rows from a data source.
    /// </summary>
    public partial class EDBDataReader
    {
        internal async Task InitAsync()
        {
            _rowDescription = _statements[0].Description;
            if (_rowDescription == null)
            {
                // The first query has not result set, seek forward to the first query that does (if any)
                if (!(await NextResultAsync()))
                {
                    // No resultsets at all
                    return;
                }
            }

            if (Command.Parameters.Any(p => p.IsOutputDirection))
            {
                PopulateOutputParameters();
            }
        }

        async Task<bool> ReadInternalAsync()
        {
            if (_row != null)
            {
                await _row.ConsumeAsync();
                _row = null;
            }

            switch (State)
            {
                case ReaderState.InResult:
                    break;
                case ReaderState.BetweenResults:
                case ReaderState.Consumed:
                case ReaderState.Closed:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            try
            {
                if ((_behavior & CommandBehavior.SingleRow) != 0 && _readOneRow)
                {
                    await ConsumeAsync();
                    return false;
                }

                while (true)
                {
                    var msg = (await ReadMessageAsync());
                    switch (ProcessMessage(msg))
                    {
                        case ReadResult.RowRead:
                            return true;
                        case ReadResult.RowNotRead:
                            return false;
                        case ReadResult.ReadAgain:
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (EDBException)
            {
                State = ReaderState.Consumed;
                throw;
            }
        }

        async Task<bool> NextResultInternalAsync()
        {
            Contract.Requires(!IsSchemaOnly);
            // Contract.Ensures(Command.CommandType != CommandType.StoredProcedure || Contract.Result<bool>() == false);
            //EDB TODO CheckMe
            /*(if (Command.Parameters[0].EDBDbType == EDBDbType.Refcursor)
           {

               EDBCommand cmd = new EDBCommand("select 1");
               EDBDataReader reader = cmd.ExecuteReader();
           }*/
            try
            {
                // If we're in the middle of a resultset, consume it
                switch (State)
                {
                    case ReaderState.InResult:
                        if (_row != null)
                        {
                            await _row.ConsumeAsync();
                            _row = null;
                        }

                        // TODO: Duplication with SingleResult handling above
                        var completedMsg = (await SkipUntilAsync(BackendMessageCode.CompletedResponse, BackendMessageCode.EmptyQueryResponse));
                        ProcessMessage(completedMsg);
                        break;
                    case ReaderState.BetweenResults:
                        break;
                    case ReaderState.Consumed:
                    case ReaderState.Closed:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                //ZK Contract.Assert(State == ReaderState.BetweenResults);
                _hasRows = null;
#if !DNXCORE50
                _cachedSchemaTable = null;
#endif
                if ((_behavior & CommandBehavior.SingleResult) != 0)
                {
                    if (State == ReaderState.BetweenResults)
                    {
                        await ConsumeAsync();
                    }

                    return false;
                }

                // We are now at the end of the previous result set. Read up to the next result set, if any.
                for (_statementIndex++; _statementIndex < _statements.Count; _statementIndex++)
                {
                    _rowDescription = _statements[_statementIndex].Description;
                    if (_rowDescription != null)
                    {
                        State = ReaderState.InResult;
                        // Found a resultset
                        return true;
                    }

                    // Next query has no resultset, read and process its completion message and move on to the next
                    var completedMsg = (await SkipUntilAsync(BackendMessageCode.CompletedResponse, BackendMessageCode.EmptyQueryResponse));
                    ProcessMessage(completedMsg);
                }

                // There are no more queries, we're done. Read to the RFQ.
                ProcessMessage(SkipUntil(BackendMessageCode.ReadyForQuery));
                _rowDescription = null;
                return false;
            }
            catch (EDBException)
            {
                State = ReaderState.Consumed;
                throw;
            }
        }

        async Task<IBackendMessage> ReadMessageAsync()
        {
            if (_pendingMessage != null)
            {
                var msg = _pendingMessage;
                _pendingMessage = null;
                return msg;
            }

            return await _connector.ReadSingleMessageAsync(IsSequential ? DataRowLoadingMode.Sequential : DataRowLoadingMode.NonSequential);
        }

        async Task<IBackendMessage> SkipUntilAsync(BackendMessageCode stopAt)
        {
            if (_pendingMessage != null)
            {
                if (_pendingMessage.Code == stopAt)
                {
                    var msg = _pendingMessage;
                    _pendingMessage = null;
                    return msg;
                }

                _pendingMessage = null;
            }

            return await _connector.SkipUntilAsync(stopAt);
        }

        async Task<IBackendMessage> SkipUntilAsync(BackendMessageCode stopAt1, BackendMessageCode stopAt2)
        {
            if (_pendingMessage != null)
            {
                if (_pendingMessage.Code == stopAt1 || _pendingMessage.Code == stopAt2)
                {
                    var msg = _pendingMessage;
                    _pendingMessage = null;
                    return msg;
                }

                _pendingMessage = null;
            }

            return await _connector.SkipUntilAsync(stopAt1, stopAt2);
        }

        async Task ConsumeAsync()
        {
            if (IsSchemaOnly)
            {
                State = ReaderState.Consumed;
                return;
            }

            if (_row != null)
            {
                await _row.ConsumeAsync();
                _row = null;
            }

            // Skip over the other result sets, processing only CommandCompleted for RecordsAffected
            while (true)
            {
                var msg = (await SkipUntilAsync(BackendMessageCode.CompletedResponse, BackendMessageCode.ReadyForQuery));
                switch (msg.Code)
                {
                    case BackendMessageCode.CompletedResponse:
                        ProcessMessage(msg);
                        continue;
                    case BackendMessageCode.ReadyForQuery:
                        ProcessMessage(msg);
                        return;
                    default:
                        throw new Exception("Unexpected message of type " + msg.Code);
                }
            }
        }

        async Task<bool> IsDBNullInternalAsync(int ordinal)
        {
            CheckRowAndOrdinal(ordinal);
            Contract.EndContractBlock();
            await Row.SeekToColumnAsync(ordinal);
            return _row.IsColumnNull;
        }

        async Task<T> GetFieldValueInternalAsync<T>(int ordinal)
        {
            CheckRowAndOrdinal(ordinal);
            Contract.EndContractBlock();
            var t = typeof (T);
            if (!t.IsArray)
            {
                if (t == typeof (object))
                {
                    return (T)GetValue(ordinal);
                }

                return await ReadColumnAsync<T>(ordinal);
            }

            // Getting an array
            var fieldDescription = _rowDescription[ordinal];
            var handler = fieldDescription.Handler;
            // If the type handler can simply return the requested array, call it as usual. This is the case
            // of reading a string as char[], a bytea as a byte[]...
            var tHandler = handler as ITypeReader<T>;
            if (tHandler != null)
            {
                return await ReadColumnAsync<T>(ordinal);
            }

            // We need to treat this as an actual array type, these need special treatment because of
            // typing/generics reasons
            var elementType = t.GetElementType();
            var arrayHandler = handler as ArrayHandler;
            if (arrayHandler == null)
            {
                throw new InvalidCastException(String.Format("Can't cast database type {0} to {1}", fieldDescription.Handler.PgName, typeof (T).Name));
            }

            if (arrayHandler.GetElementFieldType(fieldDescription) == elementType)
            {
                return (T)GetValue(ordinal);
            }

            if (arrayHandler.GetElementPsvType(fieldDescription) == elementType)
            {
                return (T)GetProviderSpecificValue(ordinal);
            }

            throw new InvalidCastException(String.Format("Can't cast database type {0} to {1}", handler.PgName, typeof (T).Name));
        }

        async Task<T> ReadColumnWithoutCacheAsync<T>(int ordinal)
        {
            _row.SeekToColumnStart(ordinal);
            var fieldDescription = _rowDescription[ordinal];
            try
            {
                return await fieldDescription.Handler.ReadAsync<T>(_row, Row.ColumnLen, fieldDescription);
            }
            catch (SafeReadException e)
            {
                throw e.InnerException;
            }
            catch
            {
                _connector.Break();
                throw;
            }
        }

        async Task<T> ReadColumnAsync<T>(int ordinal)
        {
            CachedValue<T> cache = null;
            if (IsCaching)
            {
                cache = _rowCache.Get<T>(ordinal);
                if (cache.IsSet)
                {
                    return cache.Value;
                }
            }

            var result = (await ReadColumnWithoutCacheAsync<T>(ordinal));
            if (IsCaching)
            {
                //ZK Contract.Assert(cache != null);
                cache.Value = result;
            }

            return result;
        }
    }

    // ReSharper disable once InconsistentNaming
    internal static partial class PGUtil
    {
        public static async Task<Stream> WriteInt32Async(this Stream stream, Int32 value)
        {
            await stream.WriteAsync(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)), 0, 4);
            return stream;
        }
    }

    internal abstract partial class TypeHandler
    {
        internal async Task<T> ReadAsync<T>(DataRowMessage row, int len, FieldDescription fieldDescription = null)
        {
            Contract.Requires(row.PosInColumn == 0);
            Contract.Ensures(row.PosInColumn == row.ColumnLen);
            T result;
            try
            {
                result = (await ReadAsync<T>(row.Buffer, len, fieldDescription));
            }
            finally
            {
                // Important in case a SafeReadException was thrown, position must still be updated
                row.PosInColumn += row.ColumnLen;
            }

            return result;
        }

        internal async Task<T> ReadAsync<T>(EDBBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            T result;
            var asSimpleReader = this as ISimpleTypeReader<T>;
            if (asSimpleReader != null)
            {
                await buf.EnsureAsync(len);
                result = asSimpleReader.Read(buf, len, fieldDescription);
            }
            else
            {
                var asChunkingReader = this as IChunkingTypeReader<T>;
                if (asChunkingReader == null)
                {
                    if (fieldDescription == null)
                        throw new InvalidCastException("Can't cast database type to " + typeof (T).Name);
                    throw new InvalidCastException(String.Format("Can't cast database type {0} to {1}", fieldDescription.Handler.PgName, typeof (T).Name));
                }

                asChunkingReader.PrepareRead(buf, len, fieldDescription);
                while (!asChunkingReader.Read(out result))
                {
                    await buf.ReadMoreAsync();
                }
            }

            return result;
        }
    }
}