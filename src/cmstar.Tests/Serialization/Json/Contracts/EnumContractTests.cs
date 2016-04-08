#region Licence
// The MIT License (MIT)
// 
// Copyright (c) 2013 Eric Ruan
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public class EnumContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(SaleOrderType); }
        }

        protected override bool SupportsNullValue
        {
            get { return false; }
        }

        [Test]
        public virtual void Write()
        {
            var json = DoWrite(SaleOrderType.Normal);
            Assert.AreEqual("0", json);

            json = DoWrite(SaleOrderType.Emergency);
            Assert.AreEqual("1", json);

            json = DoWrite((SaleOrderType)35);
            Assert.AreEqual("35", json);
        }

        [Test]
        public void ReadIndex()
        {
            var result = DoRead("0");
            Assert.AreEqual(SaleOrderType.Normal, result);

            result = DoRead("1");
            Assert.AreEqual(SaleOrderType.Emergency, result);

            result = DoRead("-35");
            Assert.AreEqual((SaleOrderType)(-35), result);
        }

        [Test]
        public void ReadStringIndex()
        {
            var result = DoRead("'0'");
            Assert.AreEqual(SaleOrderType.Normal, result);

            result = DoRead("'1'");
            Assert.AreEqual(SaleOrderType.Emergency, result);

            result = DoRead("'-35'");
            Assert.AreEqual((SaleOrderType)(-35), result);
        }

        [Test]
        public void ReadIllegalIndex()
        {
            Assert.Throws<JsonContractException>(() => DoRead("1.5"));
            Assert.Throws<JsonContractException>(() => DoRead("-33.23"));
            Assert.Throws<JsonContractException>(() => DoRead("NaN"));
        }

        [Test]
        public void ReadName()
        {
            var result = DoRead("\"Normal\"");
            Assert.AreEqual(SaleOrderType.Normal, result);

            result = DoRead("\"Emergency\"");
            Assert.AreEqual(SaleOrderType.Emergency, result);
        }

        [Test]
        public void ReadIllegalName()
        {
            Assert.Throws<JsonContractException>(() => DoRead("\"wrongvalue\""));
        }
    }

    [TestFixture]
    public class WriteNameEnumContractTests : EnumContractTests
    {
        protected override JsonContract GetContarct()
        {
            var contract = new EnumContract(UnderlyingType);
            contract.UseEnumName = true;
            return contract;
        }

        [Test]
        public override void Write()
        {
            var json = DoWrite(SaleOrderType.Normal);
            Assert.AreEqual("\"Normal\"", json);

            json = DoWrite(SaleOrderType.Emergency);
            Assert.AreEqual("\"Emergency\"", json);
        }

        [Test]
        public void WriteUndefinedName()
        {
            Assert.Throws<JsonContractException>(() => DoWrite((SaleOrderType)35));
        }
    }
}