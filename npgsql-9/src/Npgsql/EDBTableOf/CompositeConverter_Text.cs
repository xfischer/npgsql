using EnterpriseDB.EDBClient.Internal.Composites;
using System;

namespace EnterpriseDB.EDBClient.Internal.Converters;

// EnterpriseDB: partial class for ITextFormatConverter impl to allow Text to composite conversion (EC-3164)
sealed partial class CompositeConverter<T> : ITextFormatConverter
{
    public object ReadFromValues(params object[] values)
    {
        using var builder = new CompositeBuilder<T>(_composite);

        var count = values.Length;
        if (count != _composite.Fields.Count)
            throw new InvalidOperationException("Cannot read composite type with mismatched number of fields.");

        int index = 0;
        foreach (var field in _composite.Fields)
        {
            field.AddValue(builder, values[index++]);
        }

        return builder.Complete();
    }
}
