using System;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public class CustomFormatDateTimeOffsetContractTests : DateTimeOffsetContractTests
    {
        protected override JsonContract GetContract()
        {
            var contract = new CustomFormatDateTimeOffsetContract();
            contract.Format = "yyyy@M@dd HH~~mm~~ss ffffff";
            return contract;
        }

        public override void Write()
        {
            var date = new DateTime(2013, 1, 25, 12, 26, 33, 123);
            var json = DoWrite(date);
            Assert.AreEqual("\"2013@1@25 12~~26~~33 123000\"", json);

            date = new DateTime(2012, 3, 15, 6, 25, 35, 152);
            json = DoWrite(date);
            Assert.AreEqual("\"2012@3@15 06~~25~~35 152000\"", json);

            date = new DateTime(1976, 12, 2, 23, 42, 25, 0);
            json = DoWrite(date);
            Assert.AreEqual("\"1976@12@02 23~~42~~28 000000\"", json);
        }
    }
}