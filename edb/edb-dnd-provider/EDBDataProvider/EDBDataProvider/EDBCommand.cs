// created on 21/5/2002 at 20:03

// EDB.EDBCommand.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The EDB Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Text;
using System.Resources;
using System.ComponentModel;
using System.Collections;
using EDBTypes;
using EnterpriseDB.EDBClient.Design;
using System.Text.RegularExpressions;
using System.IO;

#if WITHDESIGN
using EDB.Design;
#endif


namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents a SQL statement or function (stored procedure) to execute
    /// against a PostgreSQL database. This class cannot be inherited.
    /// </summary>
    #if WITHDESIGN
    [System.Drawing.ToolboxBitmapAttribute(typeof(EDBCommand)), ToolboxItem(true)]
    #endif
    public sealed class EDBCommand : Component, IDbCommand, ICloneable
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlCommand";
        private static ResourceManager resman = new ResourceManager(typeof(EDBCommand));
        private static readonly Regex parameterReplace = new Regex(@"([:@][\w\.]*)", RegexOptions.Singleline);

        private EDBConnection            connection;
        private EDBConnector             connector;
        private EDBTransaction           transaction;
        private String                      text;
        private Int32                       timeout;
        private CommandType                 type;
        private EDBParameterCollection   parameters;
        private String                      planName;

        private EDBParse                 parse;
        private EDBBind                  bind;

        private Boolean						invalidTransactionDetected = false;
			
        private CommandBehavior             commandBehavior;

        private Int64                       lastInsertedOID = 0;

        // Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EDB.EDBCommand">EDBCommand</see> class.
        /// </summary>
        public EDBCommand() : this(String.Empty, null, null)
        {}
        /// <summary>
        /// Initializes a new instance of the <see cref="EDB.EDBCommand">EDBCommand</see> class with the text of the query.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        public EDBCommand(String cmdText) : this(cmdText, null, null)
        {}
        /// <summary>
        /// Initializes a new instance of the <see cref="EDB.EDBCommand">EDBCommand</see> class with the text of the query and a <see cref="EDB.EDBConnection">EDBConnection</see>.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">A <see cref="EDB.EDBConnection">EDBConnection</see> that represents the connection to a PostgreSQL server.</param>
        public EDBCommand(String cmdText, EDBConnection connection) : this(cmdText, connection, null)
        {
		

		}
        /// <summary>
        /// Initializes a new instance of the <see cref="EDB.EDBCommand">EDBCommand</see> class with the text of the query, a <see cref="EDB.EDBConnection">EDBConnection</see>, and the <see cref="EDB.EDBTransaction">EDBTransaction</see>.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">A <see cref="EDB.EDBConnection">EDBConnection</see> that represents the connection to a EnterpriseDB server.</param>
        /// <param name="transaction">The <see cref="EDB.EDBTransaction">EDBTransaction</see> in which the <see cref="EDB.EDBCommand">EDBCommand</see> executes.</param>
        public EDBCommand(String cmdText, EDBConnection connection, EDBTransaction transaction)
        {	
			
          
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
			planName = String.Empty;
            text = cmdText;
            this.connection = connection;
            
            if (this.connection != null)
                this.connector = connection.Connector;
		
		
			
            parameters = new EDBParameterCollection();
            
            type = CommandType.Text;
            this.Transaction = transaction;
            commandBehavior = CommandBehavior.Default;
            
            SetCommandTimeout();
            
            
        }

        /// <summary>
        /// Used to execute internal commands.
        /// </summary>
        internal EDBCommand(String cmdText, EDBConnector connector)
        {
            resman = new System.Resources.ResourceManager(this.GetType());
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
            

            planName = String.Empty;
            text = cmdText;
            this.connector = connector;
            type = CommandType.Text;
            commandBehavior = CommandBehavior.Default;
            
            parameters = new EDBParameterCollection();
            timeout = 20;
        }

        // Public properties.
        /// <summary>
        /// Gets or sets the SQL statement or function (stored procedure) to execute at the data source.
        /// </summary>
        /// <value>The Transact-SQL statement or stored procedure to execute. The default is an empty string.</value>
        [Category("Data"), DefaultValue("")]
        public String CommandText {
            get
            {
                return text;
            }

            set
            {
                // [TODO] Validate commandtext.
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "CommandText", value);
                text = value;
                planName = String.Empty;
                parse = null;
                bind = null;
                commandBehavior = CommandBehavior.Default;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt
        /// to execute a command and generating an error.
        /// </summary>
        /// <value>The time (in seconds) to wait for the command to execute.
        /// The default is 20 seconds.</value>
        [DefaultValue(20)]
        public Int32 CommandTimeout {
            get
            {
                return timeout;
            }

            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(resman.GetString("Exception_CommandTimeoutLessZero"));

                timeout = value;
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "CommandTimeout", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how the
        /// <see cref="EDB.EDBCommand.CommandText">CommandText</see> property is to be interpreted.
        /// </summary>
        /// <value>One of the <see cref="System.Data.CommandType">CommandType</see> values. The default is <see cref="System.Data.CommandType">CommandType.Text</see>.</value>
        [Category("Data"), DefaultValue(CommandType.Text)]
        public CommandType CommandType {
            get
            {
                return type;
            }

            set
            {
                type = value;
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "CommandType", value);
            }
        }

        IDbConnection IDbCommand.Connection 
        {
            get
            {
                return Connection;
            }

            set
            {
                Connection = (EDBConnection) value;
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "IDbCommand.Connection", value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="EDB.EDBConnection">EDBConnection</see>
        /// used by this instance of the <see cref="EDB.EDBCommand">EDBCommand</see>.
        /// </summary>
        /// <value>The connection to a data source. The default value is a null reference.</value>
        [Category("Behavior"), DefaultValue(null)]
        public EDBConnection Connection {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Connection");
                return connection;
            }

            set
            {
                if (this.Connection == value)
                    return;

                //if (this.transaction != null && this.transaction.Connection == null)
                  //  this.transaction = null;
                                    
                if (this.transaction != null && this.connection != null && this.Connector.Transaction != null)
                    throw new InvalidOperationException(resman.GetString("Exception_SetConnectionInTransaction"));


                this.connection = value;
                Transaction = null;
                if (this.connection != null)
                    connector = this.connection.Connector;

				
                SetCommandTimeout();
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "Connection", value);
            }
        }
		
        internal EDBConnector Connector {
            get
            {
                if (connector == null && this.connection != null)
                    connector = this.connection.Connector;

                return connector;
            }
        }

        IDataParameterCollection IDbCommand.Parameters {
            get
            {
                return Parameters;
            }
        }
        /// <summary>
        /// Gets the <see cref="EDB.EDBParameterCollection">EDBParameterCollection</see>.
        /// </summary>
        /// <value>The parameters of the SQL statement or function (stored procedure). The default is an empty collection.</value>
        #if WITHDESIGN
        [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        #endif
        
        public EDBParameterCollection Parameters {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Parameters");
                return parameters;
            }
        }

//EDB Team: Patch of new npgsql release         
        IDbTransaction IDbCommand.Transaction 
        {
            get
            {
                return Transaction;
            }

            set
            {
                Transaction = (EDBTransaction) value;
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "IDbCommand.Transaction", value);
            }
        }
        
        /// <summary>
        /// Gets or sets the <see cref="EDB.EDBTransaction">EDBTransaction</see>
        /// within which the <see cref="EDB.EDBCommand">EDBCommand</see> executes.
        /// </summary>
        /// <value>The <see cref="EDB.EDBTransaction">EDBTransaction</see>.
        /// The default value is a null reference.</value>
        #if WITHDESIGN
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        #endif
        
        public EDBTransaction Transaction {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Transaction");

                if (this.transaction != null && this.transaction.Connection == null)
                {
                    this.transaction = null;
                }
                return this.transaction;
            }

            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "Transaction" ,value);

                this.transaction = (EDBTransaction) value;
            }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the <see cref="System.Data.DataRow">DataRow</see>
        /// when used by the <see cref="System.Data.Common.DbDataAdapter.Update">Update</see>
        /// method of the <see cref="System.Data.Common.DbDataAdapter">DbDataAdapter</see>.
        /// </summary>
        /// <value>One of the <see cref="System.Data.UpdateRowSource">UpdateRowSource</see> values.</value>
        #if WITHDESIGN
        [Category("Behavior"), DefaultValue(UpdateRowSource.Both)]
        #endif
        
        public UpdateRowSource UpdatedRowSource {
            get
            {

                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "UpdatedRowSource");

                return UpdateRowSource.Both;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns oid of inserted row. This is only updated when using executenonQuery and when command inserts just a single row. If table is created without oids, this will always be 0.
        /// </summary>

	public Int64 LastInsertedOID
        {
            get
            {
                return lastInsertedOID;
            }
        }


        /// <summary>
        /// Attempts to cancel the execution of a <see cref="EDBCommand">EDBCommand</see>.
        /// </summary>
        /// <remarks>This Method isn't implemented yet.</remarks>
        public void Cancel()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Cancel");

			try
			{
				// get copy for thread safety of null test
				EDBConnector connector = Connector;
				if (connector != null)
				{
					connector.CancelRequest();
				}
			}
			catch (IOException)
			{
				Connection.ClearPool();
			}   
			catch (EDBException)
			{
				// Cancel documentation says the Cancel doesn't throw on failure
			}

        }
//EDB Team: Patch of new npgsql release               
        /// <summary>
        /// Create a new command based on this one.
        /// </summary>
        /// <returns>A new EDBCommand object.</returns>
        Object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Create a new connection based on this one.
        /// </summary>
        /// <returns>A new EDBConnection object.</returns>
        public EDBCommand Clone()
        {
            // TODO: Add consistency checks.

            return new EDBCommand(CommandText, Connection, Transaction);
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataParameter">IDbDataParameter</see> object.
        /// </summary>
        /// <returns>An <see cref="System.Data.IDbDataParameter">IDbDataParameter</see> object.</returns>
        IDbDataParameter IDbCommand.CreateParameter()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbCommand.CreateParameter");

            return (EDBParameter) CreateParameter();
        }

        /// <summary>
        /// Creates a new instance of a <see cref="EDB.EDBParameter">EDBParameter</see> object.
        /// </summary>
        /// <returns>A <see cref="EDB.EDBParameter">EDBParameter</see> object.</returns>
        public EDBParameter CreateParameter()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateParameter");

            return new EDBParameter();
        }

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public Int32 ExecuteNonQuery()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteNonQuery");
			if(parameters!=null)
				connector.Mediator.setParameters(parameters);
			lastInsertedOID = 0;
            ExecuteCommand();
            
            //UpdateOutputParameters();//EDB Team: Patch of new npgsql release       
            
            // If nothing is returned, just return -1.
            if(Connector.Mediator.CompletedResponses.Count == 0)
            {
                return -1;
            }

            // Check if the response is available.
            String firstCompletedResponse = (String)Connector.Mediator.CompletedResponses[0];

            if (firstCompletedResponse == null)
                return -1;

            String[] ret_string_tokens = firstCompletedResponse.Split(null);        // whitespace separator.


            // Check if the command was insert, delete or update.
            // Only theses commands return rows affected.
            // [FIXME] Is there a better way to check this??
            if ((String.Compare(ret_string_tokens[0], "INSERT", true) == 0) ||
                    (String.Compare(ret_string_tokens[0], "UPDATE", true) == 0) ||
                    (String.Compare(ret_string_tokens[0], "DELETE", true) == 0) ||
                    (String.Compare(ret_string_tokens[0], "FETCH", true) == 0) ||
                    (String.Compare(ret_string_tokens[0], "MOVE", true) == 0))
                
                
            {
                if (String.Compare(ret_string_tokens[0], "INSERT", true) == 0)
                    // Get oid of inserted row.
                    lastInsertedOID = Int32.Parse(ret_string_tokens[1]);

                // The number of rows affected is in the third token for insert queries
                // and in the second token for update and delete queries.
                // In other words, it is the last token in the 0-based array.

                return Int32.Parse(ret_string_tokens[ret_string_tokens.Length - 1]);
            }
            else
                return -1;
        }
        
//EDB Team: Patch of new npgsql release               
        
        private void UpdateOutputParameters()
        {
            // Check if there was some resultset returned. If so, put the result in output parameters.
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "UpdateOutputParameters");
            
            // Get ResultSets.
            ArrayList resultSets = Connector.Mediator.ResultSets;
            
            if (resultSets.Count != 0)
            {
                EDBResultSet nrs = (EDBResultSet)resultSets[0];
                                
                if ((nrs != null) && (nrs.Count > 0))
                {
                    EDBAsciiRow nar = (EDBAsciiRow)nrs[0];
                    
                    Int32 i = 0;
                    Boolean hasMapping = false;
                                        
                    // First check if there is any mapping between parameter name and resultset name.
                    // If so, just update output parameters which has mapping.
                    
                    foreach (EDBParameter p in Parameters)
                    {
                        try
                        {
                            if (nrs.RowDescription.FieldIndex(p.ParameterName.Substring(1)) > -1)
                            {
                                hasMapping = true;
                                break;
                            }
                        }
                        catch(ArgumentOutOfRangeException)
                        {}
                    }
                                        
                    
                    if (hasMapping)
                    {
                        foreach (EDBParameter p in Parameters)
                        {
                            if (((p.Direction == ParameterDirection.Output) ||
                                (p.Direction == ParameterDirection.InputOutput)) && (i < nrs.RowDescription.NumFields ))
                            {
                                try
                                {
                                    p.Value = nar[nrs.RowDescription.FieldIndex(p.ParameterName.Substring(1))];
                                    i++;
                                }
                                catch(ArgumentOutOfRangeException)
                                {}
                            }
                        }
                        
                    }
                    else
                        foreach (EDBParameter p in Parameters)
                        {
//                            if (((p.Direction == ParameterDirection.Output) ||
//                                (p.Direction == ParameterDirection.InputOutput)) && (i < nrs.RowDescription.NumFields ))
//                            {
                                p.Value = nar[i];
								
                                i++;
                            //}
                        }
                }
                
            }   
            
            
        }

        /// <summary>
        /// Sends the <see cref="EDB.EDBCommand.CommandText">CommandText</see> to
        /// the <see cref="EDB.EDBConnection">Connection</see> and builds a
        /// <see cref="EDB.EDBDataReader">EDBDataReader</see>.
        /// </summary>
        /// <returns>A <see cref="EDB.EDBDataReader">EDBDataReader</see> object.</returns>
        IDataReader IDbCommand.ExecuteReader()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbCommand.ExecuteReader");

            return (EDBDataReader) ExecuteReader();
        }

        /// <summary>
        /// Sends the <see cref="EDB.EDBCommand.CommandText">CommandText</see> to
        /// the <see cref="EDB.EDBConnection">Connection</see> and builds a
        /// <see cref="EDB.EDBDataReader">EDBDataReader</see>
        /// using one of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.
        /// </summary>
        /// <param name="cb">One of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.</param>
        /// <returns>A <see cref="EDB.EDBDataReader">EDBDataReader</see> object.</returns>
        IDataReader IDbCommand.ExecuteReader(CommandBehavior cb)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbCommand.ExecuteReader", cb);

            return (EDBDataReader) ExecuteReader(cb);
        }

        /// <summary>
        /// Sends the <see cref="EDB.EDBCommand.CommandText">CommandText</see> to
        /// the <see cref="EDB.EDBConnection">Connection</see> and builds a
        /// <see cref="EDB.EDBDataReader">EDBDataReader</see>.
        /// </summary>
        /// <returns>A <see cref="EDB.EDBDataReader">EDBDataReader</see> object.</returns>
        public EDBDataReader ExecuteReader()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteReader");
			
            return ExecuteReader(CommandBehavior.Default);

        }

        /// <summary>
        /// Sends the <see cref="EDB.EDBCommand.CommandText">CommandText</see> to
        /// the <see cref="EDB.EDBConnection">Connection</see> and builds a
        /// <see cref="EDB.EDBDataReader">EDBDataReader</see>
        /// using one of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.
        /// </summary>
        /// <param name="cb">One of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.</param>
        /// <returns>A <see cref="EDB.EDBDataReader">EDBDataReader</see> object.</returns>
        /// <remarks>Currently the CommandBehavior parameter is ignored.</remarks>
        public EDBDataReader ExecuteReader(CommandBehavior cb)
        {
            // [FIXME] No command behavior handling.

            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteReader", cb);
            commandBehavior = cb;
			if(parameters!=null)
				connector.Mediator.setParameters(parameters);
            ExecuteCommand();
            
          //  UpdateOutputParameters();//EDB Team: Patch of new npgsql release   
            
            // Get the resultsets and create a Datareader with them.
            return new EDBDataReader(Connector.Mediator.ResultSets, Connector.Mediator.CompletedResponses, cb, this);
        }

        ///<summary>
        /// This method binds the parameters from parameters collection to the bind
        /// message.
        /// </summary>
        private void BindParameters()
        {

            if (parameters.Count != 0)
            {
                Object[] parameterValues = new Object[parameters.Count];
				
				Int16[] parameterFormatCodes = new Int16[parameters.Count];
                
				for (Int32 i = 0; i < parameters.Count; i++)
                {
                    // Do not quote strings, or escape existing quotes - this will be handled by the backend.
                    // DBNull or null values are returned as null.
                    // TODO: Would it be better to remove this null special handling out of ConvertToBackend??
                    //parameterValues[i] = parameters[i].TypeInfo.ConvertToBackend(parameters[i].Value, true);
                
					if(parameters[i].TypeInfo.EDBDbType != EDBDbType.Bytea)
					{
						parameterFormatCodes[i] = (Int16)FormatCode.Text;
						parameterValues[i]= parameters[i].TypeInfo.ConvertToBackend(parameters[i].Value,true);
					
					}
					else
					{
						
					
						if(parameters[i].Value !=null && Parameters[i].Direction != ParameterDirection.Output)
						{
							parameterFormatCodes[i]=(Int16)FormatCode.Binary;
							parameterValues[i]=(byte[])parameters[i].Value;
						}
					}
				}
                bind.ParameterValues = parameterValues;
				bind.ParameterFormatCodes = parameterFormatCodes;
            }

            Connector.Bind(bind);
            Connector.Mediator.RequireReadyForQuery = false;
            Connector.Flush();

            connector.CheckErrorsAndNotifications();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row
        /// in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <returns>The first column of the first row in the result set,
        /// or a null reference if the result set is empty.</returns>
        public Object ExecuteScalar()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteScalar");

            /*if ((type == CommandType.Text) || (type == CommandType.StoredProcedure))
              if (parse == null)
            			connection.Query(this); 
               else
               {
                 BindParameters();
                 connection.Execute(new EDBExecute(bind.PortalName, 0));
               }
            else
            	throw new NotImplementedException(resman.GetString("Exception_CommandTypeTableDirect"));
            */
			if(parameters!=null)
				connector.Mediator.setParameters(parameters);
            ExecuteCommand();

            // Now get the results.
            // Only the first column of the first row must be returned.

            // Get ResultSets.
            ArrayList resultSets = Connector.Mediator.ResultSets;

            // First data is the RowDescription object.
            // Check all resultsets as insert commands could	ng
            // with resultset queries. The insert commands return null and and some queries
            // may return empty resultsets, so, if we find one of these, skip to next resultset.
            // If no resultset is found, return null as per specification.

            EDBAsciiRow ascii_row = null;
            foreach( EDBResultSet nrs in resultSets )
            {
                if( (nrs != null) && (nrs.Count > 0) )
                {
                    ascii_row = (EDBAsciiRow) nrs[0];
                    return ascii_row[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a prepared version of the command on a EntepriseDB server.
        /// </summary>
        public void Prepare()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Prepare");

            // Check the connection state.
            CheckConnectionState();

		  // reset any responses just before getting new ones
            Connector.Mediator.ResetResponses();
            
            // Set command timeout.
            connector.Mediator.CommandTimeout = CommandTimeout;
            if (!Connector.SupportsPrepare)
            {
                return;	// Do nothing.
            }

            if (connector.BackendProtocolVersion == ProtocolVersion.Version2)
            {
                EDBCommand command = new EDBCommand(GetPrepareCommandText(), connector );
                command.ExecuteNonQuery();
            }
            else
            {
				EDBCommand command = new EDBCommand();  //= new EDBCommand(GetPrepareCommandText(), connector );				
				command.CommandType = type;
				//System.Windows.Forms.MessageBox.Show(CommandType.GetType().ToString());
                // Use the extended query parsing...
                //planName = "EDBPlan" + Connector.NextPlanIndex();
                
				planName = Connector.NextPlanName();
                String portalName = Connector.NextPortalName();
                parse = new EDBParse(planName, GetParseCommandText(), new Int32[] {},parameters ,command);
					

		 //EnterpriseDB Team :fix for 2177.Sending sync before parse ,if Mediator had error in last statement execution.
				
				if(Connector.Mediator.Errors.Count > 0)
				{
					
					connector.Mediator.ResetExpectations();
					connector.Mediator.ResetResponses();
					connector.Sync();
				}
				

				Connector.Parse(parse ,command);
                Connector.Mediator.RequireReadyForQuery = false;
                Connector.Flush();

                // Check for errors and/or notifications and do the Right Thing.
					connector.CheckErrorsAndNotifications();


                    
				EDBRowDescription returnRowDesc = connector.Mediator.LastRowDescription;
                
				Int16[] resultFormatCodes;
                    
                    
				if (returnRowDesc != null)
				{
					resultFormatCodes = new Int16[returnRowDesc.NumFields];
                        
					for (int i=0; i < returnRowDesc.NumFields; i++)
					{
						EDBRowDescriptionFieldData returnRowDescData = returnRowDesc[i];
                            
                            
						if (returnRowDescData.type_info != null && returnRowDescData.type_info.EDBDbType == EDBDbType.Bytea)
						{
							// Binary format
							resultFormatCodes[i] = (Int16)FormatCode.Binary;
						}
						else 
							// Text Format
							resultFormatCodes[i] = (Int16)FormatCode.Text;
					}
                    
                        
				}
				else
					resultFormatCodes = new Int16[]{0};
                    





                bind = new EDBBind("", planName, new Int16[] {0}, null, new Int16[] {0});
            }
        }

        /*
        /// <summary>
        /// Releases the resources used by the <see cref="EDB.EDBCommand">EDBCommand</see>.
        /// </summary>
        protected override void Dispose (bool disposing)
        {
            
            if (disposing)
            {
                // Only if explicitly calling Close or dispose we still have access to 
                // managed resources.
                EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Dispose");
                if (connection != null)
                {
                    connection.Dispose();
                }
                base.Dispose(disposing);
                
            }
        }*/

        ///<summary>
        /// This method checks the connection state to see if the connection
        /// is set or it is open. If one of this conditions is not met, throws
        /// an InvalidOperationException
        ///</summary>
        private void CheckConnectionState()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CheckConnectionState");


            // Check the connection state.
            if (Connector == null || Connector.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(resman.GetString("Exception_ConnectionNotOpen"));
            }
        }

        /// <summary>
        /// This method substitutes the <see cref="EDB.EDBCommand.Parameters">Parameters</see>, if exist, in the command
        /// to their actual values.
        /// The parameter name format is <b>:ParameterName</b>.
        /// </summary>
        /// <returns>A version of <see cref="EDB.EDBCommand.CommandText">CommandText</see> with the <see cref="EDB.EDBCommand.Parameters">Parameters</see> inserted.</returns>
        internal String GetCommandText()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetCommandText");

            if (planName == String.Empty)
				return GetClearCommandText();
            else
                return GetPreparedCommandText();
        }


        private String GetClearCommandText()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetClearCommandText");

            Boolean addProcedureParenthesis = false;  // Do not add procedure parenthesis by default.
            
            Boolean functionReturnsRecord = false;    // Functions don't return record by default.
            
            String result = text;

            if (type == CommandType.StoredProcedure)
            {
                
                functionReturnsRecord = CheckFunctionReturnRecord();
                
                // Check if just procedure name was passed. If so, does not replace parameter names and just pass parameter values in order they were added in parameters collection.
                if (!result.Trim().EndsWith(")"))  
                {
                    addProcedureParenthesis = true;
                    result += "(";
                }
                
                if (Connector.SupportsPrepare)
                    result = "select * from " + result; // This syntax is only available in 7.3+ as well SupportsPrepare.
					
                else
                    result = "select " + result;        //Only a single result return supported. 7.2 and earlier.
            }
            else if (type == CommandType.TableDirect)
                return "select * from " + result;       // There is no parameter support on table direct.

            if (parameters == null || parameters.Count == 0)
            {
                if (addProcedureParenthesis)
                        result += ")";
                        
                if (functionReturnsRecord)
                    result = AddFunctionReturnsRecordSupport(result);
                
                
                result = AddSingleRowBehaviorSupport(result);
                                           
                return result;
            }   

             
            // Get parameters in query string to translate them to their actual values.
             
            // This regular expression gets all the parameters in format :param or @param 
            // and everythingelse.
            // This is only needed if query string has parameters. Else, just append the
            // parameter values in order they were put in parameter collection.
            
            
            // If parenthesis don't need to be added, they were added by user with parameter names. Replace them.
            if (!addProcedureParenthesis)
            {
            
                Regex a = new Regex(@"(:[\w]*)|(@[\w]*)|(.)");
                
                //CheckParameters();
    
                StringBuilder sb = new StringBuilder();
                
                for ( Match m = a.Match(result); m.Success; m = m.NextMatch() )
                {
                    String s = m.Groups[0].ToString();
                    
                    if (s.StartsWith(":") ||
                    s.StartsWith("@"))
                    {
                        // It's a parameter. Lets handle it.
                    
                        EDBParameter p = Parameters[s];
                        if ((p.Direction == ParameterDirection.Input) ||
                        (p.Direction == ParameterDirection.InputOutput))
		        {
                            
                            // FIXME DEBUG ONLY
                            // adding the '::<datatype>' on the end of a parameter is a highly
                            // questionable practice, but it is great for debugging!
                            sb.Append(p.TypeInfo.ConvertToBackend(p.Value, false));
		            // Only add data type info if value is not null.
				                                
                            if (p.Value != DBNull.Value)
                            {
                                sb.Append("::");
			        sb.Append(p.TypeInfo.Name);
			        if (p.TypeInfo.UseSize)
			            sb.Append("(").Append(p.Size).Append(")");
			    }
                        }   
                    }   
                    else 
                        sb.Append(s);
                    
                }
                
                result = sb.ToString();
            }
                
                
            else
            {
                
                
                for (Int32 i = 0; i < parameters.Count; i++)
                {
                    EDBParameter Param = parameters[i];
    
                    
                    if ((Param.Direction == ParameterDirection.Input) ||
                        (Param.Direction == ParameterDirection.InputOutput))
                    
                        
                            result += Param.TypeInfo.ConvertToBackend(Param.Value, false) + "::" + Param.TypeInfo.Name + ",";
                }
            
            
                // Remove a trailing comma added from parameter handling above. If any.
                // Maybe there are only output parameters. If so, there will be no comma.
                if (result.EndsWith(","))
                    result = result.Remove(result.Length - 1, 1);
                
                result += ")";
            }

            if (functionReturnsRecord)
                result = AddFunctionReturnsRecordSupport(result);
                
            return AddSingleRowBehaviorSupport(result);
        }
        
        
//EDB Team: Patch of new npgsql release           
        private Boolean CheckFunctionReturnRecord()
        {
        
            if (Parameters.Count == 0)
                return false;
                
            String returnRecordQuery = "select count(*) > 0 from pg_proc where prorettype = ( select oid from pg_type where typname = 'record' ) and proargtypes='{0}' and proname='{1}';";
            
            StringBuilder parameterTypes = new StringBuilder("");
            
            foreach(EDBParameter p in Parameters)
            {
                if ((p.Direction == ParameterDirection.Input) ||
                (p.Direction == ParameterDirection.InputOutput))
                {
                    parameterTypes.Append(Connection.Connector.OidToNameMapping[p.TypeInfo.Name].OID + " ");
                }
            }
        
                
            EDBCommand c = new EDBCommand(String.Format(returnRecordQuery, parameterTypes.ToString(), CommandText), Connection);
            
            Boolean ret = (Boolean) c.ExecuteScalar();
            
            // reset any responses just before getting new ones
            connector.Mediator.ResetResponses();
            return ret;
            
        
        }
        
        
        private String AddFunctionReturnsRecordSupport(String OriginalResult)
        {
                                
            StringBuilder sb = new StringBuilder(OriginalResult);
            
            sb.Append(" as (");
            
            foreach(EDBParameter p in Parameters)
            {
                if ((p.Direction == ParameterDirection.Output) ||
                (p.Direction == ParameterDirection.InputOutput))
                {
                    sb.Append(String.Format("{0} {1}, ", p.ParameterName.Substring(1), p.TypeInfo.Name));
                }
            }
            
            String result = sb.ToString();
            
            result = result.Remove(result.Length - 2, 1);
            
            result += ")";
            
            
            
            return result;
            
            
        }



        private String GetPreparedCommandText()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetPreparedCommandText");

            if (parameters.Count == 0)
                return "execute " + planName;


            StringBuilder result = new StringBuilder("execute " + planName + '(');


            for (Int32 i = 0; i < parameters.Count; i++)
            {
                result.Append(parameters[i].TypeInfo.ConvertToBackend(parameters[i].Value, false) + ',');
            }

            result = result.Remove(result.Length - 1, 1);
            result.Append(')');

            return result.ToString();

        }



        private String GetParseCommandText()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetParseCommandText");

            Boolean addProcedureParenthesis = false;  // Do not add procedure parenthesis by default.
            
            String parseCommand = text;

            if (type == CommandType.StoredProcedure)
            {
                // Check if just procedure name was passed. If so, does not replace parameter names and just pass parameter values in order they were added in parameters collection.
                if (!parseCommand.Trim().EndsWith(")"))  
                {
                    addProcedureParenthesis = true;
                    parseCommand += "(";
                }
                
            //    parseCommand = "select * from " + parseCommand; // This syntax is only available in 7.3+ as well SupportsPrepare.
					parseCommand = "call " + parseCommand; // This syntax i s only available in 7.3+ as well SupportsPrepare.
            }
            else if (type == CommandType.TableDirect)
                return "select * from " + parseCommand; // There is no parameter support on TableDirect.

            if (parameters.Count > 0)
            {
                // The ReplaceParameterValue below, also checks if the parameter is present.

                String parameterName;
                Int32 i;

                for (i = 0; i < parameters.Count; i++)
                {
//                    if ((parameters[i].Direction == ParameterDirection.Input) ||
//                    (parameters[i].Direction == ParameterDirection.InputOutput))
//                    {
//                    
//                        if (!addProcedureParenthesis)
//                        {
//                            //result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
                            parameterName = parameters[i].ParameterName;
                            //textCommand = textCommand.Replace(':' + parameterName, "$" + (i+1));
                            // For postgres parseCommand = ReplaceParameterValue(parseCommand, parameterName, "$" + (i+1) + "::" + parameters[i].TypeInfo.Name);
								parseCommand = ReplaceParameterValue(parseCommand, parameterName, "$" + (i+1));
//                        }	
//                        else
//                            parseCommand += "$" + (i+1) + "::" + parameters[i].TypeInfo.Name;
//                    }

                }
            }

            if (addProcedureParenthesis)
                return parseCommand + ")";
            else
                return parseCommand;

        }


        private String GetPrepareCommandText()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetPrepareCommandText");

            Boolean addProcedureParenthesis = false;  // Do not add procedure parenthesis by default.

            planName = Connector.NextPlanName();

            StringBuilder command = new StringBuilder("prepare " + planName);

            String textCommand = text;
			
            if (type == CommandType.StoredProcedure)
            {
                // Check if just procedure name was passed. If so, does not replace parameter names and just pass parameter values in order they were added in parameters collection.
                if (!textCommand.Trim().EndsWith(")"))  
                {
                    addProcedureParenthesis = true;
                    textCommand += "(";
                }
                
                textCommand = "select * from " + textCommand;
            }
            else if (type == CommandType.TableDirect)
                return "select * from " + textCommand; // There is no parameter support on TableDirect.


            if (parameters.Count > 0)
            {
                // The ReplaceParameterValue below, also checks if the parameter is present.

                String parameterName;
                Int32 i;

                for (i = 0; i < parameters.Count; i++)
                {
                    if ((parameters[i].Direction == ParameterDirection.Input) ||
                    (parameters[i].Direction == ParameterDirection.InputOutput))
                    {
                    
                        if (!addProcedureParenthesis)
                        {
                            //result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
                            parameterName = parameters[i].ParameterName;
                            // The space in front of '$' fixes a parsing problem in 7.3 server
                            // which gives errors of operator when finding the caracters '=$' in
                            // prepare text
                            textCommand = ReplaceParameterValue(textCommand, parameterName, " $" + (i+1));
                        }
                        else
                            textCommand += " $" + (i+1);
                    }

                }

                //[TODO] Check if there is any missing parameters in the query.
                // For while, an error is thrown saying about the ':' char.

                command.Append('(');

                for (i = 0; i < parameters.Count; i++)
                {
                    //                    command.Append(EDBTypesHelper.GetDefaultTypeInfo(parameters[i].DbType));
                    command.Append(parameters[i].TypeInfo.Name);

                    command.Append(',');
                }

                command = command.Remove(command.Length - 1, 1);
                command.Append(')');

            }
            
            if (addProcedureParenthesis)
                textCommand += ")";

            command.Append(" as ");
            command.Append(textCommand);


            return command.ToString();

        }


        private String ReplaceParameterValue(String result, String parameterName, String paramVal)
        {
            Int32 resLen = result.Length;
            Int32 paramStart = result.IndexOf(parameterName);
            Int32 paramLen = parameterName.Length;
            Int32 paramEnd = paramStart + paramLen;
            Boolean found = false;


            while(paramStart > -1)
            {
                if((resLen > paramEnd) && !Char.IsLetterOrDigit(result, paramEnd))
                {
                    result = result.Substring(0, paramStart) + paramVal + result.Substring(paramEnd);
                    found = true;
                }
                else if(resLen == paramEnd)
                {
                    result = result.Substring(0, paramStart)+ paramVal;
                    found = true;
                }
                else
                    break;
                resLen = result.Length;
                paramStart = result.IndexOf(parameterName, paramStart);
                paramEnd = paramStart + paramLen;

            }//while
            if(!found)
                throw new IndexOutOfRangeException (String.Format(resman.GetString("Exception_ParamNotInQuery"), parameterName));


            return result;
        }//ReplaceParameterValue
        
//EDB Team: Patch of new npgsql release           
        private String AddSingleRowBehaviorSupport(String ResultCommandText)
        {
            ResultCommandText = ResultCommandText.Trim();
			if ((commandBehavior & CommandBehavior.SingleRow) > 0)
            {
                if (ResultCommandText.EndsWith(";"))
                    ResultCommandText = ResultCommandText.Substring(0, ResultCommandText.Length - 1);
				if(!Char.IsNumber(ResultCommandText,ResultCommandText.Length -1 ))
					ResultCommandText += " limit 1;";
            }
            return ResultCommandText;
		}


        private void ExecuteCommand()
        {
            // Check the connection state first.
            CheckConnectionState();

            // reset any responses just before getting new ones
            connector.Mediator.ResetResponses();


            if (parse == null)
            {
                Connector.Query(this);

                // Check for errors and/or notifications and do the Right Thing.
                connector.CheckErrorsAndNotifications();
            }
            else
            {
                try
                {

                    BindParameters();

                    connector.Execute(new EDBExecute(bind.PortalName, 0));

                    // Check for errors and/or notifications and do the Right Thing.
                    connector.CheckErrorsAndNotifications();
                }
                finally
                {
                    // As per documentation:
                    // "[...] When an error is detected while processing any extended-query message,
                    // the backend issues ErrorResponse, then reads and discards messages until a
                    // Sync is reached, then issues ReadyForQuery and returns to normal message processing.[...]"
                    // So, send a sync command if we get any problems.

                    connector.Sync();
				}
				
            }
        }
        private void SetCommandTimeout()
        {
            if (Connector != null)
                timeout = Connector.CommandTimeout;
            else
                timeout = ConnectionStringDefaults.CommandTimeout;
        }

        private void ClearPoolAndThrowException(Exception e)
        {
            Connection.ClearPool();
            throw new EDBException(resman.GetString("Exception_ConnectionBroken"), e);

        }
        
         
        
    }
}
