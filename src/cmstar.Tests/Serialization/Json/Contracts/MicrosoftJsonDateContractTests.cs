using System;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public class MicrosoftJsonDateContractTests : DateTimeOffsetContractTests
    {
        protected override JsonContract GetContract()
        {
            var contract = new MicrosoftJsonDateContract();
            return contract;
        }

        [Test]
        public override void Write()
        {
            var date = new DateTimeOffset(2013, 1, 25, 12, 26, 33, 123, TimeSpan.FromHours(0));
            var json = DoWrite(date);
            Assert.AreEqual("\"\\/Date(1359116793123)\\/\"", json);

            date = new DateTimeOffset(2012, 3, 15, 14, 25, 35, 152, TimeSpan.FromHours(8));
            json = DoWrite(date);
            Assert.AreEqual("\"\\/Date(1331792735152+0800)\\/\"", json);

            date = new DateTimeOffset(1976, 12, 2, 17, 42, 25, 765, TimeSpan.FromHours(-6));
            json = DoWrite(date);
            Assert.AreEqual("\"\\/Date(218418145765-0600)\\/\"", json);
        }
    }
}