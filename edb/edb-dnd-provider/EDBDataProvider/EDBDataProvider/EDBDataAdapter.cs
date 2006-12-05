// created on 1/8/2002 at 23:02
//
// EDB.EDBDataAdapter.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The EDB Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
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
using System.Data.Common;
using System.Resources;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents the method that handles the <see cref="EDB.EDBDataAdapter.RowUpdated">RowUpdated</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="EDB.EDBRowUpdatedEventArgs">EDBRowUpdatedEventArgs</see> that contains the event data.</param>
    public delegate void EDBRowUpdatedEventHandler(Object sender, EDBRowUpdatedEventArgs e);

    /// <summary>
    /// Represents the method that handles the <see cref="EDB.EDBDataAdapter.RowUpdating">RowUpdating</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="EDB.EDBRowUpdatingEventArgs">EDBRowUpdatingEventArgs</see> that contains the event data.</param>
    public delegate void EDBRowUpdatingEventHandler(Object sender, EDBRowUpdatingEventArgs e);


    public sealed class EDBDataAdapter : DbDataAdapter, IDbDataAdapter
    {

        private EDBCommand 	_selectCommand;
        private EDBCommand		_updateCommand;
        private EDBCommand		_deleteCommand;
        private EDBCommand		_insertCommand;

        private EDBCommandBuilder cmd_builder;

        // Log support
        private static readonly String CLASSNAME = "EDBDataAdapter";


        public event EDBRowUpdatedEventHandler RowUpdated;
        public event EDBRowUpdatingEventHandler RowUpdating;

        public EDBDataAdapter()
        {}

        public EDBDataAdapter(EDBCommand selectCommand)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
            _selectCommand = selectCommand;
            cmd_builder = new EDBCommandBuilder(this);
        }

        public EDBDataAdapter(String selectCommandText, EDBConnection selectConnection)
        {
			/* 
			 * Open connection in DataAdapter class explicitly if connection is not open already 
			*/
			if(selectConnection != null)
				if (selectConnection.State!= ConnectionState.Open)
					selectConnection.Open();  
			_selectCommand = (new EDBCommand(selectCommandText, selectConnection));
			
		}
		


        public EDBDataAdapter(String selectCommandText, String selectConnectionString) : this(selectCommandText, new EDBConnection(selectConnectionString))
        {}


        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(
            DataRow dataRow,
            IDbCommand command,
            StatementType statementType,
            DataTableMapping tableMapping
        )
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateRowUpdatedEvent");
            return new EDBRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);



        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(
            DataRow dataRow,
            IDbCommand command,
            StatementType statementType,
            DataTableMapping tableMapping
        )
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateRowUpdatingEvent");
            return new EDBRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void OnRowUpdated(
            RowUpdatedEventArgs value
        )
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "OnRowUpdated");
            //base.OnRowUpdated(value);
            if ((RowUpdated != null) && (value is EDBRowUpdatedEventArgs))
                RowUpdated(this, (EDBRowUpdatedEventArgs) value);

        }

        protected override void OnRowUpdating(
            RowUpdatingEventArgs value
        )
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "OnRowUpdating");
            if ((RowUpdating != null) && (value is EDBRowUpdatingEventArgs))
                RowUpdating(this, (EDBRowUpdatingEventArgs) value);
    }

        ITableMappingCollection IDataAdapter.TableMappings
        {
            get
            {
                return TableMappings;
            }
        }

        IDbCommand IDbDataAdapter.DeleteCommand
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IDbDataAdapter.DeleteCommand");
                return (EDBCommand) DeleteCommand;
            }

            set
            {
                DeleteCommand = (EDBCommand) value;
            }
        }


        public EDBCommand DeleteCommand
        {
            get
            {
                return _deleteCommand;
            }

            set
            {
                _deleteCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.SelectCommand
        {
            get
            {
                return (EDBCommand) SelectCommand;
            }

            set
            {
                SelectCommand = (EDBCommand) value;
            }
        }


        public EDBCommand SelectCommand
        {
            get
            {
                return _selectCommand;
            }

            set
            {
                _selectCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.UpdateCommand
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IDbDataAdapter.UpdateCommand");
                return (EDBCommand) UpdateCommand;
            }

            set
            {
                UpdateCommand = (EDBCommand) value;
            }
        }


        public EDBCommand UpdateCommand
        {
            get
            {
                return _updateCommand;
            }

            set
            {
                _updateCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.InsertCommand
        {
            get
            {
                return (EDBCommand) InsertCommand;
            }

            set
            {
                InsertCommand = (EDBCommand) value;
            }
        }


        public EDBCommand InsertCommand
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "InsertCommand");
                return _insertCommand;
            }

            set
            {
                _insertCommand = value;
            }
        }


    }
}


public class EDBRowUpdatingEventArgs : RowUpdatingEventArgs
{
    public EDBRowUpdatingEventArgs (
        DataRow dataRow,
        IDbCommand command,
        StatementType statementType,
        DataTableMapping tableMapping
    ) : base(dataRow, command, statementType, tableMapping)

    {}

}

public class EDBRowUpdatedEventArgs : RowUpdatedEventArgs
{
    public EDBRowUpdatedEventArgs (
        DataRow dataRow,
        IDbCommand command,
        StatementType statementType,
        DataTableMapping tableMapping
    ) : base(dataRow, command, statementType, tableMapping)

    {}

}
