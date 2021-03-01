using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;
#nullable disable
namespace EnterpriseDB.EDBClient.Tests
{
    class MyXML
    {
        public string value { get; set; }
    }

    [TestFixture]
    class EDBAQTest : TestBase
    {
        EDBConnection con = null;

        [SetUp]
        public void Init()
        {
            //write setup for following test cases
            con = OpenConnection();
            EDBCommand Command = new EDBCommand("", con);


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
            EDBCommand Command = new EDBCommand("", con);
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
            using (EDBAQQueue queue = new EDBAQQueue("MSG_QUEUE", con))
            {
                queue.MessageType = EDBAQMessageType.Xml;
                EDBTransaction txn = queue.Connection.BeginTransaction();

                try
                {
                    EDBAQMessage queMsg = new EDBAQMessage();
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
            int waitTime = 10;
            using (EDBAQQueue queueListen = new EDBAQQueue("MSG_QUEUE", con))
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
                    string v = queueListen.Listen(null, waitTime);
                    // If we are waiting for a message and we specify a Wait time,
                    // then if there are no more messages, we want to just bounce out.
                    if (waitTime > -1 && v == null)
                    {
                        throw new InvalidOperationException("Message was expected");
                    }

                    // once we're here that means a message has been detected in the queue. Let's deal with it.
                    txn = queueListen.Connection.BeginTransaction();
                    // dequeue the message
                    EDBAQMessage deqMsg;
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
                        MyXML obj = new MyXML();
                        queueListen.Map<MyXML>(deqMsg.Payload, obj);
                        Assert.AreEqual(obj.value, "(<Message><MessageText>Mahesh</MessageText></Message>)");
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
                }
            }
        }
    }
}
#nullable restore
