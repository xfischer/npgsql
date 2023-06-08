using System.Data.Common;
using System.IO;

namespace EnterpriseDB.EDBClient.Internal.TypeHandling;

/// <summary>
/// Implemented by handlers which support <see cref="DbDataReader.GetTextReader"/>, returns a standard
/// TextReader given a binary Stream.
/// </summary>
interface ITextReaderHandler
{
    TextReader GetTextReader(Stream stream, EDBReadBuffer buffer);
}
