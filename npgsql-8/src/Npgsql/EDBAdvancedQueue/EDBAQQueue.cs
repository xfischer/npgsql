#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable
namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents a SQL statement  to execute
    /// DMBS_AQ functionality on a PostgreSQL database.
    /// </summary>
    public class EDBAQQueue : IDisposable, ICloneable
    {
        /// <summary>
        /// Connection to be used for AQ operations
        /// </summary>
        /// <value>The connection to be used.</value>
        public EDBConnection Connection { get; set; }
        /// <summary>
        /// Name of the queue
        /// </summary>
        /// <value>.</value>
        public string Name { get; set; }
        /// <summary>
        /// MessageType of the message.
        /// </summary>
        /// <value>The name of the queue.</value>
        [System.ComponentModel.DefaultValue(EDBAQMessageType.Udt)]
        public EDBAQMessageType MessageType { get; set; }
        /// <summary>
        /// Name of the user defined type.
        /// </summary>
        /// <value>The message type that is enqueued/dequeued from this queue. For example EDBAQMessageType.Udt.</value>
        public string UdtTypeName { get; set; }
        /// <summary>
        /// EDBAQEnqueueOptions to be used.
        /// </summary>
        /// <value>The enqueue options to be used.</value>
        public EDBAQEnqueueOptions EnqueueOptions { get; set; }
        /// <summary>
        /// EDBAQDequeueOptions to be used 
        /// </summary>
        /// <value>The dequeue options to be used.</value>
        public EDBAQDequeueOptions DequeueOptions { get; set; }
        /// <summary>
        /// EDBAQMessageProperties to be used 
        /// </summary>
        /// <value>The message properties to be used.</value>
        public EDBAQMessageProperties MessageProperties { get; set; }

        static EDBAQQueue()
        {
           // EDBConnection.GlobalTypeMapper.MapComposite<EDBAQEnqueueOptions>("dbms_aq.enqueue_options_t");
           // EDBConnection.GlobalTypeMapper.MapComposite<EDBAQDequeueOptions>("dbms_aq.dequeue_options_t");
           // EDBConnection.GlobalTypeMapper.MapComposite<EDBAQMessageProperties>("dbms_aq.message_properties_t");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public EDBAQQueue(string name, EDBConnection con)
        {
            Connection = con;
            Name = name;
            EnqueueOptions = new EDBAQEnqueueOptions(visibility: 0,
                                                     relative_msgid: null,
                                                     sequence_deviation: null,
                                                     transformation: null,
                                                     delivery_mode: 0);
            DequeueOptions = new EDBAQDequeueOptions(consumer_name: null,
                                                     dequeue_mode: EDBAQDequeueMode.REMOVE,
                                                     navigation: 0,
                                                     visibility: 0,
                                                     wait: 0,
                                                     msgid: null,
                                                     correlation: null,
                                                     deq_condition: null,
                                                     transformation: null,
                                                     delivery_mode: 0);
            MessageProperties = new EDBAQMessageProperties(priority: 1,
                                                           delay: 0,
                                                           expiration: null,
                                                           correlation: null,
                                                           attempts: null,
                                                           recipient_list: null,
                                                           exception_queue: null,
                                                           enqueue_time: null,
                                                           state: null,
                                                           original_msgid: null,
                                                           transaction_group: null,
                                                           delivery_mode: 0);
            MessageType = EDBAQMessageType.Udt;
        }

        /// <summary>
        /// Enques the provided message in queue.
        /// </summary>
        public void Enqueue(EDBAQMessage msg)
        {
            string dataTypeName = null;
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
                command.AllResultTypesAreUnknown = true;
                command.Parameters.Add(new EDBParameter("queue_name", EDBTypes.EDBDbType.Varchar, 10, "queue_name", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters[0].Value = Name;
                command.Parameters.Add(new EDBParameter
                {
                    ParameterName = "enqueue_options",
                    IsNullable = true,
                    DataTypeName = "dbms_aq.enqueue_options_t",
                    Value = EnqueueOptions
                });
                command.Parameters.Add(new EDBParameter("message_properties", EDBTypes.EDBDbType.Unknown, 110, "message_properties", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters[2].DataTypeName = "dbms_aq.message_properties_t";
                command.Parameters[2].Value = MessageProperties.ToTextParam();


                command.Parameters.Add(new EDBParameter
                {
                    ParameterName = "payload",
                    IsNullable = true,
                    DataTypeName = dataTypeName,
                    Value = msg.Payload
                });

                command.Parameters.Add(new EDBParameter("MsgId", EDBTypes.EDBDbType.Bytea, 10, "MsgId", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                var connector = Connection.Connector;//  CheckReadyAndGetConnector();
                command.Prepare();
                var reader = command.ExecuteNonQuery();
                msg.MessageId = Encoding.ASCII.GetBytes(command.Parameters[4].Value?.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Dequeues the message from provided queue.
        /// </summary>
        public EDBAQMessage Dequeue()
        {
            try
            {
                string dataTypeName = null;
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
                command.AllResultTypesAreUnknown = true;
                command.Parameters.Add(new EDBParameter("queue_name", EDBTypes.EDBDbType.Varchar, 10, "queue_name", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters[0].Value = Name;
                command.Parameters.Add(new EDBParameter
                {
                    ParameterName = "dequeue_options",
                    IsNullable = true,
                    DataTypeName = "dbms_aq.dequeue_options_t",
                    Value = DequeueOptions
                });
                command.Parameters.Add(new EDBParameter("message_properties", EDBTypes.EDBDbType.Unknown, 110, "message_properties", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters[2].DataTypeName = "dbms_aq.message_properties_t";
                command.Parameters.Add(new EDBParameter
                {
                    ParameterName = "payload",
                    Direction = ParameterDirection.Output,
                    DataTypeName = dataTypeName
                });
                command.Parameters.Add(new EDBParameter("MsgId", EDBTypes.EDBDbType.Bytea, 10, "MsgId", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Prepare();
                var reader = command.ExecuteNonQuery();
                msg.MessageId = (byte[])command.Parameters[4].Value;
                msg.Payload = command.Parameters[3].Value;
                MessageProperties = MessageProperties.ToObjectParam(command.Parameters[2].Value.ToString());
                return msg;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Listens to the queue for available messages.
        /// </summary>
        /// <param name="listenConsumers"></param>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        public string Listen(string[] listenConsumers, int waitTime)
        {
            if (listenConsumers != null)
            {
                throw new InvalidOperationException("Listen Consumers is not supported");
            }

            var dqMode = DequeueOptions.DequeueMode;
            var wait = DequeueOptions.Wait;

            try
            {
                DequeueOptions.Wait = waitTime;
                DequeueOptions.DequeueMode = EDBAQDequeueMode.BROWSE;
                EDBAQMessage msg = Dequeue();
                return ByteArrayToString((byte[])msg.MessageId);
            }
            catch (PostgresException e)
            {
                e.ToString();
                if (e.SqlState == "P0002")
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

        private string ByteArrayToString(byte[] byteArray)
        {
            // Sanity check if it's null so we don't incur overhead of an exception
            if (byteArray == null)
            {
                return string.Empty;
            }
            try
            {
                var hex = new StringBuilder(byteArray.Length * 2);
                foreach (var b in byteArray)
                {
                    hex.AppendFormat("{0:x2}", b);
                }

                return hex.ToString().ToUpper();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Maps payload to the user defined composite object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        [Obsolete("Advanced Queue messages payload are now strongly typed, no mapping is required.")]
        public void Map<T>(object source, T destination)
        {
            // If types are mapped, parameters are strongly typed, so direct cast is OK
            if (source is T)
            {
                destination = DeepCopyReflection<T>((T)source, ref destination);
                return;
            }

            // Old implementation not working, as there is no ExpandoObject instanciated on the caller, neither docs suggesting it
            if (!(source is System.Dynamic.ExpandoObject))
            {
                throw new InvalidOperationException("Invalid Object provided");
            }
            IDictionary<string, object> dict = (System.Dynamic.ExpandoObject)source;

            foreach (var prop in typeof(T).GetProperties())
            {
                var lower = prop.Name.ToLower();
                var key = dict.Keys.SingleOrDefault(k => k.ToLower() == lower);

                if (key != null)
                {
                    prop.SetValue(destination, dict[key], null);
                }
            }
        }

        private static T DeepCopyReflection<T>(T input, ref T clonedObj)
        {
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    object value = property.GetValue(input);
                    if (value != null && value.GetType().IsClass && !value.GetType().FullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
                    {
                        property.SetValue(clonedObj, DeepCopyReflection(value));
                    }
                    else
                    {
                        property.SetValue(clonedObj, value);
                    }
                }
            }
            return clonedObj;
        }

        private static T DeepCopyReflection<T>(T input)
        {
            var properties = typeof(T).GetProperties();
            T clonedObj = (T)Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    object value = property.GetValue(input);
                    if (value != null && value.GetType().IsClass && !value.GetType().FullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
                    {
                        property.SetValue(clonedObj, DeepCopyReflection(value));
                    }
                    else
                    {
                        property.SetValue(clonedObj, value);
                    }
                }
            }
            return clonedObj;
        }


        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (EnqueueOptions != null)
                    {
                        //EnqueueOptions = null;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
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
    }
}
#nullable restore
