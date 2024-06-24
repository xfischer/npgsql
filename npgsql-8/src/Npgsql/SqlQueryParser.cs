//#define EDB_DIAGNOSTICS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EnterpriseDB.EDBClient;

sealed class SqlQueryParser
{
    static EDBParameterCollection EmptyParameters { get; } = new();

    readonly Dictionary<string, int> _paramIndexMap = new(StringComparer.OrdinalIgnoreCase);
    readonly StringBuilder _rewrittenSql = new();
    readonly bool supportsRedwoodDialect; // EnterpriseDB Team

    // EnterpriseDB Team
    // Added for best parsing perfs
    // When db is in Redwood mode : perfoms SPL detection, but reverts back to plpgsql if detected
    public SqlQueryParser(bool redwoodDialect) => supportsRedwoodDialect = redwoodDialect;

    /// <summary>
    /// <p>
    /// Receives a user SQL query as passed in by the user in <see cref="EDBCommand.CommandText"/> or
    /// <see cref="EDBBatchCommand.CommandText"/>, and rewrites it for PostgreSQL compatibility.
    /// </p>
    /// <p>
    /// This includes doing rewriting named parameter placeholders to positional (@p => $1), and splitting the query
    /// up by semicolons (legacy batching, SELECT 1; SELECT 2).
    /// </p>
    /// </summary>
    /// <param name="command">The user-facing <see cref="EDBCommand"/> being executed.</param>
    /// <param name="standardConformingStrings">Whether PostgreSQL standards-conforming are used.</param>
    /// <param name="deriveParameters">
    /// A bool indicating whether parameters contains a list of preconfigured parameters or an empty list to be filled with derived
    /// parameters.
    /// </param>
    internal void ParseRawQuery(
        EDBCommand? command,
        bool standardConformingStrings = true,
        bool deriveParameters = false)
        => ParseRawQuery(command, batchCommand: null, standardConformingStrings, deriveParameters);

    /// <summary>
    /// <p>
    /// Receives a user SQL query as passed in by the user in <see cref="EDBCommand.CommandText"/> or
    /// <see cref="EDBBatchCommand.CommandText"/>, and rewrites it for PostgreSQL compatibility.
    /// </p>
    /// <p>
    /// This includes doing rewriting named parameter placeholders to positional (@p => $1), and splitting the query
    /// up by semicolons (legacy batching, SELECT 1; SELECT 2).
    /// </p>
    /// </summary>
    /// <param name="batchCommand"> The user-facing <see cref="EDBBatchCommand"/> being executed.</param>
    /// <param name="standardConformingStrings">Whether PostgreSQL standards-conforming are used.</param>
    /// <param name="deriveParameters">
    /// A bool indicating whether parameters contains a list of preconfigured parameters or an empty list to be filled with derived
    /// parameters.
    /// </param>
    internal void ParseRawQuery(
        EDBBatchCommand? batchCommand,
        bool standardConformingStrings = true,
        bool deriveParameters = false)
        => ParseRawQuery(command: null, batchCommand, standardConformingStrings, deriveParameters);

    void ParseRawQuery(
        EDBCommand? command,
        EDBBatchCommand? batchCommand,
        bool standardConformingStrings = true,
        bool deriveParameters = false)
    {
        string sql;
        EDBParameterCollection parameters;
        List<EDBBatchCommand>? batchCommands;

        var statementIndex = 0;
        if (command is null)
        {
            // Batching mode. We're processing only one batch - if we encounter a semicolon (legacy batching), that's an error.
            Debug.Assert(batchCommand is not null);
            sql = batchCommand.CommandText;
            parameters = batchCommand._parameters ?? EmptyParameters;
            batchCommands = null;
        }
        else
        {
            // Command mode. Semicolons (legacy batching) may occur.
            Debug.Assert(batchCommand is null);
            sql = command.CommandText;
            parameters = command._parameters ?? EmptyParameters;
            batchCommands = command.InternalBatchCommands;
            if (command?.InternalConnection?.Connector?.CommandLogger != null) // EnterpriseDB Team
            {
                LogMessages.TryEDBTrace(command?.InternalConnection?.Connector!.CommandLogger!, $"ParseRawQuery. Command has {batchCommands.Count} item(s)");
            }
            MoveToNextBatchCommand();
        }

        Debug.Assert(batchCommand is not null);
        Debug.Assert(parameters.PlaceholderType != PlaceholderType.Positional);
        Debug.Assert(deriveParameters == false || parameters.Count == 0);
        // Debug.Assert(batchCommand.PositionalParameters is not null && batchCommand.PositionalParameters.Count == 0);

        _paramIndexMap.Clear();
        _rewrittenSql.Clear();

        //char[] trim = new char[] { ' ', '\r', '\n' };
        sql = sql.Trim(); //EnterpriseDB Team
        var currCharOfs = 0;
        var isProcedure = false;//EnterpriseDB Team
        var numActiveBlocks = 0;//EnterpriseDB Team
        var variableDeclare = 0;//EnterpriseDB Team
        var end = sql.Length;
        var ch = '\0';
        int dollarTagStart;
        int dollarTagEnd;
        var currTokenBeg = 0;
        var blockCommentLevel = 0;
        var parenthesisLevel = 0;
        string sqlString, temp;

        sqlString = temp = sql; // EnterpriseDB Team

        if (ContainsSPLStartingKeyword(temp))
            isProcedure = true;

        None:
        if (currCharOfs >= end)
        {
            WriteDebug("Finish1");
            goto Finish;
        }
        var lastChar = ch;
        ch = sql[currCharOfs++];
    NoneContinue:
        while (true)
        {
            //EnterpriseDB Team
            if (isProcedure)
            {
                temp = sql.ToString().Substring(currCharOfs - 1);
            }

            if (isProcedure && temp.StartsWith("BEGIN", StringComparison.OrdinalIgnoreCase))
            {
                numActiveBlocks++;
                variableDeclare--;
            }


            if (isProcedure && temp.StartsWith("END", StringComparison.OrdinalIgnoreCase))
            {
                if (!(temp.StartsWith("END IF", StringComparison.OrdinalIgnoreCase)
                    || temp.StartsWith("END_", StringComparison.OrdinalIgnoreCase)
                    || temp.StartsWith("END LOOP", StringComparison.OrdinalIgnoreCase)
                    || temp.StartsWith("END CASE", StringComparison.OrdinalIgnoreCase)))
                    numActiveBlocks--;
            }

            if (isProcedure && temp.StartsWith("IS", StringComparison.OrdinalIgnoreCase)
                                || temp.StartsWith("AS", StringComparison.OrdinalIgnoreCase)
                                || temp.StartsWith("TRIGGER", StringComparison.OrdinalIgnoreCase))
            {
                var next = ' ';
                if (temp.StartsWith("TRIGGER", StringComparison.OrdinalIgnoreCase) && temp.Length > 7)
                {

                    next = temp[7];
                }
                else
                {
                    if (temp.Length > 2)
                    {
                        next = temp[2];
                    }
                }
                if (next == ' ' || next == '\n' || next == '\t' || next == ';')
                {
                    // EnterpriseDB
                    // AS is not a variable declaration in one line declaration such as
                    //  CREATE PROCEDURE name() AS 'SELECT 1' LANGUAGE sql;

                    if (temp.Length > 3)
                    {
                        next = temp[3];
                        if (next != '\'' && next != '$')
                            variableDeclare++;
                    }
                    else
                        variableDeclare++;
                }
            }

            if (isProcedure && temp.StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase))
            {
                var next = ' ';
                if (temp.Length > 7)
                {
                    next = temp[7];
                }
                if (next == ' ' || next == '\n' || next == '\t' || next == ';')
                {
                    variableDeclare++;
                }
            }
            // ^^^ EnterpriseDB Team
            switch (ch)
            {
            case '/':
                WriteDebug("BlockCommentBegin /");
                goto BlockCommentBegin;
            case '-':
                WriteDebug("LineCommentBegin -");
                goto LineCommentBegin;
            case '\'':
                if (standardConformingStrings)
                {
                    WriteDebug("Quoted \\");
                    goto Quoted;
                }
                WriteDebug("Escaped \\");
                goto Escaped;
            case '$':
                if (!IsIdentifier(lastChar))
                {
                    WriteDebug("DollarQuotedStart $");
                    goto DollarQuotedStart;
                }
                break;
            case '"':
                WriteDebug("Quoted \"");
                goto Quoted;
            case ':':
                if (lastChar != ':')
                {
                    WriteDebug("NamedParamStart :");
                    goto NamedParamStart;
                }
                break;
            case '@':
                if (lastChar != '@')
                {
                    WriteDebug("NamedParamStart @");
                    goto NamedParamStart;
                }
                break;
            case ';':
                if (parenthesisLevel == 0)
                {
                    WriteDebug("SemiColon ;");
                    goto SemiColon;
                }
                break;
            case '(':
                parenthesisLevel++;
                break;
            case ')':
                parenthesisLevel--;
                break;
            case 'e':
            case 'E'://EnterpriseDB Team
                if (!isProcedure)
                {
                    if (!IsLetter(lastChar))
                    {
                        WriteDebug("EscapedStart E/e");
                        goto EscapedStart;
                    }
                    else
                        break;
                }
                else
                {
                    break;
                }
            case 'x': //EnterpriseDB Team
            case 'X': //EnterpriseDB Team
                if (!IsLetter(lastChar))
                {
                    WriteDebug("EscapedStart X/x");
                    goto EscapedStart;
                }
                break;
            }

            if (currCharOfs >= end)
            {
                WriteDebug("Finish currCharOfs >= end");
                goto Finish;
            }

            lastChar = ch;
            ch = sql[currCharOfs++];
        }

    NamedParamStart:
        if (currCharOfs < end)
        {
            lastChar = ch;
            ch = sql[currCharOfs];
            if (IsParamNameChar(ch))
            {
                if (currCharOfs - 1 > currTokenBeg)
                    _rewrittenSql.Append(sql, currTokenBeg, currCharOfs - 1 - currTokenBeg);
                currTokenBeg = currCharOfs++ - 1;
                WriteDebug("NamedParamStart -> NamedParam");
                goto NamedParam;
            }
            currCharOfs++;
            WriteDebug("NamedParamStart -> NoneContinue");
            goto NoneContinue;
        }
        WriteDebug("NamedParamStart -> Finish");
        goto Finish;

    NamedParam:
        // We have already at least one character of the param name
        while (true)
        {
            lastChar = ch;
            if (currCharOfs >= end || !IsParamNameChar(ch = sql[currCharOfs]))
            {
                var paramName = sql.Substring(currTokenBeg + 1, currCharOfs - (currTokenBeg + 1));

                if (!_paramIndexMap.TryGetValue(paramName, out var index))
                {
                    // Parameter hasn't been seen before in this query
                    if (!parameters.TryGetValue(paramName, out var parameter))
                    {
                        if (deriveParameters)
                        {
                            parameter = new EDBParameter { ParameterName = paramName };
                            parameters.Add(parameter);
                        }
                        else
                        {
                            // Parameter placeholder does not match a parameter on this command.
                            // Leave the text as it was in the SQL, it may not be a an actual placeholder
                            _rewrittenSql.Append(sql, currTokenBeg, currCharOfs - currTokenBeg);
                            currTokenBeg = currCharOfs;
                            if (currCharOfs >= end)
                            {
                                WriteDebug("NamedParam -> Finish (parameters)");
                                goto Finish;
                            }

                            currCharOfs++;
                            WriteDebug("NamedParam -> NoneContinue (parameters)");
                            goto NoneContinue;
                        }
                    }

                    if (!parameter.IsInputDirection)
                        ThrowHelper.ThrowInvalidOperationException("Parameter '{0}' referenced in SQL but is an out-only parameter", paramName);

                    batchCommand.PositionalParameters.Add(parameter);
                    index = _paramIndexMap[paramName] = batchCommand.PositionalParameters.Count;
                }
                _rewrittenSql.Append('$');
                _rewrittenSql.Append(index);
                currTokenBeg = currCharOfs;

                if (currCharOfs >= end)
                {
                    WriteDebug("NamedParam -> Finish (currCharOfs >= end)");
                    goto Finish;
                }

                currCharOfs++;
                WriteDebug("NamedParam -> Finish (currCharOfs >= end)");
                goto NoneContinue;
            }

            currCharOfs++;
        }

    Quoted:
        Debug.Assert(ch == '\'' || ch == '"');
        while (currCharOfs < end && sql[currCharOfs] != ch)
        {
            currCharOfs++;
        }
        if (currCharOfs < end)
        {
            currCharOfs++;
            ch = '\0';
            WriteDebug("Quoted -> None");
            goto None;
        }
        WriteDebug("Quoted -> Finish");
        goto Finish;

    EscapedStart:
        if (currCharOfs < end)
        {
            lastChar = ch;
            ch = sql[currCharOfs++];
            if (ch == '\'')
            {
                WriteDebug("EscapedStart -> Escaped");
                goto Escaped;
            }
            WriteDebug("EscapedStart -> NoneContinue");
            goto NoneContinue;
        }
        WriteDebug("EscapedStart -> Finish");
        goto Finish;

    Escaped:
        while (currCharOfs < end)
        {
            ch = sql[currCharOfs++];
            switch (ch)
            {
            case '\'':
                WriteDebug("Escaped -> MaybeConcatenatedEscaped");
                goto MaybeConcatenatedEscaped;
            case '\\':
            {
                if (currCharOfs >= end)
                {
                    WriteDebug("Escaped -> Finish \\\\");
                    goto Finish;
                }
                currCharOfs++;
                break;
            }
            }
        }
        WriteDebug("Escaped -> Finish");
        goto Finish;

    MaybeConcatenatedEscaped:
        while (currCharOfs < end)
        {
            ch = sql[currCharOfs++];
            switch (ch)
            {
            case '\r':
            case '\n':
                WriteDebug("MaybeConcatenatedEscaped -> MaybeConcatenatedEscaped2 (/r/n)");
                goto MaybeConcatenatedEscaped2;
            case ' ':
            case '\t':
            case '\f':
                continue;
            default:
                lastChar = '\0';
                WriteDebug("MaybeConcatenatedEscaped -> NoneContinue (/0)");
                goto NoneContinue;
            }
        }
        WriteDebug("MaybeConcatenatedEscaped -> Finish");
        goto Finish;

    MaybeConcatenatedEscaped2:
        while (currCharOfs < end)
        {
            ch = sql[currCharOfs++];
            switch (ch)
            {
            case '\'':
                WriteDebug("MaybeConcatenatedEscaped2 -> Escaped (\\)");
                goto Escaped;
            case '-':
            {
                if (currCharOfs >= end)
                {
                    WriteDebug("MaybeConcatenatedEscaped2 -> Finish (-)");
                    goto Finish;
                }
                ch = sql[currCharOfs++];
                if (ch == '-')
                {
                    WriteDebug("MaybeConcatenatedEscaped2 -> MaybeConcatenatedEscapeAfterComment (-)");
                    goto MaybeConcatenatedEscapeAfterComment;
                }
                lastChar = '\0';
                WriteDebug("MaybeConcatenatedEscaped2 -> NoneContinue");
                goto NoneContinue;
            }
            case ' ':
            case '\t':
            case '\n':
            case '\r':
            case '\f':
                continue;
            default:
                lastChar = '\0';
                WriteDebug("MaybeConcatenatedEscaped2 -> NoneContinue (default)");
                goto NoneContinue;
            }
        }
        WriteDebug("MaybeConcatenatedEscaped2 -> Finish");
        goto Finish;

    MaybeConcatenatedEscapeAfterComment:
        while (currCharOfs < end)
        {
            ch = sql[currCharOfs++];
            if (ch == '\r' || ch == '\n')
            {
                WriteDebug("MaybeConcatenatedEscapeAfterComment -> MaybeConcatenatedEscaped2 (/r/n)");
                goto MaybeConcatenatedEscaped2;
            }
        }
        WriteDebug("MaybeConcatenatedEscapeAfterComment -> Finish");
        goto Finish;

    DollarQuotedStart:
        if (currCharOfs < end)
        {
            ch = sql[currCharOfs];
            if (ch == '$')
            {
                // Empty tag
                dollarTagStart = dollarTagEnd = currCharOfs;
                currCharOfs++;
                WriteDebug("DollarQuotedStart -> DollarQuoted");
                goto DollarQuoted;
            }
            if (IsIdentifierStart(ch))
            {
                dollarTagStart = currCharOfs;
                currCharOfs++;
                WriteDebug("DollarQuotedStart -> DollarQuotedInFirstDelim");
                goto DollarQuotedInFirstDelim;
            }
            lastChar = '$';
            currCharOfs++;
            WriteDebug("DollarQuotedStart -> NoneContinue");
            goto NoneContinue;
        }
        WriteDebug("DollarQuotedStart -> Finish");
        goto Finish;

    DollarQuotedInFirstDelim:
        while (currCharOfs < end)
        {
            lastChar = ch;
            ch = sql[currCharOfs++];
            if (ch == '$')
            {
                dollarTagEnd = currCharOfs - 1;
                WriteDebug("DollarQuotedInFirstDelim -> DollarQuoted");
                goto DollarQuoted;
            }
            if (!IsDollarTagIdentifier(ch))
            {
                WriteDebug("DollarQuotedInFirstDelim -> NoneContinue");
                goto NoneContinue;
            }
        }
        WriteDebug("DollarQuotedInFirstDelim -> Finish");
        goto Finish;

    DollarQuoted:
        var tag = sql.AsSpan(dollarTagStart - 1, dollarTagEnd - dollarTagStart + 2);
        var pos = sql.AsSpan(dollarTagEnd + 1).IndexOf(tag);
        if (pos == -1)
        {
            currCharOfs = end;
            WriteDebug("DollarQuoted -> Finish");
            goto Finish;
        }
        pos += dollarTagEnd + 1; // If the substring is found adjust the position to be relative to the entire string
        currCharOfs = pos + dollarTagEnd - dollarTagStart + 2;
        ch = '\0';
        WriteDebug("DollarQuoted -> None");
        goto None;

    LineCommentBegin:
        if (currCharOfs < end)
        {
            ch = sql[currCharOfs++];
            if (ch == '-')
            {
                WriteDebug("LineCommentBegin -> LineComment");
                goto LineComment;
            }
            lastChar = '\0';
            WriteDebug("LineCommentBegin -> NoneContinue");
            goto NoneContinue;
        }
        WriteDebug("LineCommentBegin -> Finish");
        goto Finish;

    LineComment:
        while (currCharOfs < end)
        {
            ch = sql[currCharOfs++];
            if (ch == '\r' || ch == '\n')
            {
                WriteDebug("LineComment -> None");
                goto None;
            }
        }
        WriteDebug("LineComment -> Finish");
        goto Finish;

    BlockCommentBegin:
        while (currCharOfs < end)
        {
            ch = sql[currCharOfs++];
            if (ch == '*')
            {
                blockCommentLevel++;
                WriteDebug("BlockCommentBegin -> BlockComment *");
                goto BlockComment;
            }
            if (ch != '/')
            {
                if (blockCommentLevel > 0)
                {
                    WriteDebug("BlockCommentBegin -> BlockComment");
                    goto BlockComment;
                }
                lastChar = '\0';
                WriteDebug("BlockCommentBegin -> NoneContinue");
                goto NoneContinue;
            }
        }
        WriteDebug("BlockCommentBegin -> Finish");
        goto Finish;

    BlockComment:
        while (currCharOfs < end)
        {
            ch = sql[currCharOfs++];
            switch (ch)
            {
            case '*':
                WriteDebug("BlockComment -> BlockCommentEnd");
                goto BlockCommentEnd;
            case '/':
                WriteDebug("BlockComment -> BlockCommentBegin");
                goto BlockCommentBegin;
            }
        }
        WriteDebug("BlockComment -> Finish");
        goto Finish;

    BlockCommentEnd:
        while (currCharOfs < end)
        {
            ch = sql[currCharOfs++];
            if (ch == '/')
            {
                if (--blockCommentLevel > 0)
                {
                    WriteDebug("BlockCommentEnd -> BlockComment /");
                    goto BlockComment;
                }
                WriteDebug("BlockCommentEnd -> None");
                goto None;
            }
            if (ch != '*')
            {
                WriteDebug("BlockCommentEnd -> BlockComment *");
                goto BlockComment;
            }
        }
        WriteDebug("BlockCommentEnd -> Finish");
        goto Finish;

    SemiColon:
        if (isProcedure && (numActiveBlocks > 0 || variableDeclare > 0)) //EnterpriseDB Team
        {
            currCharOfs++;
            WriteDebug("SemiColon -> None");
            goto None;
        }
        _rewrittenSql.Append(sql, currTokenBeg, currCharOfs - currTokenBeg - 1);
        batchCommand.FinalCommandText = _rewrittenSql.ToString();
        while (currCharOfs < end)
        {
            ch = sql[currCharOfs];
            if (char.IsWhiteSpace(ch))
            {
                currCharOfs++;
                continue;
            }
            // TODO: Handle end of line comment? Although psql doesn't seem to handle them...

            // We've found a non-whitespace character after a semicolon - this is legacy batching.

            if (command is null)
            {
                ThrowHelper.ThrowNotSupportedException($"Specifying multiple SQL statements in a single {nameof(EDBBatchCommand)} isn't supported, " +
                     "please remove all semicolons.");
            }

            statementIndex++;
            //EnterpriseDB Team
            isProcedure = false;
            if (sqlString != null)
            {
                temp = sqlString;
                if (ContainsSPLStartingKeyword(temp))
                    isProcedure = true;
            }
            MoveToNextBatchCommand();
            _paramIndexMap.Clear();
            _rewrittenSql.Clear();

            currTokenBeg = currCharOfs;
            WriteDebug("SemiColon -> None while");
            goto None;
        }
        if (batchCommands is not null && batchCommands.Count > statementIndex + 1)
            batchCommands.RemoveRange(statementIndex + 1, batchCommands.Count - (statementIndex + 1));
        return;

    Finish:
        _rewrittenSql.Append(sql, currTokenBeg, end - currTokenBeg);
        if (statementIndex is 0 && _paramIndexMap.Count is 0)
            // Single statement, no parameters, no rewriting necessary
            batchCommand.FinalCommandText = sql;
        else
            batchCommand.FinalCommandText = _rewrittenSql.ToString();
        if (batchCommands is not null && batchCommands.Count > statementIndex + 1)
            batchCommands.RemoveRange(statementIndex + 1, batchCommands.Count - (statementIndex + 1));

        void MoveToNextBatchCommand()
        {
            Debug.Assert(batchCommands is not null);
            if (batchCommands.Count > statementIndex)
            {
                batchCommand = batchCommands[statementIndex];
                batchCommand.Reset();
                batchCommand._parameters = parameters;
            }
            else
            {
                batchCommand = new EDBBatchCommand { _parameters = parameters };
                batchCommands.Add(batchCommand);
            }
        }
    }

    //EnterpriseDB Team
    private bool ContainsSPLStartingKeyword(string temp)
    {
        // abort if postgres
        if (!supportsRedwoodDialect)
            return false;

        // abort if redwood and plpgsql
#if NETSTANDARD2_0 || NETFRAMEWORK // EnterpriseDB (NETFRAMEWORK)
        if (temp.IndexOf("LANGUAGE PLPGSQL", StringComparison.OrdinalIgnoreCase) >= 0)
            return false;
#else
        if (temp.Contains("LANGUAGE PLPGSQL", StringComparison.OrdinalIgnoreCase))
            return false;
#endif


#if NETSTANDARD2_0 || NETFRAMEWORK // EnterpriseDB (NETFRAMEWORK)
        return (temp.StartsWith("CREATE", StringComparison.OrdinalIgnoreCase)
            || temp.StartsWith("DECLARE ", StringComparison.OrdinalIgnoreCase))
                        && (temp.Contains("PROCEDURE ")
                        || temp.Contains("FUNCTION ")
                        || temp.Contains("TRIGGER ")
                        || temp.Contains("DECLARE ")
                        || temp.Contains("PACKAGE "));
#else
        return (temp.StartsWith("CREATE", StringComparison.OrdinalIgnoreCase)
            || temp.StartsWith("DECLARE ", StringComparison.OrdinalIgnoreCase))
                        && (temp.Contains("PROCEDURE ", StringComparison.OrdinalIgnoreCase)
                        || temp.Contains("FUNCTION ", StringComparison.OrdinalIgnoreCase)
                        || temp.Contains("TRIGGER ", StringComparison.OrdinalIgnoreCase)
                        || temp.Contains("DECLARE ", StringComparison.OrdinalIgnoreCase)
                        || temp.Contains("PACKAGE ", StringComparison.OrdinalIgnoreCase));
#endif
    }

    // Is ASCII letter comparison optimization https://github.com/dotnet/runtime/blob/60cfaec2e6cffeb9a006bec4b8908ffcf71ac5b4/src/libraries/System.Private.CoreLib/src/System/Char.cs#L236

    static bool IsLetter(char ch)
        // [a-zA-Z]
        => (uint)((ch | 0x20) - 'a') <= ('z' - 'a');

    static bool IsIdentifierStart(char ch)
        // [a-zA-Z_\x80-\xFF]
        => (uint)((ch | 0x20) - 'a') <= ('z' - 'a') || ch == '_' || (uint)(ch - 128) <= 127u;

    static bool IsDollarTagIdentifier(char ch)
        // [a-zA-Z0-9_\x80-\xFF]
        => (uint)((ch | 0x20) - 'a') <= ('z' - 'a') || (uint)(ch - '0') <= ('9' - '0') || ch == '_' || (uint)(ch - 128) <= 127u;

    static bool IsIdentifier(char ch)
        // [a-zA-Z0-9_$\x80-\xFF]
        => (uint)((ch | 0x20) - 'a') <= ('z' - 'a') || (uint)(ch - '0') <= ('9' - '0') || ch == '_' || ch == '$' || (uint)(ch - 128) <= 127u;

    static bool IsParamNameChar(char ch)
        => char.IsLetterOrDigit(ch) || ch == '_' || ch == '.';  // why dot??

    [Conditional("EDB_DIAGNOSTICS")]
    static void WriteDebug(string message)
    {
        Debug.WriteLine("SqlQueryParser: " + message);
    }
}
