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
    public class BooleanContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(bool); }
        }

        protected override bool SupportsNullValue
        {
            get { return false; }
        }

        [Test]
        public void Write()
        {
            var json = DoWrite(true);
            Assert.AreEqual("true", json);

            json = DoWrite(false);
            Assert.AreEqual("false", json);
        }

        [Test]
        public void Read()
        {
            var result = DoRead("true");
            Assert.AreEqual(true, result);

            result = DoRead("false");
            Assert.AreEqual(false, result);
        }        
        
        [Test]
        public void ReadString()
        {
            var result = DoRead("\"true\"");
            Assert.AreEqual(true, result);

            result = DoRead("\"True\"");
            Assert.AreEqual(true, result);

            result = DoRead("\"TRue\"");
            Assert.AreEqual(true, result);

            result = DoRead("\"false\"");
            Assert.AreEqual(false, result);

            result = DoRead("\"False\"");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void ReadNumber()
        {
            var result = DoRead("1");
            Assert.AreEqual(true, result);

            result = DoRead("123");
            Assert.AreEqual(true, result);

            result = DoRead("-222.232");
            Assert.AreEqual(true, result);

            result = DoRead("0");
            Assert.AreEqual(false, result);

            result = DoRead("0.0");
            Assert.AreEqual(false, result);

            result = DoRead(".0000");
            Assert.AreEqual(false, result);
        }
    }
}