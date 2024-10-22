using System;
using System.Net;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests;

/// <summary>
/// Tests EDBTypes.* independent of a database
/// </summary>
public class TypesTests
{
#pragma warning disable CS0618 // {EDBTsVector,EDBTsQuery}.Parse are obsolete
    [Test]
    public void TsVector()
    {
        EDBTsVector vec;

        vec = EDBTsVector.Parse("a");
        Assert.AreEqual("'a'", vec.ToString());

        vec = EDBTsVector.Parse("a ");
        Assert.AreEqual("'a'", vec.ToString());

        vec = EDBTsVector.Parse("a:1A");
        Assert.AreEqual("'a':1A", vec.ToString());

        vec = EDBTsVector.Parse(@"\abc\def:1a ");
        Assert.AreEqual("'abcdef':1A", vec.ToString());

        vec = EDBTsVector.Parse(@"abc:3A 'abc' abc:4B 'hello''yo' 'meh\'\\':5");
        Assert.AreEqual(@"'abc':3A,4B 'hello''yo' 'meh''\\':5", vec.ToString());

        vec = EDBTsVector.Parse(" a:12345C  a:24D a:25B b c d 1 2 a:25A,26B,27,28");
        Assert.AreEqual("'1' '2' 'a':24,25A,26B,27,28,12345C 'b' 'c' 'd'", vec.ToString());
    }

    [Test]
    public void TsQuery()
    {
        EDBTsQuery query;

        query = new EDBTsQueryLexeme("a", EDBTsQueryLexeme.Weight.A | EDBTsQueryLexeme.Weight.B);
        query = new EDBTsQueryOr(query, query);
        query = new EDBTsQueryOr(query, query);

        var str = query.ToString();

        query = EDBTsQuery.Parse("a & b | c");
        Assert.AreEqual("'a' & 'b' | 'c'", query.ToString());

        query = EDBTsQuery.Parse("'a''':*ab&d:d&!c");
        Assert.AreEqual("'a''':*AB & 'd':D & !'c'", query.ToString());

        query = EDBTsQuery.Parse("(a & !(c | d)) & (!!a&b) | c | d | e");
        Assert.AreEqual("( ( 'a' & !( 'c' | 'd' ) & !( !'a' ) & 'b' | 'c' ) | 'd' ) | 'e'", query.ToString());
        Assert.AreEqual(query.ToString(), EDBTsQuery.Parse(query.ToString()).ToString());

        query = EDBTsQuery.Parse("(((a:*)))");
        Assert.AreEqual("'a':*", query.ToString());

        query = EDBTsQuery.Parse(@"'a\\b''cde'");
        Assert.AreEqual(@"a\b'cde", ((EDBTsQueryLexeme)query).Text);
        Assert.AreEqual(@"'a\\b''cde'", query.ToString());

        query = EDBTsQuery.Parse(@"a <-> b");
        Assert.AreEqual("'a' <-> 'b'", query.ToString());

        query = EDBTsQuery.Parse("((a & b) <5> c) <-> !d <0> e");
        Assert.AreEqual("( ( 'a' & 'b' <5> 'c' ) <-> !'d' ) <0> 'e'", query.ToString());

        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("a b c & &"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("&"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("|"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("!"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("("));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse(")"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("()"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("<"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("<-"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("<->"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("a <->"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("<>"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("a <a> b"));
        Assert.Throws(typeof(FormatException), () => EDBTsQuery.Parse("a <-1> b"));
    }
#pragma warning restore CS0618 // {EDBTsVector,EDBTsQuery}.Parse are obsolete

    [Test]
    public void TsQueryEquatibility()
    {
        //Debugger.Launch();
        AreEqual(
            new EDBTsQueryLexeme("lexeme"),
            new EDBTsQueryLexeme("lexeme"));

        AreEqual(
            new EDBTsQueryLexeme("lexeme", EDBTsQueryLexeme.Weight.A | EDBTsQueryLexeme.Weight.B),
            new EDBTsQueryLexeme("lexeme", EDBTsQueryLexeme.Weight.A | EDBTsQueryLexeme.Weight.B));

        AreEqual(
            new EDBTsQueryLexeme("lexeme", EDBTsQueryLexeme.Weight.A | EDBTsQueryLexeme.Weight.B, true),
            new EDBTsQueryLexeme("lexeme", EDBTsQueryLexeme.Weight.A | EDBTsQueryLexeme.Weight.B, true));

        AreEqual(
            new EDBTsQueryNot(new EDBTsQueryLexeme("not")),
            new EDBTsQueryNot(new EDBTsQueryLexeme("not")));

        AreEqual(
            new EDBTsQueryAnd(new EDBTsQueryLexeme("left"), new EDBTsQueryLexeme("right")),
            new EDBTsQueryAnd(new EDBTsQueryLexeme("left"), new EDBTsQueryLexeme("right")));

        AreEqual(
            new EDBTsQueryOr(new EDBTsQueryLexeme("left"), new EDBTsQueryLexeme("right")),
            new EDBTsQueryOr(new EDBTsQueryLexeme("left"), new EDBTsQueryLexeme("right")));

        AreEqual(
            new EDBTsQueryFollowedBy(new EDBTsQueryLexeme("left"), 0, new EDBTsQueryLexeme("right")),
            new EDBTsQueryFollowedBy(new EDBTsQueryLexeme("left"), 0, new EDBTsQueryLexeme("right")));

        AreEqual(
            new EDBTsQueryFollowedBy(new EDBTsQueryLexeme("left"), 1, new EDBTsQueryLexeme("right")),
            new EDBTsQueryFollowedBy(new EDBTsQueryLexeme("left"), 1, new EDBTsQueryLexeme("right")));

        AreEqual(
            new EDBTsQueryEmpty(),
            new EDBTsQueryEmpty());

        AreNotEqual(
            new EDBTsQueryLexeme("lexeme a"),
            new EDBTsQueryLexeme("lexeme b"));

        AreNotEqual(
            new EDBTsQueryLexeme("lexeme", EDBTsQueryLexeme.Weight.A | EDBTsQueryLexeme.Weight.D),
            new EDBTsQueryLexeme("lexeme", EDBTsQueryLexeme.Weight.A | EDBTsQueryLexeme.Weight.B));

        AreNotEqual(
            new EDBTsQueryLexeme("lexeme", EDBTsQueryLexeme.Weight.A | EDBTsQueryLexeme.Weight.B, true),
            new EDBTsQueryLexeme("lexeme", EDBTsQueryLexeme.Weight.A | EDBTsQueryLexeme.Weight.B, false));

        AreNotEqual(
            new EDBTsQueryNot(new EDBTsQueryLexeme("not")),
            new EDBTsQueryNot(new EDBTsQueryLexeme("ton")));

        AreNotEqual(
            new EDBTsQueryAnd(new EDBTsQueryLexeme("right"), new EDBTsQueryLexeme("left")),
            new EDBTsQueryAnd(new EDBTsQueryLexeme("left"), new EDBTsQueryLexeme("right")));

        AreNotEqual(
            new EDBTsQueryOr(new EDBTsQueryLexeme("right"), new EDBTsQueryLexeme("left")),
            new EDBTsQueryOr(new EDBTsQueryLexeme("left"), new EDBTsQueryLexeme("right")));

        AreNotEqual(
            new EDBTsQueryFollowedBy(new EDBTsQueryLexeme("right"), 0, new EDBTsQueryLexeme("left")),
            new EDBTsQueryFollowedBy(new EDBTsQueryLexeme("left"), 0, new EDBTsQueryLexeme("right")));

        AreNotEqual(
            new EDBTsQueryFollowedBy(new EDBTsQueryLexeme("left"), 0, new EDBTsQueryLexeme("right")),
            new EDBTsQueryFollowedBy(new EDBTsQueryLexeme("left"), 1, new EDBTsQueryLexeme("right")));

        void AreEqual(EDBTsQuery left, EDBTsQuery right)
        {
            Assert.True(left == right);
            Assert.False(left != right);
            Assert.AreEqual(left, right);
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        void AreNotEqual(EDBTsQuery left, EDBTsQuery right)
        {
            Assert.False(left == right);
            Assert.True(left != right);
            Assert.AreNotEqual(left, right);
            Assert.AreNotEqual(left.GetHashCode(), right.GetHashCode());
        }
    }

#pragma warning disable CS0618 // {EDBTsVector,EDBTsQuery}.Parse are obsolete
    [Test]
    public void TsQueryOperatorPrecedence()
    {
        var query = EDBTsQuery.Parse("!a <-> b & c | d & e");
        var expectedGrouping = EDBTsQuery.Parse("((!(a) <-> b) & c) | (d & e)");
        Assert.AreEqual(expectedGrouping.ToString(), query.ToString());
    }
#pragma warning restore CS0618 // {EDBTsVector,EDBTsQuery}.Parse are obsolete

    [Test]
    public void EDBPath_empty()
        => Assert.That(new EDBPath { new(1, 2) }, Is.EqualTo(new EDBPath(new EDBPoint(1, 2))));

    [Test]
    public void EDBPolygon_empty()
        => Assert.That(new EDBPolygon { new(1, 2) }, Is.EqualTo(new EDBPolygon(new EDBPoint(1, 2))));

    [Test]
    public void EDBPath_default()
    {
        EDBPath defaultPath = default;
        Assert.IsFalse(defaultPath.Equals(new EDBPath { new(1, 2) }));
    }

    [Test]
    public void EDBPolygon_default()
    {
        EDBPolygon defaultPolygon = default;
        Assert.IsFalse(defaultPolygon.Equals(new EDBPolygon { new(1, 2) }));
    }

    [Test]
    public void Bug1011018()
    {
        var p = new EDBParameter();
        p.EDBDbType = EDBDbType.Time;
        p.Value = DateTime.Now;
        var o = p.Value;
    }

#pragma warning disable 618
    [Test]
    [IssueLink("https://github.com/npgsql/npgsql/issues/750")]
    public void EDBInet()
    {
        var v = new EDBInet(IPAddress.Parse("2001:1db8:85a3:1142:1000:8a2e:1370:7334"), 32);
        Assert.That(v.ToString(), Is.EqualTo("2001:1db8:85a3:1142:1000:8a2e:1370:7334/32"));
    }
#pragma warning restore 618

    [Test]
    public void EDBInet_parse_ipv4()
    {
        var ipv4 = new EDBInet("192.168.1.1/8");
        Assert.That(ipv4.Address, Is.EqualTo(IPAddress.Parse("192.168.1.1")));
        Assert.That(ipv4.Netmask, Is.EqualTo(8));

        ipv4 = new EDBInet("192.168.1.1/32");
        Assert.That(ipv4.Address, Is.EqualTo(IPAddress.Parse("192.168.1.1")));
        Assert.That(ipv4.Netmask, Is.EqualTo(32));
    }

    [Test]
    [IssueLink("https://github.com/npgsql/npgsql/issues/5638")]
    public void EDBInet_parse_ipv6()
    {
        var ipv6 = new EDBInet("2001:0000:130F:0000:0000:09C0:876A:130B/32");
        Assert.That(ipv6.Address, Is.EqualTo(IPAddress.Parse("2001:0000:130F:0000:0000:09C0:876A:130B")));
        Assert.That(ipv6.Netmask, Is.EqualTo(32));

        ipv6 = new EDBInet("2001:0000:130F:0000:0000:09C0:876A:130B");
        Assert.That(ipv6.Address, Is.EqualTo(IPAddress.Parse("2001:0000:130F:0000:0000:09C0:876A:130B")));
        Assert.That(ipv6.Netmask, Is.EqualTo(128));
    }

    [Test]
    public void EDBInet_ToString_ipv4()
    {
        Assert.That(new EDBInet("192.168.1.1/8").ToString(), Is.EqualTo("192.168.1.1/8"));
        Assert.That(new EDBInet("192.168.1.1/32").ToString(), Is.EqualTo("192.168.1.1"));
    }

    [Test]
    public void EDBInet_ToString_ipv6()
    {
        Assert.That(new EDBInet("2001:0:130f::9c0:876a:130b/32").ToString(), Is.EqualTo("2001:0:130f::9c0:876a:130b/32"));
        Assert.That(new EDBInet("2001:0:130f::9c0:876a:130b/128").ToString(), Is.EqualTo("2001:0:130f::9c0:876a:130b"));
    }
}
