// created on 1/8/2002 at 23:02
//
// Npgsql.NpgsqlDataAdapter.cs
//
// Author:
//  Francisco Jr. (fxjrlists@yahoo.com.br)
//
//  Copyright (C) 2002 The Npgsql Development Team
//  npgsql-general@gborg.postgresql.org
//  http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
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
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace EnterpriseDB.EDBClient
{
	/// <summary>
	/// Represents the method that handles the <see cref="Npgsql.NpgsqlDataAdapter.RowUpdated">RowUpdated</see> events.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="EDBRowUpdatedEventArgs">EDBRowUpdatedEventArgs</see> that contains the event data.</param>
	public delegate void NpgsqlRowUpdatedEventHandler(Object sender, EDBRowUpdatedEventArgs e);

	/// <summary>
	/// Represents the method that handles the <see cref="Npgsql.NpgsqlDataAdapter.RowUpdating">RowUpdating</see> events.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="EDBRowUpdatingEventArgs">EDBRowUpdatingEventArgs</see> that contains the event data.</param>
	public delegate void NpgsqlRowUpdatingEventHandler(Object sender, EDBRowUpdatingEventArgs e);


	/// <summary>
	/// This class represents an adapter from many commands: select, update, insert and delete to fill <see cref="System.Data.DataSet">Datasets.</see>
	/// </summary>
	public sealed class EDBDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		private EDBCommand _selectCommand;
		private EDBCommand _updateCommand;
		private EDBCommand _deleteCommand;
		private EDBCommand _insertCommand;

		// Log support
        private static readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;


		public event NpgsqlRowUpdatedEventHandler RowUpdated;
		public event NpgsqlRowUpdatingEventHandler RowUpdating;

		public EDBDataAdapter()
		{
		}

		public EDBDataAdapter(EDBCommand selectCommand)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
			_selectCommand = selectCommand;
		}

		public EDBDataAdapter(String selectCommandText, EDBConnection selectConnection)
			: this(new EDBCommand(selectCommandText, selectConnection))
		{
		}

		public EDBDataAdapter(String selectCommandText, String selectConnectionString)
			: this(selectCommandText, new EDBConnection(selectConnectionString))
		{
		}


		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command,
		                                                             StatementType statementType,
		                                                             DataTableMapping tableMapping)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateRowUpdatedEvent");
			return new EDBRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
		}

		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command,
		                                                               StatementType statementType,
		                                                               DataTableMapping tableMapping)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateRowUpdatingEvent");
			return new EDBRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
		}

		protected override void OnRowUpdated(RowUpdatedEventArgs value)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "OnRowUpdated");
			//base.OnRowUpdated(value);
			if ((RowUpdated != null) && (value is EDBRowUpdatedEventArgs))
			{
				RowUpdated(this, (EDBRowUpdatedEventArgs) value);
			}
		}

		protected override void OnRowUpdating(RowUpdatingEventArgs value)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "OnRowUpdating");
			if ((RowUpdating != null) && (value is EDBRowUpdatingEventArgs))
			{
				RowUpdating(this, (EDBRowUpdatingEventArgs) value);
			}
		}

		ITableMappingCollection IDataAdapter.TableMappings
		{
			get { return TableMappings; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IDbDataAdapter.DeleteCommand");
				return DeleteCommand;
			}

			set { DeleteCommand = (EDBCommand) value; }
		}


		public new EDBCommand DeleteCommand
		{
			get { return _deleteCommand; }

			set { _deleteCommand = value; }
		}

		IDbCommand IDbDataAdapter.SelectCommand
		{
			get { return SelectCommand; }

			set { SelectCommand = (EDBCommand) value; }
		}


		public new EDBCommand SelectCommand
		{
			get { return _selectCommand; }

			set { _selectCommand = value; }
		}

		IDbCommand IDbDataAdapter.UpdateCommand
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IDbDataAdapter.UpdateCommand");
				return UpdateCommand;
			}

			set { UpdateCommand = (EDBCommand) value; }
		}


		public new EDBCommand UpdateCommand
		{
			get { return _updateCommand; }

			set { _updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.InsertCommand
		{
			get { return InsertCommand; }

			set { InsertCommand = (EDBCommand) value; }
		}


		public new EDBCommand InsertCommand
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "InsertCommand");
				return _insertCommand;
			}

			set { _insertCommand = value; }
		}
	}
}

public class EDBRowUpdatingEventArgs : RowUpdatingEventArgs
{
	public EDBRowUpdatingEventArgs(DataRow dataRow, IDbCommand command, StatementType statementType,
	                                  DataTableMapping tableMapping)
		: base(dataRow, command, statementType, tableMapping)

	{
	}
}

public class EDBRowUpdatedEventArgs : RowUpdatedEventArgs
{
	public EDBRowUpdatedEventArgs(DataRow dataRow, IDbCommand command, StatementType statementType,
	                                 DataTableMapping tableMapping)
		: base(dataRow, command, statementType, tableMapping)

	{
	}
}