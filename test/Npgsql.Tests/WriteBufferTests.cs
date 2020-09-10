using System.IO;
using EnterpriseDB.EDBClient.Util;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests
{
    class WriteBufferTests
    {
        [Test, IssueLink("https://github.com/EDB/EDB/issues/1275")]
        public void WriteZeroChars()
        {
            // Fill up the buffer entirely
            WriteBuffer.WriteBytes(new byte[WriteBuffer.Size], 0, WriteBuffer.Size);
            Assert.That(WriteBuffer.WriteSpaceLeft, Is.Zero);

            int charsUsed;
            bool completed;
            WriteBuffer.WriteStringChunked("hello", 0, 5, true, out charsUsed, out completed);
            Assert.That(charsUsed, Is.Zero);
            Assert.That(completed, Is.False);
            WriteBuffer.WriteStringChunked("hello".ToCharArray(), 0, 5, true, out charsUsed, out completed);
            Assert.That(charsUsed, Is.Zero);
            Assert.That(completed, Is.False);
        }

#pragma warning disable CS8625
        [SetUp]
        public void SetUp()
        {
            Underlying = new MemoryStream();
            WriteBuffer = new EDBWriteBuffer(null, Underlying, EDBReadBuffer.DefaultSize, PGUtil.UTF8Encoding);
        }
#pragma warning restore CS8625

        // ReSharper disable once InconsistentNaming
        EDBWriteBuffer WriteBuffer = default!;
        // ReSharper disable once InconsistentNaming
        MemoryStream Underlying = default!;
    }
}
