using System;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    public class DbNullContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(DBNull); }
        }

        protected override bool SupportsNullValue
        {
            get { return false; }
        }

        [Test]
        public void Write()
        {
            var json = DoWrite(DBNull.Value);
            Assert.AreEqual("null", json);
        }

        [Test]
        public void WriteInvalid()
        {
            Assert.Throws<JsonContractException>(() => DoWrite(123));
            Assert.Throws<JsonContractException>(() => DoWrite(new object()));
        }

        [Test]
        public void Read()
        {
            var value = DoRead("null");
            Assert.AreEqual(DBNull.Value, value);

            value = DoRead("undefined");
            Assert.AreEqual(DBNull.Value, value);
        }

        [Test]
        public void ReadInvalid()
        {
            Assert.Throws<JsonContractException>(() => DoRead("''"));
            Assert.Throws<JsonContractException>(() => DoRead("{}"));
            Assert.Throws<JsonContractException>(() => DoRead("[]"));
            Assert.Throws<JsonContractException>(() => DoRead("123"));
        }
    }
}
