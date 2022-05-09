using System;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public class DateTimeOffsetContractTests : ContractTestBase
    {
        protected override Type UnderlyingType => typeof(DateTimeOffset);

        protected override bool SupportsNullValue => false;

        [Test]
        public virtual void Write()
        {
            var date = new DateTimeOffset(2013, 1, 25, 12, 26, 33, 123, TimeSpan.FromHours(0));
            var json = DoWrite(date);
            Assert.AreEqual("\"2013-01-25T12:26:33.1230000Z\"", json);

            date = new DateTimeOffset(2012, 3, 15, 6, 25, 35, 152, TimeSpan.FromHours(4));
            json = DoWrite(date);
            Assert.AreEqual("\"2012-03-15T06:25:35.1520000+04:00\"", json);

            date = new DateTimeOffset(1976, 12, 2, 23, 42, 25, 765, TimeSpan.FromHours(-6));
            json = DoWrite(date);
            Assert.AreEqual("\"1976-12-02T23:42:25.7650000-06:00\"", json);
        }

        // The contract can read value in Microsoft JSON datetime format.
        [Test]
        public void ReadMicrosoftJsonDate()
        {
            var result = DoRead("\"\\/Date(1359116793123)\\/\"");
            Assert.IsInstanceOf<DateTimeOffset>(result);
            var expected = new DateTimeOffset(2013, 1, 25, 12, 26, 33, 123, TimeSpan.FromHours(0));
            Assert.AreEqual(expected, (DateTimeOffset)result);

            result = DoRead("\"/Date(218418145765+0400)/\"");
            Assert.IsInstanceOf<DateTimeOffset>(result);
            expected = new DateTimeOffset(1976, 12, 3, 3, 42, 25, 765, TimeSpan.FromHours(4));
            Assert.AreEqual(expected, (DateTimeOffset)result);

            result = DoRead("\"/Date(218418145765-0800)/\"");
            Assert.IsInstanceOf<DateTimeOffset>(result);
            expected = new DateTimeOffset(1976, 12, 2, 15, 42, 25, 765, TimeSpan.FromHours(-8));
            Assert.AreEqual(expected, (DateTimeOffset)result);
        }

        [Test]
        public void ReadString()
        {
            var result = DoRead("\"2013-11-25 18:28:36.213\"");
            Assert.IsInstanceOf<DateTimeOffset>(result);
            var localTime = DateTime.Parse("2013-11-25 18:28:36.213");
            var expected = new DateTimeOffset(localTime);
            Assert.AreEqual(expected, result);

            result = DoRead("\"2013-11-25T18:28:36.213+0000\"");
            Assert.IsInstanceOf<DateTimeOffset>(result);
            expected = new DateTimeOffset(2013, 11, 25, 18, 28, 36, 213, TimeSpan.FromHours(0));
            Assert.AreEqual(expected, result);

            result = DoRead("\"2013-11-25T18:28:36.213+0300\"");
            Assert.IsInstanceOf<DateTimeOffset>(result);
            expected = new DateTimeOffset(2013, 11, 25, 18, 28, 36, 213, TimeSpan.FromHours(3));
            Assert.AreEqual(expected, result);

            result = DoRead("\"2013-11-25T18:28:36.213-0600\"");
            Assert.IsInstanceOf<DateTimeOffset>(result);
            expected = new DateTimeOffset(2013, 11, 25, 18, 28, 36, 213, TimeSpan.FromHours(-6));
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadIllegal()
        {
            Assert.Throws<JsonContractException>(() => DoRead("\"some value that is not a date\""));
        }
    }
}