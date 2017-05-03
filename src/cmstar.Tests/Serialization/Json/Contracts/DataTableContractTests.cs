using System;
using System.Data;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public class DataTableContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(DataTable); }
        }

        protected override bool CanRead
        {
            get { return false; }
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

            dt.Rows.Add(1, "value", 'a', 0.009M);
            dt.Rows.Add(-9999, string.Empty, null, 16M);
            dt.Rows.Add(0, "line1\nline2", 'b', DBNull.Value);

            var json = DoWrite(dt, true);
            var expected =
@"[
    {
        ""colInt"":1,
        ""colString"":""value"",
        ""colChar"":""a"",
        ""colDecimal"":0.009
    },{
        ""colInt"":-9999,
        ""colString"":"""",
        ""colChar"":null,
        ""colDecimal"":16
    },{
        ""colInt"":0,
        ""colString"":""line1\nline2"",
        ""colChar"":""b"",
        ""colDecimal"":null
    }
]";
            Assert.AreEqual(expected, json);
        }

        [Test]
        public void Read()
        {
            Assert.Throws<NotSupportedException>(() => DoRead("[]"));
        }
    }
}