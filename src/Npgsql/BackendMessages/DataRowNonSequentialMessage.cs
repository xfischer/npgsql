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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace  EnterpriseDB.EDBClient.BackendMessages
{
    class DataRowNonSequentialMessage : DataRowMessage
    {
        List<int> _columnOffsets;
        int _endOffset;
        /// <summary>
        /// List of all streams that have been opened on this row, and need to be disposed of when the row
        /// is consumed.
        /// </summary>
        List<IDisposable> _streams;

        internal override DataRowMessage Add(DataRowMessage retRow)
        {

            //   ReadBuffer buf_new = new ReadBuffer(Buffer.Connector, Buffer.Connector.Connection. + 100, Buffer.TextEncoding);
            ReadBuffer rd = new ReadBuffer(Buffer.Connector, Buffer.Underlying, 4096, Buffer.TextEncoding);
            WriteBuffer wt = new WriteBuffer(Buffer.Connector, Buffer.Underlying, 4096, Buffer.TextEncoding);

            int buf_new_length = Buffer.Size;// retRow.Buffer.Size - sizeof(Int16);

            //TODO     retRow.Buffer.Seek(retRow._InternalreadPosition, SeekOrigin.Begin);

            retRow.Buffer.Seek(retRow._InternalreadPosition, SeekOrigin.Begin);

            // ISSUE 1 - Starting Position = Buffer.Seek(0, SeekOrigin.Begin);

            int RetRowNumColumns = (int)retRow.Buffer.ReadInt16();
            int OutParamNumColumns = (int)_columnOffsets.Count;
            int TotalNumColumns = OutParamNumColumns + RetRowNumColumns;
            NumColumns = TotalNumColumns;

            wt.WriteInt16(TotalNumColumns);

            // Get length of the buffer retRow
            Column = -1;
            ColumnLen = -1;
            PosInColumn = 0;

            // TODO: Recycle message objects rather than recreating for each row
            //_columnOffsets = new List<int>(TotalNumColumns);
            List<int> AllColumnOffsets = new List<int>(TotalNumColumns);

            for (var i = 0; i < OutParamNumColumns; i++)
            {

                AllColumnOffsets.Add(wt.WritePosition);
                Buffer.Seek(_columnOffsets[i], SeekOrigin.Begin);
                var len = Buffer.ReadInt32();
                wt.WriteInt32(len);

                if (len != -1)
                {
                    byte[] output_data = new byte[len + 1];

                    Buffer.ReadBytes(output_data, (int)SeekOrigin.Current, len);
                    wt.WriteBytes(output_data, (int)SeekOrigin.Current, len);
                }
            }

            for (var i = 0; i < RetRowNumColumns; i++)
            {
                AllColumnOffsets.Add(wt.WritePosition);
                retRow.Buffer.Seek(((DataRowNonSequentialMessage)retRow)._columnOffsets[i], SeekOrigin.Begin);
                var len = retRow.Buffer.ReadInt32();
                wt.WriteInt32(len);

                if (len != -1)
                {
                    byte[] output_data = new byte[len + 1];

                    retRow.Buffer.ReadBytes(output_data, (int)SeekOrigin.Current, len);
                    wt.WriteBytes(output_data, (int)SeekOrigin.Current, len);
                }
            }

            _endOffset = wt.WritePosition;
            //TODO   buf_new.SetFilledBytes = _endOffset;

            Buffer._buf = wt._buf;

            _columnOffsets = AllColumnOffsets;

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



            return this;
        }

        internal override DataRowMessage Load(ReadBuffer buf)
        {
            _InternalreadPosition = buf.ReadPosition;
            NumColumns = buf.ReadInt16();
            Buffer = buf;
            Column = -1;
            ColumnLen = -1;
            PosInColumn = 0;
            // TODO: Recycle message objects rather than recreating for each row
            _columnOffsets = new List<int>(NumColumns);
            for (var i = 0; i < NumColumns; i++)
            {
                _columnOffsets.Add(buf.ReadPosition);
                var len = buf.ReadInt32();
              
                if (len != -1)
                {
                    buf.Seek(len, SeekOrigin.Current);
                }
            }
            _endOffset = buf.ReadPosition;
            return this;
        }

        internal override void SeekToColumn(int column)
        {
            CheckColumnIndex(column);

            if (Column != column)
            {
                Buffer.Seek(_columnOffsets[column], SeekOrigin.Begin);
                Column = column;
                ColumnLen = Buffer.ReadInt32();
                PosInColumn = 0;
            }
        }

        internal override Task SeekToColumnAsync(int column, CancellationToken cancellationToken)
        {
            SeekToColumn(column);
            return PGUtil.CompletedTask;
        }

        internal override void SeekInColumn(int posInColumn)
        {
            if (posInColumn > ColumnLen) {
                posInColumn = ColumnLen;
            }

            Buffer.Seek(_columnOffsets[Column] + 4 + posInColumn, SeekOrigin.Begin);
            PosInColumn = posInColumn;
        }

        internal override Stream GetStream()
        {
            Contract.Requires(PosInColumn == 0);
            var s = Buffer.GetMemoryStream(ColumnLen);
            if (_streams == null) {
                _streams = new List<IDisposable>();
            }
            _streams.Add(s);
            return s;
        }

        internal override void Consume()
        {
            Buffer.Seek(_endOffset, SeekOrigin.Begin);
            if (_streams != null)
            {
                foreach (var stream in _streams) {
                    stream.Dispose();
                }
                _streams.Clear();
            }
        }

        internal override Task ConsumeAsync(CancellationToken cancellationToken)
        {
            Consume();
            return PGUtil.CompletedTask;
        }
    }
}
