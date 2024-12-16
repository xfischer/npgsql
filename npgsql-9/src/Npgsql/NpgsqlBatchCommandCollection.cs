using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace EnterpriseDB.EDBClient;

/// <inheritdoc cref="DbBatchCommandCollection"/>
public class EDBBatchCommandCollection : DbBatchCommandCollection, IList<EDBBatchCommand>
{
    readonly List<EDBBatchCommand> _list;

    internal EDBBatchCommandCollection(List<EDBBatchCommand> batchCommands)
        => _list = batchCommands;

    /// <inheritdoc/>
    public override int Count => _list.Count;

    /// <inheritdoc/>
    public override bool IsReadOnly => false;

    IEnumerator<EDBBatchCommand> IEnumerable<EDBBatchCommand>.GetEnumerator() => _list.GetEnumerator();

    /// <inheritdoc/>
    public override IEnumerator<DbBatchCommand> GetEnumerator() => _list.GetEnumerator();

    /// <inheritdoc/>
    public void Add(EDBBatchCommand item) => _list.Add(item);

    /// <inheritdoc/>
    public override void Add(DbBatchCommand item) => Add(Cast(item));

    /// <inheritdoc/>
    public override void Clear() => _list.Clear();

    /// <inheritdoc/>
    public bool Contains(EDBBatchCommand item) => _list.Contains(item);

    /// <inheritdoc/>
    public override bool Contains(DbBatchCommand item) => Contains(Cast(item));

    /// <inheritdoc/>
    public void CopyTo(EDBBatchCommand[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public override void CopyTo(DbBatchCommand[] array, int arrayIndex)
    {
        if (array is EDBBatchCommand[] typedArray)
        {
            CopyTo(typedArray, arrayIndex);
            return;
        }

        throw new InvalidCastException(
            $"{nameof(array)} is not of type {nameof(EDBBatchCommand)} and cannot be used in this batch command collection.");
    }

    /// <inheritdoc/>
    public int IndexOf(EDBBatchCommand item) => _list.IndexOf(item);

    /// <inheritdoc/>
    public override int IndexOf(DbBatchCommand item) => IndexOf(Cast(item));

    /// <inheritdoc/>
    public void Insert(int index, EDBBatchCommand item) => _list.Insert(index, item);

    /// <inheritdoc/>
    public override void Insert(int index, DbBatchCommand item) => Insert(index, Cast(item));

    /// <inheritdoc/>
    public bool Remove(EDBBatchCommand item) => _list.Remove(item);

    /// <inheritdoc/>
    public override bool Remove(DbBatchCommand item) => Remove(Cast(item));

    /// <inheritdoc/>
    public override void RemoveAt(int index) => _list.RemoveAt(index);

    EDBBatchCommand IList<EDBBatchCommand>.this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    /// <inheritdoc/>
    public new EDBBatchCommand this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    /// <inheritdoc/>
    protected override DbBatchCommand GetBatchCommand(int index)
        => _list[index];

    /// <inheritdoc/>
    protected override void SetBatchCommand(int index, DbBatchCommand batchCommand)
        => _list[index] = Cast(batchCommand);

    static EDBBatchCommand Cast(DbBatchCommand? value)
    {
        var castedValue = value as EDBBatchCommand;
        if (castedValue is null)
            ThrowInvalidCastException(value);

        return castedValue;
    }

    [DoesNotReturn]
    static void ThrowInvalidCastException(DbBatchCommand? value) =>
        throw new InvalidCastException(
            $"The value \"{value}\" is not of type \"{nameof(EDBBatchCommand)}\" and cannot be used in this batch command collection.");
}
