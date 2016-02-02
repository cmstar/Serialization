using System;
using System.Data;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    public class DataRecordContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(DataRowRecord); }
        }

        protected override bool SupportsNullValue
        {
            get { return false; }
        }

        [Test]
        public void Write()
        {
            var dt = new DataTable();
            dt.Columns.Add("colInt", typeof(int));
            dt.Columns.Add("colString", typeof(string));
            dt.Columns.Add("colChar", typeof(char));
            dt.Columns.Add("colDecimal", typeof(decimal));
            dt.Columns.Add("colGuid", typeof(Guid));

            dt.Rows.Add(1, "value", 'a', 0.009M, Guid.Empty);

            var record = new DataRowRecord(dt.Rows[0]);
            var json = DoWrite(record, true);
            var expected =
@"{
    ""colInt"":1,
    ""colString"":""value"",
    ""colChar"":""a"",
    ""colDecimal"":0.009,
    ""colGuid"":""00000000-0000-0000-0000-000000000000""
}";
            Assert.AreEqual(expected, json);
        }

        [Test]
        public void Read()
        {
            Assert.Throws<NotSupportedException>(() => DoRead("{}"));
        }
    }
}