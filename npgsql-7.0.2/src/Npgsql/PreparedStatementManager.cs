//#define EDB_DIAGNOSTICS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using EnterpriseDB.EDBClient.Internal;
using System.Linq;

namespace EnterpriseDB.EDBClient;

sealed class PreparedStatementManager
{
    internal int MaxAutoPrepared { get; }
    internal int UsagesBeforePrepare { get; }

    private HashSet<DateTime> _assignedDates = new();
    internal Dictionary<string, PreparedStatement> BySql { get; } = new();
    internal PreparedStatement?[] AutoPrepared { get; }

    readonly PreparedStatement?[] _candidates;

    /// <summary>
    /// Total number of current prepared statements (whether explicit or automatic).
    /// </summary>
    internal int NumPrepared;

    readonly EDBConnector _connector;

    internal string NextPreparedStatementName() => "_p" + (++_preparedStatementIndex);
    ulong _preparedStatementIndex;

    internal readonly ILogger _commandLogger;

    internal const int CandidateCount = 100;

    internal PreparedStatementManager(EDBConnector connector)
    {
        _connector = connector;
        _commandLogger = connector.LoggingConfiguration.CommandLogger;

        MaxAutoPrepared = connector.Settings.MaxAutoPrepare;
        UsagesBeforePrepare = connector.Settings.AutoPrepareMinUsages;

        if (MaxAutoPrepared > 0)
        {
            if (MaxAutoPrepared > 256)
                _commandLogger.LogWarning($"{nameof(MaxAutoPrepared)} is over 256, performance degradation may occur. Please report via an issue.");
            AutoPrepared = new PreparedStatement[MaxAutoPrepared];
            _candidates = new PreparedStatement[CandidateCount];
        }
        else
        {
            AutoPrepared = null!;
            _candidates = null!;
        }
    }

    internal PreparedStatement? GetOrAddExplicit(EDBBatchCommand batchCommand)
    {
        var sql = batchCommand.FinalCommandText!;

        PreparedStatement? statementBeingReplaced = null;
        if (BySql.TryGetValue(sql, out var pStatement))
        {
            Debug.Assert(pStatement.State != PreparedState.Unprepared);
            if (pStatement.IsExplicit)
            {
                // Great, we've found an explicit prepared statement.
                // We just need to check that the parameter types correspond, since prepared statements are
                // only keyed by SQL (to prevent pointless allocations). If we have a mismatch, simply run unprepared.
                return pStatement.DoParametersMatch(batchCommand.PositionalParameters!)
                    ? pStatement
                    : null;
            }

            // We've found an autoprepare statement (candidate or otherwise)
            switch (pStatement.State)
            {
            case PreparedState.NotPrepared:
                // Found a candidate for autopreparation. Remove it and prepare explicitly.
                RemoveCandidate(pStatement);
                break;
            case PreparedState.Prepared:
                // The statement has already been autoprepared. We need to "promote" it to explicit.
                statementBeingReplaced = pStatement;
                break;
            case PreparedState.Unprepared:
                throw new InvalidOperationException($"Found unprepared statement in {nameof(PreparedStatementManager)}");
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        // Statement hasn't been prepared yet
        return BySql[sql] = PreparedStatement.CreateExplicit(this, sql, NextPreparedStatementName(), batchCommand.PositionalParameters, statementBeingReplaced);
    }

    internal PreparedStatement? TryGetAutoPrepared(EDBBatchCommand batchCommand)
    {
        var sql = batchCommand.FinalCommandText!;
        if (!BySql.TryGetValue(sql, out var pStatement))
        {
            // New candidate. Find an empty candidate slot or eject a least-used one.
            int slotIndex = -1, leastUsages = int.MaxValue;
            var lastUsed = DateTime.MaxValue;
            for (var i = 0; i < _candidates.Length; i++)
            {
                var candidate = _candidates[i];
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // ReSharper disable HeuristicUnreachableCode
                if (candidate == null)  // Found an unused candidate slot, return immediately
                {
                    slotIndex = i;
                    break;
                }
                // ReSharper restore HeuristicUnreachableCode
                if (candidate.Usages < leastUsages)
                {
                    leastUsages = candidate.Usages;
                    slotIndex = i;
                    lastUsed = candidate.LastUsed;
                }
                else if (candidate.Usages == leastUsages && candidate.LastUsed < lastUsed)
                {
                    slotIndex = i;
                    lastUsed = candidate.LastUsed;
                }
            }

            var leastUsed = _candidates[slotIndex];
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (leastUsed != null)
                BySql.Remove(leastUsed.Sql);
            pStatement = BySql[sql] = _candidates[slotIndex] = PreparedStatement.CreateAutoPrepareCandidate(this, sql);
        }

        switch (pStatement.State)
        {
        case PreparedState.NotPrepared:
        case PreparedState.Invalidated:
            break;

        case PreparedState.Prepared:
        case PreparedState.BeingPrepared:
            // The statement has already been prepared (explicitly or automatically), or has been selected
            // for preparation (earlier identical statement in the same command).
            // We just need to check that the parameter types correspond, since prepared statements are
            // only keyed by SQL (to prevent pointless allocations). If we have a mismatch, simply run unprepared.
            if (!pStatement.DoParametersMatch(batchCommand.PositionalParameters))
                return null;
            // Prevent this statement from being replaced within this batch
            SetLastUsed(pStatement, DateTime.MaxValue);
            return pStatement;

        case PreparedState.BeingUnprepared:
            // The statement is being replaced by an earlier statement in this same batch.
            return null;

        default:
            Debug.Fail($"Unexpected {nameof(PreparedState)} in auto-preparation: {pStatement.State}");
            break;
        }

        if (++pStatement.Usages < UsagesBeforePrepare)
        {
            // Statement still hasn't passed the usage threshold, no automatic preparation.
            // Return null for unprepared execution.
            SetLastUsed(pStatement, DateTime.UtcNow);
            return null;
        }

        // Bingo, we've just passed the usage threshold, statement should get prepared
        LogMessages.AutoPreparingStatement(_commandLogger, sql, _connector.Id);

        // Look for either an empty autoprepare slot, or the least recently used prepared statement which we'll replace it.
        var oldestTimestamp = DateTime.MaxValue;
        var selectedIndex = -1;
        for (var i = 0; i < AutoPrepared.Length; i++)
        {
            var slot = AutoPrepared[i];

            if (slot is null or { State: PreparedState.Invalidated })
            {
                // We found a free or invalidated slot, exit the loop immediately
                selectedIndex = i;
                break;
            }

            switch (slot.State)
            {
            case PreparedState.Prepared:
                if (slot.LastUsed < oldestTimestamp)
                {
                    selectedIndex = i;
                    oldestTimestamp = slot.LastUsed;
                }
                break;

            case PreparedState.BeingPrepared:
                // Slot has already been selected for preparation by an earlier statement in this batch. Skip it.
                continue;

            default:
                throw new Exception(
                    $"Invalid {nameof(PreparedState)} state {slot.State} encountered when scanning prepared statement slots");
            }
        }

        if (selectedIndex == -1)
        {
            // We're here if we couldn't find a free slot or a prepared statement to replace - this means all slots are taken by
            // statements being prepared in this batch.
            return null;
        }

        if (pStatement.State != PreparedState.Invalidated)
            RemoveCandidate(pStatement);

#if EDB_DIAGNOSTICS
        LogMessages.CustomMessageEDB(_commandLogger, $"oldPreparedStatement: AutoPrepared: [{string.Join(",", AutoPrepared.Where(s => s != null).Select(s => string.Concat(s?.Sql, "/", s?.State, "/", s?.LastUsed.Ticks)))}] (index: {selectedIndex})");
#endif

        var oldPreparedStatement = AutoPrepared[selectedIndex];

        if (oldPreparedStatement is null)
        {
            pStatement.Name = "_auto" + selectedIndex;
        }
        else
        {
            // When executing an invalidated prepared statement, the old and the new statements are the same instance.
            // Create a copy so that we have two distinct instances with their own states.
            if (oldPreparedStatement == pStatement)
            {
                oldPreparedStatement = new PreparedStatement(this, oldPreparedStatement.Sql, isExplicit: false)
                {
                    Name = oldPreparedStatement.Name
                };
            }

            pStatement.Name = oldPreparedStatement.Name;
            pStatement.State = PreparedState.NotPrepared;
            pStatement.StatementBeingReplaced = oldPreparedStatement;
            oldPreparedStatement.State = PreparedState.BeingUnprepared;
        }

        pStatement.AutoPreparedSlotIndex = selectedIndex;
        AutoPrepared[selectedIndex] = pStatement;


        // Make sure this statement isn't replaced by a later statement in the same batch.
        SetLastUsed(pStatement, DateTime.MaxValue);

        // Note that the parameter types are only set at the moment of preparation - in the candidate phase
        // there's no differentiation between overloaded statements, which are a pretty rare case, saving
        // allocations.
        pStatement.SetParamTypes(batchCommand.PositionalParameters);

        return pStatement;
    }

    void RemoveCandidate(PreparedStatement candidate)
    {
#if EDB_DIAGNOSTICS
        LogMessages.CustomMessageEDB(_commandLogger, $"BEGIN Remove candidate {candidate}");
#endif
        var i = 0;
        for (; i < _candidates.Length; i++)
        {
            if (_candidates[i] == candidate)
            {
                _candidates[i] = null;
                return;
            }
        }
        Debug.Assert(i < _candidates.Length);
    }

    internal void ClearAll()
    {
        _assignedDates.Clear();
        BySql.Clear();
        NumPrepared = 0;
        _preparedStatementIndex = 0;
        if (AutoPrepared is not null)
            for (var i = 0; i < AutoPrepared.Length; i++)
                AutoPrepared[i] = null;
        if (_candidates != null)
            for (var i = 0; i < _candidates.Length; i++)
                _candidates[i] = null;
    }

    // EnterpriseDB: prevent DateTime.UtcNow collision bug in .net Framework
    // due to > 1 ms precision error
    internal void SetLastUsed(PreparedStatement pStatement, DateTime dateTime)
    {
        if (pStatement.LastUsed == dateTime)
            return;

#if NETFRAMEWORK
        _assignedDates.Remove(pStatement.LastUsed);
        while (_assignedDates.Contains(dateTime))
        {
            dateTime = new DateTime(dateTime.Ticks + 1);
        }        
        _assignedDates.Add(dateTime);
#endif

        pStatement.LastUsed = dateTime;
    }
}
