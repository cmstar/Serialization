using System;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public class JavascriptDateTimeContractTests : DateTimeContractTests
    {
        protected override JsonContract GetContarct()
        {
            var contract = new JavascriptDateTimeContract();
            return contract;
        }

        [Test]
        public override void Write()
        {
            var date = new DateTime(2013, 1, 25, 12, 26, 33, 123, DateTimeKind.Utc);
            var json = DoWrite(date);
            Assert.AreEqual("\"\\/Date(1359116793123+0000)\\/\"", json);

            date = new DateTime(2012, 3, 15, 6, 25, 35, 152, DateTimeKind.Utc);
            json = DoWrite(date);
            Assert.AreEqual("\"\\/Date(1331792735152+0000)\\/\"", json);

            date = new DateTime(1976, 12, 2, 23, 42, 25, 765, DateTimeKind.Utc);
            json = DoWrite(date);
            Assert.AreEqual("\"\\/Date(218418145765+0000)\\/\"", json);
        }
    }
}