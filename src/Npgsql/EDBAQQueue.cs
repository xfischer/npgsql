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

        /// <summary>
        /// Constructor.
        /// </summary>
        public EDBAQQueue(string name, EDBConnection con)
        {
            this.Connection = con;
            this.Name = name;
            this.EnqueueOptions = new EDBAQEnqueueOptions(0, null, null, null, 0);
            this.DequeueOptions = new EDBAQDequeueOptions(null, EDBAQDequeueMode.REMOVE, 0, 0, 0, null, null, null, null, 0);
            this.MessageProperties = new EDBAQMessageProperties(1, 0, null, null, null, null, null, null, null, null, null, 0);
            this.MessageType = EDBAQMessageType.Udt;
        }

        /// <summary>
        /// Enques the provided message in queue.
        /// </summary>
        public void Enqueue(EDBAQMessage msg)
        {
            string dataTypeName = null;
            if (this.MessageType == EDBAQMessageType.Udt)
            {
                if (this.UdtTypeName == null || this.UdtTypeName.Length == 0)
                {
                    throw new InvalidCastException("Type name must be set for user defined types");
                }
                dataTypeName = this.UdtTypeName;
            } else
            {
                throw new InvalidCastException("Message Type must be user defined type");
            }
            if (msg.Payload == null)
            {
                throw new InvalidCastException("Payload must be set");
            }

            if (this.Connection == null || this.Connection.State == ConnectionState.Closed)
            {
                throw new InvalidOperationException("Connection is not open");
            }
            try
            {
                EDBCommand command = new EDBCommand("DBMS_AQ.ENQUEUE(:queue_name, :enqueue_options, :message_properties, :payload, :MsgId)", this.Connection);
                command.CommandType = CommandType.StoredProcedure;
                command.AllResultTypesAreUnknown = true;
                command.Parameters.Add(new EDBParameter("queue_name", EDBTypes.EDBDbType.Varchar, 10, "queue_name", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters[0].Value = this.Name;
                command.Parameters.Add(new EDBParameter
                {
                    ParameterName = "enqueue_options",
                    IsNullable = true,
                    DataTypeName = "dbms_aq.enqueue_options_t",
                    Value = this.EnqueueOptions
                });
                command.Parameters.Add(new EDBParameter("message_properties", EDBTypes.EDBDbType.Text, 110, "message_properties", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters[2].DataTypeName = "dbms_aq.message_properties_t";
                command.Parameters[2].Value = this.MessageProperties.ToTextParam();


                command.Parameters.Add(new EDBParameter
                {
                    ParameterName = "payload",
                    IsNullable = true,
                    DataTypeName = dataTypeName,
                    Value = msg.Payload
                });
                command.Parameters.Add(new EDBParameter("MsgId", EDBTypes.EDBDbType.Bytea, 10, "MsgId", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                var connector = Connection.CheckReadyAndGetConnector();//  CheckReadyAndGetConnector();
                //var ressult = connector._isCallableStmt;
                connector._AQcalled = true;
                command.Prepare();
                var reader = command.ExecuteNonQuery();
                msg.MessageId = (byte[])command.Parameters[4].Value;
            } catch (Exception e)
            {
                throw e;
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
                if (this.MessageType == EDBAQMessageType.Udt)
                {
                    if (this.UdtTypeName == null || this.UdtTypeName.Length == 0)
                    {
                        throw new InvalidCastException("Type name must be set for user defined types");
                    }
                    dataTypeName = this.UdtTypeName;
                }
                else
                {
                    throw new InvalidCastException("Message Type must be user defined type");
                }

                if (this.Connection == null || this.Connection.State == ConnectionState.Closed)
                {
                    throw new InvalidOperationException("Connection is not open");
                }
                EDBAQMessage msg = new EDBAQMessage();

                EDBCommand command = new EDBCommand("DBMS_AQ.DEQUEUE(:queue_name, :dequeue_options, :message_properties, :payload, :MsgId)", Connection);
                command.CommandType = CommandType.StoredProcedure;
                command.AllResultTypesAreUnknown = true;
                command.Parameters.Add(new EDBParameter("queue_name", EDBTypes.EDBDbType.Varchar, 10, "queue_name", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters[0].Value = this.Name;
                command.Parameters.Add(new EDBParameter
                {
                    ParameterName = "dequeue_options",
                    IsNullable = true,
                    DataTypeName = "dbms_aq.dequeue_options_t",
                    Value = this.DequeueOptions
                });
                command.Parameters.Add(new EDBParameter("message_properties", EDBTypes.EDBDbType.Text, 110, "message_properties", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
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
                this.MessageProperties = this.MessageProperties.ToObjectParam(command.Parameters[2].Value.ToString());
                return msg;
            } catch (Exception)
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

            var dqMode = this.DequeueOptions.DequeueMode;
            var wait = this.DequeueOptions.Wait;

            try
            {
                this.DequeueOptions.Wait = waitTime;
                this.DequeueOptions.DequeueMode = EDBAQDequeueMode.BROWSE;
                EDBAQMessage msg = Dequeue();
                return ByteArrayToString((byte[])msg.MessageId);
            } catch (PostgresException e)
            {
                e.ToString();
                if (e.SqlState == "P0002")
                {
                    return null;
                } else
                {
                    throw e;
                }
                
            }
            finally
            {
                this.DequeueOptions.DequeueMode = dqMode;
                this.DequeueOptions.Wait = wait;
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
                StringBuilder hex = new StringBuilder(byteArray.Length * 2);
                foreach (byte b in byteArray)
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
        public void Map<T>(object source, T destination)
        {
            if (!(source is System.Dynamic.ExpandoObject))
            {
                throw new InvalidOperationException("Invalid Object provided");
            }
            IDictionary<string, object> dict = (System.Dynamic.ExpandoObject) source;
            var type = destination.GetType();

            foreach (var prop in type.GetProperties())
            {
                var lower = prop.Name.ToLower();
                var key = dict.Keys.SingleOrDefault(k => k.ToLower() == lower);

                if (key != null)
                {
                    prop.SetValue(destination, dict[key], null);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (this.EnqueueOptions != null)
                    {
                        //this.EnqueueOptions = null;
                    }
                } catch (Exception e)
                {
                    throw e;
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
