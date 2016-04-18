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
using System.Linq;
using System.Text;
using EDBTypes;
using  EnterpriseDB.EDBClient.BackendMessages;

namespace  EnterpriseDB.EDBClient.TypeHandlers.FullTextSearchHandlers
{
    /// <summary>
    /// http://www.postgresql.org/docs/current/static/datatype-textsearch.html
    /// </summary>
    [TypeMapping("tsvector", EDBDbType.TsVector, typeof(EDBTsVector))]
    internal class TsVectorHandler : TypeHandler<EDBTsVector>, IChunkingTypeReader<EDBTsVector>, IChunkingTypeWriter
    {
        // 2561 = 2046 (max length lexeme string) + (1) null terminator +
        // 2 (num_pos) + sizeof(int16) * 256 (max_num_pos (positions/wegihts))
        const int MaxSingleLexemeBytes = 2561;

        EDBBuffer _buf;
        List<EDBTsVector.Lexeme> _lexemes;
        int _numLexemes;
        int _lexemePos;
        int _bytesLeft;
        EDBTsVector _value;

        public void PrepareRead(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            _buf = buf;
            _lexemes = new List<EDBTsVector.Lexeme>();
            _numLexemes = -1;
            _lexemePos = 0;
            _bytesLeft = len;
        }

        public bool Read(out EDBTsVector result)
        {
            result = null;

            if (_numLexemes == -1)
            {
                if (_buf.ReadBytesLeft < 4)
                    return false;
                _numLexemes = _buf.ReadInt32();
                _bytesLeft -= 4;
            }

            for (; _lexemePos < _numLexemes; _lexemePos++)
            {
                if (_buf.ReadBytesLeft < Math.Min(_bytesLeft, MaxSingleLexemeBytes))
                    return false;
                int posBefore = _buf.ReadPosition;

                List<EDBTsVector.Lexeme.WordEntryPos> positions = null;

                var lexemeString = _buf.ReadNullTerminatedString();
                int numPositions = _buf.ReadInt16();
                for (var i = 0; i < numPositions; i++)
                {
                    var _wordEntryPos = _buf.ReadInt16();
                    if (positions == null)
                        positions = new List<EDBTsVector.Lexeme.WordEntryPos>();
                    positions.Add(new EDBTsVector.Lexeme.WordEntryPos(_wordEntryPos));
                }

                _lexemes.Add(new EDBTsVector.Lexeme(lexemeString, positions, true));

                _bytesLeft -= _buf.ReadPosition - posBefore;
            }

            result = new EDBTsVector(_lexemes, true);
            _lexemes = null;
            _buf = null;
            return true;
        }

        public int ValidateAndGetLength(object value, ref LengthCache lengthCache, EDBParameter parameter=null)
        {
            // TODO: Implement length cache
            var vec = value as EDBTsVector;
            if (vec == null) {
                throw CreateConversionException(value.GetType());
            }

            return 4 + vec.Sum(l => Encoding.UTF8.GetByteCount(l.Text) + 1 + 2 + l.Count * 2);
        }

        public void PrepareWrite(object value, EDBBuffer buf, LengthCache lengthCache, EDBParameter parameter=null)
        {
            _lexemePos = -1;
            _buf = buf;
            _value = (EDBTsVector)value;
        }

        public bool Write(ref DirectBuffer directBuf)
        {
            if (_lexemePos == -1)
            {
                if (_buf.WriteSpaceLeft < 4)
                    return false;
                _buf.WriteInt32(_value.Count);
                _lexemePos = 0;
            }

            for (; _lexemePos < _value.Count; _lexemePos++)
            {
                if (_buf.WriteSpaceLeft < MaxSingleLexemeBytes)
                    return false;

                _buf.WriteString(_value[_lexemePos].Text);
                _buf.WriteByte(0);
                _buf.WriteInt16(_value[_lexemePos].Count);
                for (var i = 0; i < _value[_lexemePos].Count; i++)
                {
                    _buf.WriteInt16(_value[_lexemePos][i]._val);
                }
            }

            return true;
        }
    }
}
