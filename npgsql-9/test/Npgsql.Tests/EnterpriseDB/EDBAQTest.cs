using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;
#nullable disable
namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
    class MyXML
    {
        public string value { get; set; }
    }

    [TestFixture]
    [NonParallelizable]
    class EDBAQTest : EPASTestBase
    {
        EDBConnection con = null;
        EDBDataSourceBuilder dataSourceBuilder = null;

        [SetUp]
        public void Init()
        {

            dataSourceBuilder = CreateDataSourceBuilder();
            dataSourceBuilder.AddTypeInfoResolverFactory(new EDBAQResolverFactory());
            dataSourceBuilder.MapComposite<EDBAQEnqueueOptions>("dbms_aq.enqueue_options_t");
            dataSourceBuilder.MapComposite<EDBAQDequeueOptions>("dbms_aq.dequeue_options_t");
            //dataSourceBuilder.MapComposite<EDBAQMessageProperties>("dbms_aq.message_properties_t");
            dataSourceBuilder.MapComposite<MyXML>("public.myxml");

            //write setup for following test cases
            con = dataSourceBuilder.Build().OpenConnection();

            var Command = new EDBCommand("", con);


            Command.CommandText = "CREATE OR REPLACE TYPE myxml AS (value XML);";

            Console.WriteLine("CREATE type TYPE myxml: " + Command.ExecuteNonQuery());

            Command.CommandText = "EXEC DBMS_AQADM.CREATE_QUEUE_TABLE (queue_table => 'MSG_QUEUE_TABLE', queue_payload_type => 'myxml', comment => 'Message queue table'); END; ";

            Command.ExecuteNonQuery();

            Command.CommandText = "EXEC DBMS_AQADM.CREATE_QUEUE(queue_name => 'MSG_QUEUE', queue_table => 'MSG_QUEUE_TABLE', comment => 'This queue contains pending messages.'); ";

            Command.ExecuteNonQuery();

            Command.CommandText = "EXEC DBMS_AQADM.START_QUEUE (queue_name => 'MSG_QUEUE'); ";

            Command.ExecuteNonQuery();

            Command.CommandText = "commit; ";

            con.ReloadTypes();

            Command.ExecuteNonQuery();
        }

        [TearDown]
        public void Dispose()
        {
            var Command = new EDBCommand("", con);
            Command.CommandText = "EXEC DBMS_AQADM.DROP_QUEUE(queue_name => 'MSG_QUEUE'); ";
            Command.ExecuteNonQuery();
            Command.CommandText = "EXEC DBMS_AQADM.DROP_QUEUE_TABLE('MSG_QUEUE_TABLE', force => TRUE); ";
            Command.ExecuteNonQuery();
            Command.CommandText = "DROP TYPE myxml;";
            Command.ExecuteNonQuery();
            TestUtil.closeDB(con);
        }

        [Test]
        public void AQTest()
        {
            Enqueue();
            Dequeue();
        }

        private void Enqueue()
        {
            using (var queue = new EDBAQQueue("MSG_QUEUE", con))
            {
                queue.MessageType = EDBAQMessageType.Xml;
                EDBTransaction txn = queue.Connection.BeginTransaction();

                try
                {
                    var queMsg = new EDBAQMessage();
                    queMsg.Payload = new MyXML { value = "(<Message><MessageText>Mahesh</MessageText></Message>)" };
                    queue.EnqueueOptions.Visibility = EDBAQVisibility.ON_COMMIT;
                    queue.MessageType = EDBAQMessageType.Udt;
                    queue.UdtTypeName = "myxml";
                    queue.Enqueue(queMsg);
                    Assert.IsNotNull(queMsg.MessageId);
                    txn.Commit();
                    queMsg = null;
                }
                catch (Exception)
                {
                    txn?.Rollback();
                    throw;
                }
            }
        }

        private void Dequeue()
        {
            var waitTime = 10;
            using (var queueListen = new EDBAQQueue("MSG_QUEUE", con))
            {
                queueListen.UdtTypeName = "myxml";
                queueListen.DequeueOptions.Navigation = EDBAQNavigationMode.FIRST_MESSAGE;
                queueListen.DequeueOptions.Visibility = EDBAQVisibility.ON_COMMIT;
                queueListen.DequeueOptions.Wait = 1;

                EDBTransaction txn = null;

                if (queueListen.Connection.State == System.Data.ConnectionState.Closed)
                {
                    queueListen.Connection.Open();
                }
                try
                {
                    var v = queueListen.Listen(null, waitTime);
                    // If we are waiting for a message and we specify a Wait time,
                    // then if there are no more messages, we want to just bounce out.
                    if (waitTime > -1 && v == null)
                    {
                        throw new InvalidOperationException("Message was expected");
                    }

                    // once we're here that means a message has been detected in the queue. Let's deal with it.
                    txn = queueListen.Connection.BeginTransaction();
                    // dequeue the message
                    var deqMsg = new EDBAQMessage();
                    try
                    {
                        deqMsg = queueListen.Dequeue();
                    }
                    catch (Exception)
                    {
                        txn.Rollback();
                        throw;
                    }

                    if (deqMsg != null)
                    {
                        // process the message payload

                        // Direct cast
                        var obj = (MyXML)deqMsg.Payload;
                        Assert.AreEqual(obj.value, "(<Message><MessageText>Mahesh</MessageText></Message>)");

                        // via Map
                        var obj2 = new MyXML();
#pragma warning disable CS0618 // Type or member is obsolete
                        queueListen.Map<MyXML>(deqMsg.Payload, obj2);
#pragma warning restore CS0618 // Type or member is obsolete

                        Assert.AreEqual(obj2.value, "(<Message><MessageText>Mahesh</MessageText></Message>)");
                        txn.Commit();
                    }
                }
                catch (Exception)
                {
                    if (txn != null)
                    {
                        txn.Rollback();
                        if (txn != null)
                        {
                            txn.Dispose();
                        }
                    }
                    throw;
                }
            }
        }
    }
}
#nullable restore
