using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient;

/// <summary>
/// EDBDataReader specialized for SPL procedures and functions with OUT parameters or return values
/// </summary>
/// <remarks>Overrides the default data reader to avoid subsequent socket reads. Inherits from EDBDataReader to avoid breaking changes</remarks>
public sealed class EDBInMemoryDataReader : EDBDataReader
{
    private enum FetchState
    {
        NotRead = 0,
        Reading = 1,
        DoneReading = 2
    }
    private readonly EDBCommand sourceCommand;
    private readonly EDBDataReader nestedReader;
    private readonly List<EDBParameter> parameters;
    private readonly RowDescriptionMessage rowDescription;
    private readonly Dictionary<string, EDBParameter> paramsByName;
    private FetchState state = FetchState.NotRead;

    internal EDBInMemoryDataReader(EDBCommand command, EDBDataReader reader) : base(reader.Connector)
    {
        sourceCommand = command;
        nestedReader = reader;
        rowDescription = reader.RowDescription?.Clone();
        
        paramsByName = new();

        parameters = BuildParameters(sourceCommand.Parameters.InternalList, rowDescription);
    }

    // Any attempt to read must return a virtual row containing INOUT, OUT parameters and return value
    // This method discards IN parameters and updates RowDescription field names
    private List<EDBParameter> BuildParameters(List<EDBParameter> sourceParameters, RowDescriptionMessage sourceRowDescription)
    {
        var filteredParameters = new List<EDBParameter>(sourceParameters.Count);

        int i = 0;
        foreach (var param in sourceParameters)
        {
            if (!param.IsOutReturnDirection) continue;

            paramsByName[param.ParameterName] = param;
            filteredParameters.Add(param);
            sourceRowDescription[i].Name = param.ParameterName;

            i++;
        }

        return filteredParameters;
    }

    /// <inheritdoc/>
    public override bool Read()
    {
        switch (state)
        {
        case FetchState.NotRead:
            state = FetchState.Reading;
            return true;
        case FetchState.Reading:
            state = FetchState.DoneReading;
            return false;
        default:
            return false;
        }
    }

    /// <summary>
    /// Checks that we have a RowDescription, but not necessary an actual resultset
    /// (for operations which work in SchemaOnly mode.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    FieldDescription GetField(int column)
    {
        if (rowDescription == null)
            throw new InvalidOperationException("No resultset is currently being traversed");

        if (column < 0 || column >= rowDescription.Count)
            throw new IndexOutOfRangeException($"Column must be between {0} and {rowDescription.Count - 1}");

        return rowDescription[column];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    FieldDescription CheckRowAndGetField(int column)
    {
        switch (state)
        {
        case FetchState.Reading:
            break;
        case FetchState.DoneReading:
            throw new InvalidOperationException("The reader is closed");
        case FetchState.NotRead:
            throw new InvalidOperationException("No row is available");
        }

        if (column < 0 || column >= rowDescription!.Count)
            throw new IndexOutOfRangeException($"Column must be between {0} and {rowDescription!.Count - 1}");

        return rowDescription[column];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void CheckClosedOrDisposed()
    {
        switch (state)
        {
        case FetchState.NotRead:
        case FetchState.Reading:
            break;
        case FetchState.DoneReading:
            throw new InvalidOperationException("The reader is closed");
        }
    }

    /// <inheritdoc/>
    public override Task<bool> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(Read());

    /// <inheritdoc/>
    public override int FieldCount => parameters.Count;

    /// <inheritdoc/>
    public override string GetName(int ordinal) => GetField(ordinal).Name;

    /// <inheritdoc/>
    public override object this[int ordinal] => GetValue(ordinal);

    /// <inheritdoc/>
    public override object this[string name] => GetValue(GetOrdinal(name));

    /// <inheritdoc/>
    public override Type GetFieldType(int ordinal) => GetField(ordinal).FieldType;

    /// <inheritdoc/>
    public override T GetFieldValue<T>(int ordinal) => (T)GetValue(ordinal);
    
    /// <inheritdoc/>
    public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken) => Task.FromResult<T>(GetFieldValue<T>(ordinal));

    /// <inheritdoc/>
    public override object GetValue(int ordinal)
    {
        var fieldDescription = CheckRowAndGetField(ordinal);
        return paramsByName[fieldDescription!.Name]!.Value;
    } 

    /// <inheritdoc/>
    public override object GetEDBValue(int ordinal) => GetValue(ordinal);

    /// <inheritdoc/>
    public override Type GetProviderSpecificFieldType(int ordinal)
    {
        var fieldDescription = GetField(ordinal);
        return fieldDescription.Handler.GetProviderSpecificFieldType(fieldDescription);
    }

    /// <inheritdoc/>
    public override object GetProviderSpecificValue(int ordinal) => GetValue(ordinal);

    /// <inheritdoc/>
    public override int GetProviderSpecificValues(object[] values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));
        if (state != FetchState.Reading)
        {
            throw new InvalidOperationException("No row is available");
        }

        var count = Math.Min(FieldCount, values.Length);
        for (var i = 0; i < count; i++)
            values[i] = GetProviderSpecificValue(i);
        return count;
    }
    /// <inheritdoc/>
    public override bool IsDBNull(int ordinal) => GetValue(ordinal) == DBNull.Value;

    /// <inheritdoc/>
    public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken) => Task.FromResult(IsDBNull(ordinal));

    /// <inheritdoc/>
    public override bool IsClosed => nestedReader.IsClosed;

    /// <inheritdoc/>
    public override int GetOrdinal(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("name cannot be empty", nameof(name));
        CheckClosedOrDisposed();
        if (RowDescription is null)
            throw new InvalidOperationException("No resultset is currently being traversed");
        return RowDescription.GetFieldIndex(name);
    }

    /// <inheritdoc/>
    public override PostgresType GetPostgresType(int ordinal) => GetField(ordinal).PostgresType;
    /// <inheritdoc/>
    public override string GetDataTypeName(int ordinal) => GetField(ordinal).TypeDisplayName;

    /// <inheritdoc/>
    public override uint GetDataTypeOID(int ordinal) => GetField(ordinal).TypeOID;

    /// <inheritdoc/>
    public override IEnumerator GetEnumerator() => new DbEnumerator(this);

    /// <inheritdoc/>
    public override ReadOnlyCollection<EDBDbColumn> GetColumnSchema() => nestedReader.GetColumnSchema();

    /// <inheritdoc/>
    public override Task<ReadOnlyCollection<EDBDbColumn>> GetColumnSchemaAsync(CancellationToken cancellationToken = default) => nestedReader.GetColumnSchemaAsync(cancellationToken);


    /// <inheritdoc/>
    public override int GetValues(object[] values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var count = Math.Min(FieldCount, values.Length);
        for (var i = 0; i < count; i++)
            values[i] = GetValue(i);
        return count;
    }

    // Encapsulation overrides pointing to initial reader

    /// <inheritdoc/>
    public override void Close() => nestedReader.Close();

    /// <inheritdoc/>
    public override bool HasRows => state == FetchState.NotRead;

    /// <inheritdoc/>
    public override Task CloseAsync() => nestedReader.CloseAsync();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing) => nestedReader.Dispose();

    /// <inheritdoc/>
    public override ValueTask DisposeAsync() => nestedReader.DisposeAsync();

    /// <inheritdoc/>
    public override bool NextResult() => false;

    /// <inheritdoc/>
    public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => Task.FromResult(false);

}
