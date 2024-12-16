using System;
using System.Text;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

/// <summary>
/// EnterpriseDB: Converts sys.xmltype to text
/// sys.xmltype is actually a record type, so this might change
/// </summary>
sealed class XmlTypeStringTextConverter : StringBasedTextConverter<string>
{
    private readonly uint _plainXmlPgTypeOid;
    private readonly Encoding _encoding;
    public XmlTypeStringTextConverter(PostgresTypes.PostgresType plainXmlPgType, Encoding encoding) : base(encoding)
    {
        _plainXmlPgTypeOid = plainXmlPgType.OID;
        _encoding = encoding;
    }
    protected override ReadOnlyMemory<char> ConvertTo(string value) => value.AsMemory();
    protected override string ConvertFrom(string value) => value;
    public override string Read(PgReader reader)
    {
        if (reader.Current.Format is not DataFormat.Binary)
            return null!;

        if (reader.ShouldBuffer(3 * sizeof(int)))
            reader.Buffer(3 * sizeof(int));

        _ = reader.ReadInt32(); // not sure if it is format or nested field count
        var oid = reader.ReadInt32();
        if (oid != _plainXmlPgTypeOid)
            throw new InvalidCastException($"Unknown wire type id : {oid}. {_plainXmlPgTypeOid} (xml) expected");

        var size = reader.ReadInt32();

        reader.Buffer(size);

        return _encoding.GetString(reader.ReadBytes(reader.CurrentRemaining));

    }
}
