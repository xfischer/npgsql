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
using System.IO;
using System.Linq;
using System.Text;
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

     //   public isReturnRow = false;
        internal override DataRowMessage Load(EDBBuffer buf)
        {
            _InternalreadPosition = buf.ReadPosition;
            buf._tempReadposition = buf.ReadPosition;
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

        internal override DataRowMessage Add(DataRowMessage retRow)
        {

            EDBBuffer buf_new = new EDBBuffer(Buffer.Underlying, Buffer.Size + 100, Buffer.TextEncoding);
            int buf_new_length = Buffer.Size;// retRow.Buffer.Size - sizeof(Int16);

            retRow.Buffer.Seek(retRow._InternalreadPosition, SeekOrigin.Begin);

            // ISSUE 1 - Starting Position = Buffer.Seek(0, SeekOrigin.Begin);

            int RetRowNumColumns = (int)retRow.Buffer.ReadInt16();
            int OutParamNumColumns = (int)_columnOffsets.Count;
            int TotalNumColumns =  OutParamNumColumns + RetRowNumColumns;
            NumColumns = TotalNumColumns;

            buf_new.WriteInt16(TotalNumColumns);

            // Get length of the buffer retRow
            Column = -1;
            ColumnLen = -1;
            PosInColumn = 0;

            // TODO: Recycle message objects rather than recreating for each row
            //_columnOffsets = new List<int>(TotalNumColumns);
            List<int> AllColumnOffsets = new List<int>(TotalNumColumns);

            for (var i = 0; i < OutParamNumColumns; i++)
            {
                AllColumnOffsets.Add(buf_new.WritePosition);
                Buffer.Seek(_columnOffsets[i], SeekOrigin.Begin);
                var len = Buffer.ReadInt32();
                buf_new.WriteInt32(len);

                if (len != -1)
                {
                    byte[] output_data = new byte[len + 1];

                    Buffer.ReadBytes(output_data, (int)SeekOrigin.Current, len);
                    buf_new.WriteBytes(output_data, (int)SeekOrigin.Current, len);
                }
            }

            for (var i = 0; i < RetRowNumColumns; i++)
            {
                AllColumnOffsets.Add(buf_new.WritePosition);
                retRow.Buffer.Seek(((DataRowNonSequentialMessage)retRow)._columnOffsets[i], SeekOrigin.Begin);
                var len = retRow.Buffer.ReadInt32();
                buf_new.WriteInt32(len);

                if (len != -1)
                {
                    byte[] output_data = new byte[len + 1];

                    retRow.Buffer.ReadBytes(output_data, (int)SeekOrigin.Current, len);
                    buf_new.WriteBytes(output_data, (int)SeekOrigin.Current, len);
                }
            }

            _endOffset = buf_new.WritePosition;
            buf_new.SetFilledBytes = _endOffset;
            
            Buffer = buf_new;
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

/*
        internal override DataRowMessage Add(DataRowMessage retRow)
        {
            NumColumns += retRow.NumColumns;// uf.ReadInt16();
            Buffer = retRow.Buffer;
            Column = -1;
            ColumnLen = -1;
            PosInColumn = 0;
            // TODO: Recycle message objects rather than recreating for each row
            _columnOffsets = new List<int>(NumColumns );
            for (var i = 0; i < NumColumns; i++)
            {
                _columnOffsets.Add(retRow.Buffer.ReadPosition);
                var len = retRow.Buffer.ReadInt32();
                if (len != -1)
                {
                    retRow.Buffer.Seek(len, SeekOrigin.Current);
                }
            }
            _endOffset = retRow.Buffer.ReadPosition;
            return this;
        }
*/


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

        internal override Task SeekToColumnAsync(int column)
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

        internal override Task ConsumeAsync()
        {
            Consume();
            return PGUtil.CompletedTask;
        }
    }
}
