using EnterpriseDB.EDBClient.Internal.Converters;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;


[TestFixture]
public class EDBBigIntergerTests
{
    [Test]
    public void NegativeBigInt_Test()
    {
        const int StackAllocByteThreshold = 64 * sizeof(uint);
        Span<short> destination = stackalloc short[StackAllocByteThreshold / sizeof(short)];

        var bigInt = new BigInteger(-1m);

        var bits = bigInt.ToByteArray().AsSpan();

        Assert.AreEqual(1, bits.Length);
        Assert.AreEqual(255, bits[0]);

        var b = new PgNumeric.Builder(bigInt, destination);
        var pgNumeric = b.Build();

        Assert.AreEqual(16384, pgNumeric.Sign);
        Assert.AreEqual(1, pgNumeric.Digits.Array![0]);
    }
}
