using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;
using System.Diagnostics;
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

        [SetUp]
        public void Init()
        {

            //write setup for following test cases
            con = CreateDataSourceBuilder()
                .MapComposite<MyXML>("public.myxml")
                .Build()
                .OpenConnection();

            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE OR REPLACE TYPE myxml AS (value XML);"
            };

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
            var Command = new EDBCommand("", con)
            {
                CommandText = "EXEC DBMS_AQADM.DROP_QUEUE(queue_name => 'MSG_QUEUE'); "
            };
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
            var msg = Enqueue();
            var msgOut = Dequeue();

            Assert.AreEqual(msg.MessageId, msgOut.MessageId);
            Assert.IsInstanceOf(typeof(MyXML), msg.Payload);
            Assert.IsInstanceOf(typeof(MyXML), msgOut.Payload);

            var payload = (MyXML)msg.Payload;
            var payloadOut = (MyXML)msgOut.Payload;

            Assert.AreEqual(payload.value, payloadOut.value);
        }

        [Test]
        public void AQTest_EnsureDequeueEmptyThrows()
        {
            Assert.Throws<InvalidOperationException>(() => Dequeue(waitTimeSeconds: 1));
        }


        [Test]
        public void AQTest_EnsureMessageIdsUnique()
        {
            var msg1 = Enqueue();
            var msg2 = Enqueue();

            Assert.AreNotEqual(msg1.MessageId, msg2.MessageId);

            var msg1Out = Dequeue();
            var msg2Out = Dequeue();

            Assert.AreEqual(msg1.MessageId, msg1Out.MessageId);
            Assert.AreEqual(msg2.MessageId, msg2Out.MessageId);
        }

        private EDBAQMessage Enqueue()
        {
            var queMsg = new EDBAQMessage();

            using var queue = new EDBAQQueue("MSG_QUEUE", con);
            var txn = queue.Connection.BeginTransaction();

            try
            {
                queMsg.Payload = new MyXML { value = "(<Message><MessageText>Mahesh</MessageText></Message>)" };
                queue.EnqueueOptions.Visibility = EDBAQVisibility.ON_COMMIT;
                queue.MessageType = EDBAQMessageType.Udt;
                queue.UdtTypeName = "myxml";
                queue.Enqueue(queMsg);
                Assert.IsNotNull(queMsg.MessageId);
                txn.Commit();
                return queMsg;
            }
            catch (Exception)
            {
                txn?.Rollback();
                throw;
            }
        }

        private EDBAQMessage Dequeue(int waitTimeSeconds = 10)
        {
            EDBAQMessage deqMsg;

            using var queueListen = new EDBAQQueue("MSG_QUEUE", con);
            queueListen.MessageType = EDBAQMessageType.Udt;
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
                var v = queueListen.Listen(waitTimeSeconds);
                // If we are waiting for a message and we specify a Wait time,
                // then if there are no more messages, we want to just bounce out.
                if (waitTimeSeconds > -1 && v == null)
                {
                    throw new InvalidOperationException("Message was expected");
                }

                // once we're here that means a message has been detected in the queue. Let's deal with it.
                txn = queueListen.Connection.BeginTransaction();
                // dequeue the message
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
                    Assert.AreEqual("(<Message><MessageText>Mahesh</MessageText></Message>)", obj.value);

                    txn.Commit();

                    return deqMsg;
                }

                return null;
            }
            catch (Exception)
            {
                txn?.Rollback();
                txn?.Dispose();
                throw;
            }
        }
    }
}
#nullable restore
