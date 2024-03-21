// created on 18/11/2013

// EnterpriseDB.EDBClient.EDBCommand.PrepareExecute.cs
//
// Author:
//    Francisco Jr. (fxjrlists@yahoo.com.br)
//
//    Copyright (C) 2002 The EnterpriseDB.EDBClient Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using EDBTypes;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents a SQL statement or function (stored procedure) to execute
    /// against a PostgreSQL database. This class cannot be inherited.
    /// </summary>
    public sealed partial class EDBCommand : DbCommand, ICloneable
    {
        /// <summary>
        /// Internal query shortcut for use in cases where the number
        /// of affected rows is of no interest.
        /// </summary>
        internal static void ExecuteBlind(EDBConnector connector, string command)
        {
            // Bypass cpmmand parsing overhead and send command verbatim.
            ExecuteBlind(connector, new EDBQuery(command));
        }

        internal static void ExecuteBlind(EDBConnector connector, EDBQuery query)
        {
            // Block the notification thread before writing anything to the wire.
            using (var blocker = connector.BlockNotificationThread())
            {
                // Set statement timeout as needed.
                connector.SetBackendCommandTimeout(20);

                // Write the Query message to the wire.
                connector.Query(query);

                // Flush, and wait for and discard all responses.
                connector.ProcessAndDiscardBackendResponses();
            }
        }

        internal static void ExecuteBlindSuppressTimeout(EDBConnector connector, string command)
        {
            // Bypass cpmmand parsing overhead and send command verbatim.
            ExecuteBlindSuppressTimeout(connector, new EDBQuery(command));
        }

        internal static void ExecuteBlindSuppressTimeout(EDBConnector connector, EDBQuery query)
        {
            // Block the notification thread before writing anything to the wire.
            using (var blocker = connector.BlockNotificationThread())
            {
                // Write the Query message to the wire.
                connector.Query(query);

                // Flush, and wait for and discard all responses.
                connector.ProcessAndDiscardBackendResponses();
            }
        }

        /// <summary>
        /// Special adaptation of ExecuteBlind() that sets statement_timeout.
        /// This exists to prevent Connector.SetBackendCommandTimeout() from calling Command.ExecuteBlind(),
        /// which will cause an endless recursive loop.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="timeout">Timeout in seconds.</param>
        internal static void ExecuteSetStatementTimeoutBlind(EDBConnector connector, int timeout)
        {
            EDBQuery query;

            // Optimize for a few common timeout values.
            switch (timeout)
            {
                case 10 :
                    query = EDBQuery.SetStmtTimeout10Sec;
                    break;

                case 20 :
                    query = EDBQuery.SetStmtTimeout20Sec;
                    break;

                case 30 :
                    query = EDBQuery.SetStmtTimeout30Sec;
                    break;

                case 60 :
                    query = EDBQuery.SetStmtTimeout60Sec;
                    break;

                case 90 :
                    query = EDBQuery.SetStmtTimeout90Sec;
                    break;

                case 120 :
                    query = EDBQuery.SetStmtTimeout120Sec;
                    break;

                default :
                    query = new EDBQuery(string.Format("SET statement_timeout = {0}", timeout * 1000));
                    break;

            }

            // Write the Query message to the wire.
            connector.Query(query);

            // Flush, and wait for and discard all responses.
            connector.ProcessAndDiscardBackendResponses();
        }

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected if known; -1 otherwise.</returns>
        public override Int32 ExecuteNonQuery()
        {
            //We treat this as a simple wrapper for calling ExecuteReader() and then
            //update the records affected count at every call to NextResult();
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteNonQuery");
            int? ret = null;
            using (EDBDataReader rdr = GetReader(CommandBehavior.SequentialAccess))
            {
                do
                {
                    int thisRecord = rdr.RecordsAffected;
                    if (thisRecord != -1)
                    {
                        ret = (ret ?? 0) + thisRecord;
                    }
                    lastInsertedOID = rdr.LastInsertedOID ?? lastInsertedOID;
                }
                while (rdr.NextResult());
            }
            return ret ?? -1;
        }

        /// <summary>
        /// Sends the <see cref="EnterpriseDB.EDBClient.EDBCommand.CommandText">CommandText</see> to
        /// the <see cref="EnterpriseDB.EDBClient.EDBConnection">Connection</see> and builds a
        /// <see cref="EnterpriseDB.EDBClient.EDBDataReader">EDBDataReader</see>
        /// using one of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.
        /// </summary>
        /// <param name="behavior">One of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.</param>
        /// <returns>A <see cref="EnterpriseDB.EDBClient.EDBDataReader">EDBDataReader</see> object.</returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
        }

        /// <summary>
        /// Sends the <see cref="EnterpriseDB.EDBClient.EDBCommand.CommandText">CommandText</see> to
        /// the <see cref="EnterpriseDB.EDBClient.EDBConnection">Connection</see> and builds a
        /// <see cref="EnterpriseDB.EDBClient.EDBDataReader">EDBDataReader</see>.
        /// </summary>
        /// <returns>A <see cref="EnterpriseDB.EDBClient.EDBDataReader">EDBDataReader</see> object.</returns>
        public new EDBDataReader ExecuteReader()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteReader");

            return ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>
        /// Sends the <see cref="EnterpriseDB.EDBClient.EDBCommand.CommandText">CommandText</see> to
        /// the <see cref="EnterpriseDB.EDBClient.EDBConnection">Connection</see> and builds a
        /// <see cref="EnterpriseDB.EDBClient.EDBDataReader">EDBDataReader</see>
        /// using one of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.
        /// </summary>
        /// <param name="cb">One of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.</param>
        /// <returns>A <see cref="EnterpriseDB.EDBClient.EDBDataReader">EDBDataReader</see> object.</returns>
        /// <remarks>Currently the CommandBehavior parameter is ignored.</remarks>
        public new EDBDataReader ExecuteReader(CommandBehavior cb)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteReader", cb);

            // Close connection if requested even when there is an error.

            try
            {
                if (connection != null)
                {
                    if (connection.PreloadReader)
                    {
                        //Adjust behaviour so source reader is sequential access - for speed - and doesn't close the connection - or it'll do so at the wrong time.
                        CommandBehavior adjusted = (cb | CommandBehavior.SequentialAccess) & ~CommandBehavior.CloseConnection;

                        return new CachingDataReader(GetReader(adjusted), cb);
                    }
                }

                return GetReader(cb);
            }
            catch (Exception)
            {
                if ((cb & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
                {
                    connection.Close();
                }
                  
                throw;
            }
        }

        internal ForwardsOnlyDataReader GetReader(CommandBehavior cb)
        {
            CheckConnectionState();
            
            // Block the notification thread before writing anything to the wire.
            using (m_Connector.BlockNotificationThread())
            {
                IEnumerable<IServerResponseObject> responseEnum;
                ForwardsOnlyDataReader reader;

                m_Connector.SetBackendCommandTimeout(CommandTimeout);

                if (prepared == PrepareStatus.NeedsPrepare)
                {
                    PrepareInternal();
                }
               

                if (prepared == PrepareStatus.NotPrepared)
                {
                    EDBQuery query;
                    byte[] commandText = GetCommandText();

                    query = new EDBQuery(commandText);

                    // Write the Query message to the wire.
                    m_Connector.Query(query);

                    // Tell to mediator what command is being sent.
                    if (prepared == PrepareStatus.NotPrepared)
                    {
                        m_Connector.Mediator.SetSqlSent(commandText, EDBMediator.SQLSentType.Simple);
                    }
                    else
                    {
                        m_Connector.Mediator.SetSqlSent(preparedCommandText, EDBMediator.SQLSentType.Execute);
                    }

                    // Flush and wait for responses.
                    responseEnum = m_Connector.ProcessBackendResponsesEnum();

                    // Construct the return reader.
                    reader = new ForwardsOnlyDataReader(
                        responseEnum,
                        cb,
                        this,
                        m_Connector.BlockNotificationThread()
                    );

                    if (
                        commandType == CommandType.StoredProcedure
                        && reader.FieldCount == 1
                        && reader.GetDataTypeName(0) == "refcursor"
                    )
                    {
                        // When a function returns a sole column of refcursor, transparently
                        // FETCH ALL from every such cursor and return those results.
                        StringWriter sw = new StringWriter();
                        string queryText;

                        while (reader.Read())
                        {
                            sw.WriteLine("FETCH ALL FROM \"{0}\";", reader.GetString(0));
                        }

                        reader.Dispose();

                        queryText = sw.ToString();

                        if (queryText == "")
                        {
                            queryText = ";";
                        }

                        // Passthrough the commandtimeout to the inner command, so user can also control its timeout.
                        // TODO: Check if there is a better way to handle that.

                        query = new EDBQuery(queryText);

                        // Write the Query message to the wire.
                        m_Connector.Query(query);

                        // Flush and wait for responses.
                        responseEnum = m_Connector.ProcessBackendResponsesEnum();

                        // Construct the return reader.
                        reader = new ForwardsOnlyDataReader(
                            responseEnum,
                            cb,
                            this,
                            m_Connector.BlockNotificationThread()
                        );
                    }
                }
                else
                {
                    // Update the Bind object with current parameter data as needed.
                    
                    BindParameters();
                    
                    /* EnterpriseDB Team */
                    if (commandType == CommandType.StoredProcedure)
                    {
                        EDBDescribe statementDescribe = new EDBDescribePortal(""); //ZK
                        EDBDescribeOut statementDescribeOut = new EDBDescribeOutPortal(""); //ZK
                        m_Connector.Bind(bind);
                        m_Connector.Describe(statementDescribe); //ZK
                        m_Connector.DescribeOut(statementDescribeOut); //ZK
                        m_Connector.Execute(execute);
                        m_Connector.ExecuteOut(executeOut);

                        m_Connector.Sync();

                    }
                    else
                    {
                        // Write the Bind, Execute, and Sync message to the wire.
                        m_Connector.Bind(bind);
                      //  m_Connector.Describe(statementDescribe); //TODO ZK: check if extra and not in vanila version
                        m_Connector.Execute(execute);
                    }
                    
                    m_Connector.Sync();
                    
                    // Tell to mediator what command is being sent.
                    m_Connector.Mediator.SetSqlSent(preparedCommandText, EDBMediator.SQLSentType.Execute);

                    // Flush and wait for responses.
                    responseEnum = m_Connector.ProcessBackendResponsesEnum();

                    // Construct the return reader, possibly with a saved row description from Prepare().
                    reader = new ForwardsOnlyDataReader(
                        responseEnum,
                        cb,
                        this,
                        m_Connector.BlockNotificationThread(),
                        true,
                        currentRowDescription
                    );

                    if (m_Connector.Mediator.Type == System.Data.CommandType.StoredProcedure)
                    {
                        foreach (EDBParameter p in m_Connector.Mediator.Parameters)
                        {

                            /*
                             * Check if paramter of type recursor
                             * Bind a data reader with as paramter value
                             * 
                             */
                            if (p.EDBDbType == EDBDbType.RefCursor)
                            {
                                /*
                                 * if refcurosor is null, then value of the paramter will be empty string
                                 */
                                if (p.Value.ToString().Trim().Equals("fetch all in \"\""))
                                {
                                    p.Value = "";
                                }
                                else
                                {
                                    /*
                                     * Check if refcursor value is sent as null from server. Else
                                     * make a query to fetch all of the cursor result.
                                     */
                                    if (p.Value != DBNull.Value)
                                    {
                                        p.Value = "fetch all in \"" + p.Value.ToString() + "\"";
                                        EDBCommand command = new EDBCommand(p.Value.ToString(), Connection);
                                        m_Connector.Mediator.Type = command.CommandType;
#pragma warning disable CS0219 // Variable is assigned but its value is never used
                                        CommandBehavior cb1 = CommandBehavior.SequentialAccess;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
                                        ForwardsOnlyDataReader rd = (ForwardsOnlyDataReader)command.ExecuteReader(cb);
                                        m_Connector.Mediator.ExecutingRefCursor = true;
                                        p.Value = new CachingDataReader(rd, cb);
                                        //  while (rd.Read()) { }
                                    }
                                }
                            }
                        }
                    }
                }

                return reader;
            }
        }

        ///<summary>
        /// This method binds the parameters from parameters collection to the bind
        /// message.
        /// </summary>
        private void BindParameters()
        {
            if (parameters.Count != 0)
            {
                byte[][] parameterValues = bind.ParameterValues;
                Int16[] parameterFormatCodes = bind.ParameterFormatCodes;
                bool bindAll = false;
                bool bound = false;

                if (parameterValues == null || parameterValues.Length != parameters.Count)
                {
                    parameterValues = new byte[parameters.Count][];
                    bindAll = true;
                }

                for (Int32 i = 0; i < parameters.Count; i++)
                {
                    if (! bindAll && parameters[i].Bound)
                    {
                        continue;
                    }

                    parameterValues[i] = parameters[i].TypeInfo.ConvertToBackend(parameters[i].Value, true, Connector.NativeToBackendTypeConverterOptions);

                    bound = true;
                    parameters[i].Bound = true;
                    /*EnterpriseDB Team  //ZK TODO Do we need this check of Date and refcursor ?*/
                    if (parameterValues[i] == null && parameters[i].EDBDbType != EDBDbType.Date && parameters[i].EDBDbType != EDBDbType.RefCursor)
                    {
                        parameterFormatCodes[i]= (Int16)FormatCode.Binary;
                    } else {
                        parameterFormatCodes[i] = parameters[i].TypeInfo.SupportsBinaryBackendData ? (Int16)FormatCode.Binary : (Int16)FormatCode.Text;
                    }
                }

                if (bound)
                {
                    bind.ParameterValues = parameterValues;
                    bind.ParameterFormatCodes = parameterFormatCodes;
                }
            }
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row
        /// in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <returns>The first column of the first row in the result set,
        /// or a null reference if the result set is empty.</returns>
        public override Object ExecuteScalar()
        {
            using (
                EDBDataReader reader =
                    GetReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow))
            {
#pragma warning disable CS8603 // Possible null reference return.
                return reader.Read() && reader.FieldCount != 0 ? reader.GetValue(0) : null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        private void UnPrepare()
        {
            if (prepared == PrepareStatus.Prepared)
            {
                ExecuteBlind(m_Connector, "DEALLOCATE " + planName);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                bind = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                execute = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                currentRowDescription = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                prepared = PrepareStatus.NeedsPrepare;
            }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            preparedCommandText = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        /// <summary>
        /// Creates a prepared version of the command on a PostgreSQL server.
        /// </summary>
        public override void Prepare()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Prepare");

            // Check the connection state.
            CheckConnectionState();

            UnPrepare();
          //  Connector.Mediator.ResetResponses();
           // Connector.Mediator.R
            PrepareInternal();
        }

        private void PrepareInternal()
        {
            // Use the extended query parsing...
            planName = m_Connector.NextPlanName();
            String portalName = "";
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            EDBParse parse = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            EDBParseOut parseOut = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string callableStmtText = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // reset any responses just before getting new ones
       //     Connector.Mediator.ResetResponses();
            

            EDBCommand command = new EDBCommand(); //ZK TODO: check if this is needed
            command.CommandType = commandType;

            /*EnterpriseDB Team */
            if (CommandType == CommandType.StoredProcedure)
            {
                m_Connector.Mediator.parameters = this.parameters;
                foreach (EDBParameter edbParameter in this.parameters)
                {
                    if (edbParameter.TypeInfo.EDBDbType == EDBDbType.RefCursor)
                    {
                        m_Connector.Mediator.hasRefcursorType = true;
                 //       m_Connector.Mediator.parameters = this.parameters;
                        break;
                    }
                //    context.Stream.Flush();
                    
              
                }

                m_Connector.Mediator.Type = commandType;
                callableStmtText = GetParseCommandText();
                parseOut = new EDBParseOut(planName, callableStmtText , new Int32[] { }, parameters, command);
             //   prepared = PrepareStatus.V3Prepared; //TODO ZK: check if its needed ?
                       

            }
            else
            {
                preparedCommandText = GetCommandText(true);
                parse = new EDBParse(planName, preparedCommandText, new Int32[] { });

            }
            
            EDBDescribe statementDescribe = new EDBDescribeStatement(planName); //ZK
            IEnumerable<IServerResponseObject> responseEnum;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            EDBRowDescription returnRowDesc = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            
            
            // Write Parse, Describe, and Sync messages to the wire.
            if (CommandType == CommandType.StoredProcedure) {
#pragma warning disable CS8604 // Possible null reference argument.
                m_Connector.ParseOut(parseOut);
#pragma warning restore CS8604 // Possible null reference argument.
            }
 
            else
#pragma warning disable CS8604 // Possible null reference argument.
                m_Connector.Parse(parse);
#pragma warning restore CS8604 // Possible null reference argument.
            
            m_Connector.Describe(statementDescribe); //ZK
            m_Connector.Sync();


          
            if(commandType == CommandType.StoredProcedure)
            // Tell to mediator what command is being sent.
                m_Connector.Mediator.SetSqlSent(BackendEncoding.UTF8Encoding.GetBytes(callableStmtText),EDBMediator.SQLSentType.Parse);
            else
                m_Connector.Mediator.SetSqlSent(preparedCommandText,EDBMediator.SQLSentType.Parse);
          
            // Flush and wait for response.
            responseEnum = m_Connector.ProcessBackendResponsesEnum();

            // Look for a EDBRowDescription in the responses, discarding everything else.
            foreach (IServerResponseObject response in responseEnum)
            {
                if (response is EDBRowDescription)
                {
                    returnRowDesc = (EDBRowDescription)response;
                }
                else if (response is IDisposable)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    (response as IDisposable).Dispose();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
            }

            Int16[] resultFormatCodes;

            if (returnRowDesc != null)
            {
                resultFormatCodes = new Int16[returnRowDesc.NumFields];

                for (int i = 0; i < returnRowDesc.NumFields; i++)
                {
                    EDBRowDescription.FieldData returnRowDescData = returnRowDesc[i];

                    if (returnRowDescData.TypeInfo != null)
                    {
                        // Binary format?
                        // PG always defaults to text encoding.  We can fix up the row description
                        // here based on support for binary encoding.  Once this is done,
                        // there is no need to request another row description after Bind.
                        returnRowDescData.FormatCode = returnRowDescData.TypeInfo.SupportsBinaryBackendData ? FormatCode.Binary : FormatCode.Text;
                        resultFormatCodes[i] = (Int16)returnRowDescData.FormatCode;
                    }
                    else
                    {
                        // Text format (default).
                        resultFormatCodes[i] = (Int16)FormatCode.Text;
                    }
                }
            }
            else
            {
                resultFormatCodes = new Int16[] { 0 };
            }

            // Save the row description for use with all future Executes.
#pragma warning disable CS8601 // Possible null reference assignment.
            currentRowDescription = returnRowDesc;
#pragma warning restore CS8601 // Possible null reference assignment.

            // The Bind and Execute message objects live through multiple Executes.
            // Only Bind changes at all between Executes, which is done in BindParameters().
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            bind = new EDBBind(portalName, planName, new Int16[Parameters.Count], null, resultFormatCodes);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            execute = new EDBExecute(portalName, 0);
            executeOut = new EDBExecuteOut(portalName, 0);
            prepared = PrepareStatus.Prepared;
        }
    }
}
