using System.Data;
using System.Data.Common;
using JetBrains.Annotations;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents the method that handles the <see cref="EDBDataAdapter.RowUpdated">RowUpdated</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="EDBRowUpdatedEventArgs">EDBRowUpdatedEventArgs</see> that contains the event data.</param>
    public delegate void EDBRowUpdatedEventHandler(object sender, EDBRowUpdatedEventArgs e);

    /// <summary>
    /// Represents the method that handles the <see cref="EDBDataAdapter.RowUpdating">RowUpdating</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="EDBRowUpdatingEventArgs">EDBRowUpdatingEventArgs</see> that contains the event data.</param>
    public delegate void EDBRowUpdatingEventHandler(object sender, EDBRowUpdatingEventArgs e);

    /// <summary>
    /// This class represents an adapter from many commands: select, update, insert and delete to fill <see cref="System.Data.DataSet">Datasets.</see>
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public sealed class EDBDataAdapter : DbDataAdapter
    {
        /// <summary>
        /// Row updated event.
        /// </summary>
        [PublicAPI]
        public event EDBRowUpdatedEventHandler? RowUpdated;

        /// <summary>
        /// Row updating event.
        /// </summary>
        public event EDBRowUpdatingEventHandler? RowUpdating;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EDBDataAdapter() {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="selectCommand"></param>
        public EDBDataAdapter(EDBCommand selectCommand)
            => SelectCommand = selectCommand;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="selectCommandText"></param>
        /// <param name="selectConnection"></param>
        public EDBDataAdapter(string selectCommandText, EDBConnection selectConnection)
            : this(new EDBCommand(selectCommandText, selectConnection)) {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="selectCommandText"></param>
        /// <param name="selectConnectionString"></param>
        public EDBDataAdapter(string selectCommandText, string selectConnectionString)
            : this(selectCommandText, new EDBConnection(selectConnectionString)) {}

        /// <summary>
        /// Create row updated event.
        /// </summary>
        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command,
                                                                     System.Data.StatementType statementType,
                                                                     DataTableMapping tableMapping)
            => new EDBRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);

        /// <summary>
        /// Create row updating event.
        /// </summary>
        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command,
                                                                       System.Data.StatementType statementType,
                                                                       DataTableMapping tableMapping)
            => new EDBRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);

        /// <summary>
        /// Raise the RowUpdated event.
        /// </summary>
        /// <param name="value"></param>
        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            //base.OnRowUpdated(value);
            if (value is EDBRowUpdatedEventArgs args)
                RowUpdated?.Invoke(this, args);
            //if (RowUpdated != null && value is EDBRowUpdatedEventArgs args)
            //    RowUpdated(this, args);
        }

        /// <summary>
        /// Raise the RowUpdating event.
        /// </summary>
        /// <param name="value"></param>
        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            if (value is EDBRowUpdatingEventArgs args)
                RowUpdating?.Invoke(this, args);
        }

        /// <summary>
        /// Delete command.
        /// </summary>
        public new EDBCommand DeleteCommand
        {
            get => (EDBCommand)base.DeleteCommand;
            set => base.DeleteCommand = value;
        }

        /// <summary>
        /// Select command.
        /// </summary>
        public new EDBCommand SelectCommand
        {
            get => (EDBCommand)base.SelectCommand;
            set => base.SelectCommand = value;
        }

        /// <summary>
        /// Update command.
        /// </summary>
        public new EDBCommand UpdateCommand
        {
            get => (EDBCommand)base.UpdateCommand;
            set => base.UpdateCommand = value;
        }

        /// <summary>
        /// Insert command.
        /// </summary>
        public new EDBCommand InsertCommand
        {
            get => (EDBCommand)base.InsertCommand;
            set => base.InsertCommand = value;
        }
    }

#pragma warning disable 1591

    public class EDBRowUpdatingEventArgs : RowUpdatingEventArgs
    {
        public EDBRowUpdatingEventArgs(DataRow dataRow, IDbCommand command, System.Data.StatementType statementType,
                                          DataTableMapping tableMapping)
            : base(dataRow, command, statementType, tableMapping) {}
    }

    public class EDBRowUpdatedEventArgs : RowUpdatedEventArgs
    {
        public EDBRowUpdatedEventArgs(DataRow dataRow, IDbCommand command, System.Data.StatementType statementType,
                                         DataTableMapping tableMapping)
            : base(dataRow, command, statementType, tableMapping) {}
    }

#pragma warning restore 1591
}
