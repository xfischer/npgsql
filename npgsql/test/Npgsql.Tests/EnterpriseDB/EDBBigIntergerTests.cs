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

        Assert.That(bits.Length, Is.EqualTo(1));
        Assert.That(bits[0], Is.EqualTo(255));

        var b = new PgNumeric.Builder(bigInt, destination);
        var pgNumeric = b.Build();

        Assert.That(pgNumeric.Sign, Is.EqualTo(16384));
        Assert.That(pgNumeric.Digits.Array![0], Is.EqualTo(1));
    }
}
