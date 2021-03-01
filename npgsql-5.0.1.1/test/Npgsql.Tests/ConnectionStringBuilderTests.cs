using System;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests
{
    class ConnectionStringBuilderTests
    {
        [Test]
        public void Basic()
        {
            var builder = new EDBConnectionStringBuilder();
            Assert.That(builder.Count, Is.EqualTo(0));
            Assert.That(builder.ContainsKey("server"), Is.True);
            builder.Host = "myhost";
            Assert.That(builder["host"], Is.EqualTo("myhost"));
            Assert.That(builder.Count, Is.EqualTo(1));
            Assert.That(builder.ConnectionString, Is.EqualTo("Host=myhost"));
            builder.Remove("HOST");
            Assert.That(builder["host"], Is.EqualTo(""));
            Assert.That(builder.Count, Is.EqualTo(0));
        }

        [Test]
        public void FromString()
        {
            var builder = new EDBConnectionStringBuilder();
            builder.ConnectionString = "Host=myhost;EF Template Database=foo";
            Assert.That(builder.Host, Is.EqualTo("myhost"));
            Assert.That(builder.EntityTemplateDatabase, Is.EqualTo("foo"));
        }

        [Test]
        public void TryGetValue()
        {
            var builder = new EDBConnectionStringBuilder();
            builder.ConnectionString = "Host=myhost";
            Assert.That(builder.TryGetValue("Host", out var value), Is.True);
            Assert.That(value, Is.EqualTo("myhost"));
            Assert.That(builder.TryGetValue("SomethingUnknown", out value), Is.False);
        }

        [Test]
        public void Remove()
        {
            var builder = new EDBConnectionStringBuilder();
            builder.SslMode = SslMode.Prefer;
            Assert.That(builder["SSL Mode"], Is.EqualTo(SslMode.Prefer));
            builder.Remove("SSL Mode");
            Assert.That(builder.ConnectionString, Is.EqualTo(""));
            builder.CommandTimeout = 120;
            Assert.That(builder["Command Timeout"], Is.EqualTo(120));
            builder.Remove("Command Timeout");
            Assert.That(builder.ConnectionString, Is.EqualTo(""));
        }

        [Test]
        public void Clear()
        {
            var builder = new EDBConnectionStringBuilder { Host = "myhost" };
            builder.Clear();
            Assert.That(builder.Count, Is.EqualTo(0));
            Assert.That(builder["host"], Is.EqualTo(""));
            Assert.That(builder.Host, Is.Null);
        }

        [Test]
        public void Default()
        {
            var builder = new EDBConnectionStringBuilder();
            Assert.That(builder.Port, Is.EqualTo(EDBConnection.DefaultPort));
            builder.Port = 8;
            builder.Remove("Port");
            Assert.That(builder.Port, Is.EqualTo(EDBConnection.DefaultPort));
        }

        [Test]
        public void Enum()
        {
            var builder = new EDBConnectionStringBuilder();
            builder.ConnectionString = "SslMode=Prefer";
            Assert.That(builder.SslMode, Is.EqualTo(SslMode.Prefer));
            Assert.That(builder.Count, Is.EqualTo(1));
        }

        [Test]
        public void Clone()
        {
            var builder = new EDBConnectionStringBuilder();
            builder.Host = "myhost";
            var builder2 = builder.Clone();
            Assert.That(builder2.Host, Is.EqualTo("myhost"));
            Assert.That(builder2["Host"], Is.EqualTo("myhost"));
            Assert.That(builder.Port, Is.EqualTo(EDBConnection.DefaultPort));
        }

        [Test]
        public void ConversionError()
        {
            var builder = new EDBConnectionStringBuilder();
            Assert.That(() => builder["Port"] = "hello",
                Throws.Exception.TypeOf<ArgumentException>().With.Message.Contains("Port"));
        }

        [Test]
        public void InvalidConnectionString()
        {
            var builder = new EDBConnectionStringBuilder();
            Assert.That(() => builder.ConnectionString = "Server=127.0.0.1;User Id=EDB_tests;Pooling:false",
                Throws.Exception.TypeOf<ArgumentException>());
        }
    }
}
