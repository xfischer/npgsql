// created on 1/8/2002 at 23:02
//
// EnterpriseDB.EDBClient.EDBDataAdapter.cs
//
// Author:
//  Francisco Jr. (fxjrlists@yahoo.com.br)
//
//  Copyright (C) 2002 The EnterpriseDB.EDBClient Development Team
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
    /// Represents the method that handles the <see cref="EnterpriseDB.EDBClient.EDBDataAdapter.RowUpdated">RowUpdated</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="EDBRowUpdatedEventArgs">EDBRowUpdatedEventArgs</see> that contains the event data.</param>
    public delegate void EDBRowUpdatedEventHandler(Object sender, EDBRowUpdatedEventArgs e);

    /// <summary>
    /// Represents the method that handles the <see cref="EnterpriseDB.EDBClient.EDBDataAdapter.RowUpdating">RowUpdating</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="EDBRowUpdatingEventArgs">EDBRowUpdatingEventArgs</see> that contains the event data.</param>
    public delegate void EDBRowUpdatingEventHandler(Object sender, EDBRowUpdatingEventArgs e);

    /// <summary>
    /// This class represents an adapter from many commands: select, update, insert and delete to fill <see cref="System.Data.DataSet">Datasets.</see>
    /// </summary>
    public sealed class EDBDataAdapter : DbDataAdapter
    {
        // Log support
        private static readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;

        /// <summary>
        /// Row updated event.
        /// </summary>
        public event EDBRowUpdatedEventHandler RowUpdated;

        /// <summary>
        /// Row updating event.
        /// </summary>
        public event EDBRowUpdatingEventHandler RowUpdating;

        /// <summary>
        /// Default constructor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EDBDataAdapter()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="selectCommand"></param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EDBDataAdapter(EDBCommand selectCommand)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
            SelectCommand = selectCommand;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="selectCommandText"></param>
        /// <param name="selectConnection"></param>
        public EDBDataAdapter(String selectCommandText, EDBConnection selectConnection)
            : this(new EDBCommand(selectCommandText, selectConnection))
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="selectCommandText"></param>
        /// <param name="selectConnectionString"></param>
        public EDBDataAdapter(String selectCommandText, String selectConnectionString)
            : this(selectCommandText, new EDBConnection(selectConnectionString))
        {
        }

        /// <summary>
        /// Create row updated event.
        /// </summary>
        /// <param name="dataRow"></param>
        /// <param name="command"></param>
        /// <param name="statementType"></param>
        /// <param name="tableMapping"></param>
        /// <returns></returns>
        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command,
                                                                     StatementType statementType,
                                                                     DataTableMapping tableMapping)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateRowUpdatedEvent");
            return new EDBRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        /// <summary>
        /// Create row updating event.
        /// </summary>
        /// <param name="dataRow"></param>
        /// <param name="command"></param>
        /// <param name="statementType"></param>
        /// <param name="tableMapping"></param>
        /// <returns></returns>
        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command,
                                                                       StatementType statementType,
                                                                       DataTableMapping tableMapping)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateRowUpdatingEvent");
            return new EDBRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        /// <summary>
        /// Raise the RowUpdated event.
        /// </summary>
        /// <param name="value"></param>
        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "OnRowUpdated");
            //base.OnRowUpdated(value);
            if ((RowUpdated != null) && (value is EDBRowUpdatedEventArgs))
            {
                RowUpdated(this, (EDBRowUpdatedEventArgs) value);
            }
        }

        /// <summary>
        /// Raise the RowUpdating event.
        /// </summary>
        /// <param name="value"></param>
        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "OnRowUpdating");
            if ((RowUpdating != null) && (value is EDBRowUpdatingEventArgs))
            {
                RowUpdating(this, (EDBRowUpdatingEventArgs) value);
            }
        }

        /// <summary>
        /// Delete command.
        /// </summary>
        public new EDBCommand DeleteCommand
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "NpgDataAdapter.DeleteCommand");
                return (EDBCommand)base.DeleteCommand;
            }

            set { base.DeleteCommand = value; }
        }

        /// <summary>
        /// Select command.
        /// </summary>
        public new EDBCommand SelectCommand
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "NpgDataAdapter.SelectCommand");
                return (EDBCommand)base.SelectCommand;
            }

            set { base.SelectCommand = value; }
        }

        /// <summary>
        /// Update command.
        /// </summary>
        public new EDBCommand UpdateCommand
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "NpgDataAdapter.UpdateCommand");
                return (EDBCommand)base.UpdateCommand;
            }

            set { base.UpdateCommand = value; }
        }

        /// <summary>
        /// Insert command.
        /// </summary>
        public new EDBCommand InsertCommand
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "NpgDataAdapter.InsertCommand");
                return (EDBCommand)base.InsertCommand;
            }

            set { base.InsertCommand = value; }
        }
    }
}

#pragma warning disable 1591

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

#pragma warning restore 1591
