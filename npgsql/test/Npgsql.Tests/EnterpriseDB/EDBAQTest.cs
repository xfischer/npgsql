using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;
using System.Diagnostics;
using EnterpriseDB.EDBClient.TypeMapping;
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
            var dataSourceBuilder = CreateDataSourceBuilder();
            
            con = dataSourceBuilder
                .MapComposite<MyXML>("public.myxml")
                .Build()
                .OpenConnection();

            var command = new EDBCommand("", con);

            command.CommandText = "SELECT to_regtype('public.myxml') IS NOT NULL;";
            var typeExists = (bool?)command.ExecuteScalar();
            if (typeExists ?? false)
                return;

            command.CommandText = "CREATE OR REPLACE TYPE myxml AS (value XML);";
            command.ExecuteNonQuery();

            command.CommandText = "EXEC DBMS_AQADM.CREATE_QUEUE_TABLE (queue_table => 'MSG_QUEUE_TABLE', queue_payload_type => 'myxml', comment => 'Message queue table'); END; ";
            command.ExecuteNonQuery();

            command.CommandText = "EXEC DBMS_AQADM.CREATE_QUEUE(queue_name => 'MSG_QUEUE', queue_table => 'MSG_QUEUE_TABLE', comment => 'This queue contains pending messages.'); ";
            command.ExecuteNonQuery();

            command.CommandText = "EXEC DBMS_AQADM.START_QUEUE (queue_name => 'MSG_QUEUE'); ";
            command.ExecuteNonQuery();

            command.CommandText = "commit; ";

            con.ReloadTypes();

            command.ExecuteNonQuery();
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

            con.Dispose();
        }

        [Test]
        public void AQTest()
        {
            var msg = Enqueue();
            var msgOut = Dequeue();

            Assert.That(msgOut.MessageId,Is.EqualTo(msg.MessageId));
            Assert.That(msg.Payload, Is.InstanceOf(typeof(MyXML)));
            Assert.That(msgOut.Payload, Is.InstanceOf(typeof(MyXML)));

            var payload = (MyXML)msg.Payload;
            var payloadOut = (MyXML)msgOut.Payload;

            Assert.That(payloadOut.value, Is.EqualTo(payload.value));
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

            Assert.That(msg2.MessageId, Is.Not.EqualTo(msg1.MessageId));

            var msg1Out = Dequeue();
            var msg2Out = Dequeue();

            Assert.That(msg1Out.MessageId, Is.EqualTo(msg1.MessageId));
            Assert.That(msg2Out.MessageId, Is.EqualTo(msg2.MessageId));
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
                Assert.That(queMsg.MessageId, Is.Not.Null);
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
                    Assert.That(obj.value, Is.EqualTo("(<Message><MessageText>Mahesh</MessageText></Message>)"));

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
