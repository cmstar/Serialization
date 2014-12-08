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
    public class StringContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(string); }
        }

        [Test]
        public void Write()
        {
            var json = DoWrite("");
            Assert.AreEqual("\"\"", json);

            json = DoWrite("some value");
            Assert.AreEqual("\"some value\"", json);
        }

        [Test]
        public void Read()
        {
            var result = DoRead("\"\"");
            Assert.AreEqual("", result);

            result = DoRead("\"some value\"");
            Assert.AreEqual("some value", result);
        }

        [Test]
        public void WriteEscaped()
        {
            var json = DoWrite("\r\n\t");
            Assert.AreEqual(@"""\r\n\t""", json);

            json = DoWrite("one line\nand another");
            Assert.AreEqual(@"""one line\nand another""", json);
        }

        [Test]
        public void ReadEscaped()
        {
            var result = DoRead(@"""\r\n\t""");
            Assert.AreEqual("\r\n\t", result);

            result = DoRead(@"""one line\nand another""");
            Assert.AreEqual("one line\nand another", result);
        }

        [Test]
        public void ReadNonString()
        {
            var result = DoRead(@"123.456");
            Assert.AreEqual("123.456", result);

            result = DoRead(@"-3335");
            Assert.AreEqual("-3335", result);

            result = DoRead(@"true");
            Assert.AreEqual("true", result);

            result = DoRead(@"false");
            Assert.AreEqual("false", result);
        }
    }
}