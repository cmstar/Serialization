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
    public class DateTimeContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(DateTime); }
        }

        protected override bool SupportsNullValue
        {
            get { return false; }
        }

        [Test]
        public virtual void Write()
        {
            var date = new DateTime(2013, 1, 25, 12, 26, 33, 123, DateTimeKind.Utc);
            var json = DoWrite(date);
            Assert.AreEqual("\"\\/Date(1359116793123)\\/\"", json);

            date = new DateTime(2012, 3, 15, 6, 25, 35, 152, DateTimeKind.Utc);
            json = DoWrite(date);
            Assert.AreEqual("\"\\/Date(1331792735152)\\/\"", json);

            date = new DateTime(1976, 12, 2, 23, 42, 25, 765, DateTimeKind.Utc);
            json = DoWrite(date);
            Assert.AreEqual("\"\\/Date(218418145765)\\/\"", json);
        }

        [Test]
        public void ReadDate()
        {
            var result = DoRead("\"\\/Date(1359116793123)\\/\"");
            Assert.IsInstanceOf<DateTime>(result);
            var expected = new DateTime(2013, 1, 25, 12, 26, 33, 123, DateTimeKind.Utc);
            Assert.AreEqual(expected, ((DateTime)result).ToUniversalTime());

            result = DoRead("\"\\/Date(1331792735152)\\/\"");
            Assert.IsInstanceOf<DateTime>(result);
            expected = new DateTime(2012, 3, 15, 6, 25, 35, 152, DateTimeKind.Utc);
            Assert.AreEqual(expected, ((DateTime)result).ToUniversalTime());

            result = DoRead("\"\\/Date(1331792735152)\\/\"");
            Assert.IsInstanceOf<DateTime>(result);
            expected = new DateTime(2012, 3, 15, 6, 25, 35, 152, DateTimeKind.Utc);
            Assert.AreEqual(expected, ((DateTime)result).ToUniversalTime());

            result = DoRead("\"\\/Date(218418145765)\\/\"");
            Assert.IsInstanceOf<DateTime>(result);
            expected = new DateTime(1976, 12, 2, 23, 42, 25, 765, DateTimeKind.Utc);
            Assert.AreEqual(expected, ((DateTime)result).ToUniversalTime());

            result = DoRead("\"/Date(218418145765+0400)/\"");
            Assert.IsInstanceOf<DateTime>(result);
            expected = new DateTime(1976, 12, 2, 23, 42, 25, 765, DateTimeKind.Utc);
            Assert.AreEqual(expected, ((DateTime)result).ToUniversalTime());

            result = DoRead("\"/Date(218418145765-0800)/\"");
            Assert.IsInstanceOf<DateTime>(result);
            expected = new DateTime(1976, 12, 2, 23, 42, 25, 765, DateTimeKind.Utc);
            Assert.AreEqual(expected, ((DateTime)result).ToUniversalTime());
        }

        [Test]
        public void ReadString()
        {
            var result = DoRead("\"May 5, 2012\"");
            Assert.IsInstanceOf<DateTime>(result);
            var expected = new DateTime(2012, 5, 5, 0, 0, 0, 0);
            Assert.AreEqual(expected, result);

            result = DoRead("\"2013-11-25 18:28:36.213\"");
            Assert.IsInstanceOf<DateTime>(result);
            expected = new DateTime(2013, 11, 25, 18, 28, 36, 213);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadIllegal()
        {
            Assert.Throws<JsonContractException>(() => DoRead("\"some value that is not a date\""));
            Assert.Throws<JsonContractException>(() => DoRead("\"/Date(218418145765-XXXX)/\""));
        }
    }
}