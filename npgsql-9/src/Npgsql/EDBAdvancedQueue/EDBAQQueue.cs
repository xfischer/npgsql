using System;
using System.Data;

namespace EnterpriseDB.EDBClient;

/// <summary>
/// Represents a SQL statement  to execute
/// DMBS_AQ functionality on a PostgreSQL database.
/// </summary>
/// <remarks>
/// Constructor.
/// </remarks>
public class EDBAQQueue(string name, EDBConnection con) : IDisposable, ICloneable
{
    private bool disposedValue;

    /// <summary>
    /// Connection to be used for AQ operations
    /// </summary>
    /// <value>The connection to be used.</value>
    public EDBConnection Connection { get; set; } = con;

    /// <summary>
    /// Name of the queue
    /// </summary>
    /// <value>.</value>
    public string Name { get; set; } = name;

    /// <summary>
    /// MessageType of the message.
    /// </summary>
    /// <value>The name of the queue.</value>
    [System.ComponentModel.DefaultValue(EDBAQMessageType.Udt)]
    public EDBAQMessageType MessageType { get; set; } = EDBAQMessageType.Udt;

    /// <summary>
    /// Name of the user defined type.
    /// </summary>
    /// <value>The message type that is enqueued/dequeued from this queue. For example EDBAQMessageType.Udt.</value>
    public string? UdtTypeName { get; set; }

    /// <summary>
    /// EDBAQEnqueueOptions to be used.
    /// </summary>
    /// <value>The enqueue options to be used.</value>
    public EDBAQEnqueueOptions EnqueueOptions { get; set; } = new EDBAQEnqueueOptions();
    /// <summary>
    /// EDBAQDequeueOptions to be used 
    /// </summary>
    /// <value>The dequeue options to be used.</value>
    public EDBAQDequeueOptions DequeueOptions { get; set; } = new EDBAQDequeueOptions() { DequeueMode = EDBAQDequeueMode.REMOVE };
    /// <summary>
    /// EDBAQMessageProperties to be used 
    /// </summary>
    /// <value>The message properties to be used.</value>
    public EDBAQMessageProperties MessageProperties { get; set; } = new EDBAQMessageProperties();

    /// <summary>
    /// Enques the provided message in queue.
    /// </summary>
    public void Enqueue(EDBAQMessage msg)
    {
        string dataTypeName;
        if (MessageType == EDBAQMessageType.Udt)
        {
            if (UdtTypeName == null || UdtTypeName.Length == 0)
            {
                throw new InvalidCastException("Type name must be set for user defined types");
            }
            dataTypeName = UdtTypeName;
        }
        else
        {
            throw new InvalidCastException("Message Type must be user defined type");
        }
        if (msg.Payload == null)
        {
            throw new InvalidCastException("Payload must be set");
        }

        if (Connection == null || Connection.State == ConnectionState.Closed)
        {
            throw new InvalidOperationException("Connection is not open");
        }
        try
        {
            var command = new EDBCommand("DBMS_AQ.ENQUEUE(:queue_name, :enqueue_options, :message_properties, :payload, :MsgId)", Connection);
            command.CommandType = CommandType.StoredProcedure;
            command.AllResultTypesAreUnknown = false;
            var nameParam = command.Parameters.Add(new EDBParameter("queue_name", EDBTypes.EDBDbType.Varchar, 10, "queue_name", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, null!));
            nameParam.Value = Name;
            command.Parameters.Add(new EDBParameter
            {
                ParameterName = "enqueue_options",
                IsNullable = true,
                DataTypeName = "dbms_aq.enqueue_options_t",
                Value = EnqueueOptions
            });
            var msgProps = command.Parameters.Add(new EDBParameter("message_properties", EDBTypes.EDBDbType.Unknown, 110, "message_properties", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, null!));
            msgProps.DataTypeName = "dbms_aq.message_properties_t";
            msgProps.Value = MessageProperties.ToTextParam();


            command.Parameters.Add(new EDBParameter
            {
                ParameterName = "payload",
                IsNullable = true,
                DataTypeName = dataTypeName,
                Value = msg.Payload
            });

            var msgIdParam = command.Parameters.Add(new EDBParameter("MsgId", EDBTypes.EDBDbType.Bytea, 10, "MsgId", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));
            command.Prepare();
            command.ExecuteNonQuery();
            msg.MessageId = Convert.ToBase64String((byte[])msgIdParam.Value!);
        }
        catch (Exception ex)
        {
            throw new EDBException("Error while enqueuing message: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Dequeues the message from provided queue.
    /// </summary>
    public EDBAQMessage Dequeue()
    {
        try
        {
            string dataTypeName;
            if (MessageType == EDBAQMessageType.Udt)
            {
                if (UdtTypeName == null || UdtTypeName.Length == 0)
                {
                    throw new InvalidCastException("Type name must be set for user defined types");
                }
                dataTypeName = UdtTypeName;
            }
            else
            {
                throw new InvalidCastException("Message Type must be user defined type");
            }

            if (Connection == null || Connection.State == ConnectionState.Closed)
            {
                throw new InvalidOperationException("Connection is not open");
            }
            var msg = new EDBAQMessage();

            var command = new EDBCommand("DBMS_AQ.DEQUEUE(:queue_name, :dequeue_options, :message_properties, :payload, :MsgId)", Connection);
            command.CommandType = CommandType.StoredProcedure;
            command.AllResultTypesAreUnknown = false;
            command.Parameters.AddWithValue("queue_name", Name);
            command.Parameters.Add(new EDBParameter
            {
                ParameterName = "dequeue_options",
                IsNullable = true,
                DataTypeName = "dbms_aq.dequeue_options_t",
                Value = DequeueOptions
            });
            var messageProps = command.Parameters.Add(new EDBParameter("message_properties", EDBTypes.EDBDbType.Unknown, 110, "message_properties", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));
            messageProps.DataTypeName = "dbms_aq.message_properties_t";
            command.Parameters.Add(new EDBParameter
            {
                ParameterName = "payload",
                Direction = ParameterDirection.Output,
                DataTypeName = dataTypeName
            });
            var msgIdParam = command.Parameters.Add(new EDBParameter("MsgId", EDBTypes.EDBDbType.Bytea, 10, "MsgId", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));
            command.Prepare();
            command.ExecuteNonQuery();
            msg.MessageId = Convert.ToBase64String((byte[])msgIdParam.Value!);
            msg.Payload = command.Parameters[3].Value;
            MessageProperties = MessageProperties.ToObjectParam(messageProps.Value!.ToString()!);
            return msg;
        }
        catch (Exception ex)
        {
            throw new EDBException("Error while dequeuing message: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Listens to the queue for available messages.
    /// </summary>
    /// <param name="waitTime"></param>
    /// <returns></returns>
    public string? Listen(int waitTime)
    {
        var dqMode = DequeueOptions.DequeueMode;
        var wait = DequeueOptions.Wait;

        try
        {
            DequeueOptions.Wait = waitTime;
            DequeueOptions.DequeueMode = EDBAQDequeueMode.BROWSE;
            var msg = Dequeue();

            return msg.MessageId;
        }
        catch (EDBException ex) when (ex.InnerException is PostgresException pgException)
        {
            if (pgException.SqlState == "P0002")
            {
                return null;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            DequeueOptions.DequeueMode = dqMode;
            DequeueOptions.Wait = wait;
        }
    }
    
    object ICloneable.Clone() => Clone();

    /// <summary>
    /// Create a new EDBAQQueue based on this one.
    /// </summary>
    /// <returns></returns>
    public EDBAQQueue Clone()
    {
        var clone = new EDBAQQueue(Name, Connection);
        return clone;
    }

    /// <summary>
    /// Disposes the queue.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
        }
    }

    /// <summary>
    /// Disposes the queue.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
