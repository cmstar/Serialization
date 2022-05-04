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
    public class NumberContractTests
    {
        [Test]
        public void TestTypeValidation()
        {
            Assert.DoesNotThrow(() => new NumberContract(typeof(int)));
            Assert.DoesNotThrow(() => new NumberContract(typeof(byte)));
            Assert.DoesNotThrow(() => new NumberContract(typeof(ulong)));
            Assert.DoesNotThrow(() => new NumberContract(typeof(char)));
            Assert.DoesNotThrow(() => new NumberContract(typeof(bool)));

            Assert.Throws<ArgumentException>(() => new NumberContract(typeof(DateTime)));
            Assert.Throws<ArgumentException>(() => new NumberContract(typeof(string)));
            Assert.Throws<ArgumentException>(() => new NumberContract(typeof(SaleOrderType)));
            Assert.Throws<ArgumentException>(() => new NumberContract(typeof(DBNull)));
        }
    }

    [TestFixture]
    public class IntegerNumberContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(int); }
        }

        protected override bool SupportsNullValue
        {
            get { return false; }
        }

        [Test]
        public void Write()
        {
            var json = DoWrite(123);
            Assert.AreEqual("123", json);

            json = DoWrite(0);
            Assert.AreEqual("0", json);

            json = DoWrite(-54321);
            Assert.AreEqual("-54321", json);
        }

        [Test]
        public void Read()
        {
            var result = DoRead("123");
            Assert.AreEqual(123, result);

            result = DoRead("0");
            Assert.AreEqual(0, result);

            result = DoRead("-54321");
            Assert.AreEqual(-54321, result);

            result = DoRead("-123.4567");
            Assert.AreEqual(-123, result);

            result = DoRead("'-123.4567'");
            Assert.AreEqual(-123, result);

            result = DoRead("\"-123.4567\"");
            Assert.AreEqual(-123, result);
        }
    }

    [TestFixture]
    public class DoubleNumberContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(double); }
        }

        protected override bool SupportsNullValue
        {
            get { return false; }
        }

        [Test]
        public void Write()
        {
            var json = DoWrite(123.456789);
            Assert.AreEqual("123.456789", json);

            json = DoWrite(0D);
            Assert.AreEqual("0", json);

            json = DoWrite(-0.33D);
            Assert.AreEqual("-0.33", json);
        }

        [Test]
        public void Read()
        {
            var result = DoRead("123.456789");
            Assert.AreEqual(123.456789, result);

            result = DoRead("0.00000");
            Assert.AreEqual(0, result);

            result = DoRead("-0.33");
            Assert.AreEqual(-0.33, result);

            result = DoRead("'-0.33'");
            Assert.AreEqual(-0.33, result);
        }

        [Test]
        public void WriteSpecial()
        {
            var json = DoWrite(double.NaN);
            Assert.AreEqual("NaN", json);

            json = DoWrite(double.PositiveInfinity);
            Assert.AreEqual("Infinity", json);

            json = DoWrite(double.NegativeInfinity);
            Assert.AreEqual("-Infinity", json);
        }

        [Test]
        public void ReadSpecial()
        {
            var result = DoRead("NaN");
            Assert.AreEqual(double.NaN, result);

            result = DoRead("Infinity");
            Assert.AreEqual(double.PositiveInfinity, result);

            result = DoRead("-Infinity");
            Assert.AreEqual(double.NegativeInfinity, result);
        }

        [Test]
        public void ReadExponent()
        {
            var result = DoRead("-1.23e-6");
            Assert.AreEqual(-1.23e-6, result);

            result = DoRead("33E35");
            Assert.AreEqual(33e35, result);
        }
    }

    [TestFixture]
    public class BooleanNumberContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(bool); }
        }

        protected override bool SupportsNullValue
        {
            get { return false; }
        }

        protected override JsonContract GetContract()
        {
            return new NumberContract(typeof(bool));
        }

        [Test]
        public void Write()
        {
            var json = DoWrite(true);
            Assert.AreEqual("1", json);

            json = DoWrite(false);
            Assert.AreEqual("0", json);
        }

        [Test]
        public void Read()
        {
            var result = DoRead("1");
            Assert.AreEqual(true, result);

            result = DoRead("-0.221");
            Assert.AreEqual(true, result);

            result = DoRead("12615e3");
            Assert.AreEqual(true, result);

            result = DoRead("0");
            Assert.AreEqual(false, result);
        }
    }

}