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

namespace cmstar.Serialization.Json
{
    [TestFixture]
    public class JsonConvertTests
    {
        [Test]
        public void ToJsonDateTimeValueUtc()
        {
            var datetime = new DateTime(2012, 3, 15, 6, 25, 35, 152, DateTimeKind.Utc);
            var result = JsonConvert.ToJsonDateTimeValue(datetime, true);
            Assert.AreEqual(@"/Date(1331792735152)/", result);

            result = JsonConvert.ToJsonDateTimeValue(datetime, false);
            Assert.AreEqual(@"Date(1331792735152)", result);

            datetime = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            result = JsonConvert.ToJsonDateTimeValue(datetime, true);
            Assert.AreEqual(@"/Date(-62135596800000)/", result);
        }

        [Test]
        public void TryParseJsonDateTimeValueSucceeded()
        {
            DateTime datetime;

            var dateString = @"/Date(1331792735152)/";
            var expected = new DateTime(2012, 3, 15, 6, 25, 35, 152, DateTimeKind.Utc);
            Assert.IsTrue(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));
            Assert.AreEqual(expected, datetime.ToUniversalTime());

            dateString = @"Date(1331792735152)";
            expected = new DateTime(2012, 3, 15, 6, 25, 35, 152, DateTimeKind.Utc);
            Assert.IsTrue(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));
            Assert.AreEqual(expected, datetime.ToUniversalTime());

            dateString = @"/Date(-62135596800000)/";
            expected = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            Assert.IsTrue(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));
            Assert.AreEqual(expected, datetime.ToUniversalTime());

            dateString = @"/Date(1331763935152+0800)/";
            expected = new DateTime(2012, 3, 14, 22, 25, 35, 152, DateTimeKind.Utc);
            Assert.IsTrue(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));
            Assert.AreEqual(expected, datetime.ToUniversalTime());

            dateString = @"Date(1331763935152-0600)";
            expected = new DateTime(2012, 3, 14, 22, 25, 35, 152, DateTimeKind.Utc);
            Assert.IsTrue(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));
            Assert.AreEqual(expected, datetime.ToUniversalTime());
        }

        [Test]
        public void TryParseJsonDateTimeValueFailed()
        {
            DateTime datetime;

            var dateString = @"./Date(1331792735152)/";
            Assert.IsFalse(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));

            dateString = @"Date(1331763935152-0600)/";
            Assert.IsFalse(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));

            dateString = @"Date(1331763935152X0600)";
            Assert.IsFalse(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));

            dateString = @"/Date(1331763935152-0600)";
            Assert.IsFalse(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));

            dateString = @"Date(1331763935152-0600).";
            Assert.IsFalse(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));

            dateString = @"DATE(1331763935152-0600)";
            Assert.IsFalse(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));

            dateString = @"/Date1331763935152-0600/";
            Assert.IsFalse(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));

            dateString = @"\/Date(1331763935152)\/";
            Assert.IsFalse(JsonConvert.TryParseJsonDateTimeValue(dateString, out datetime));
        }

        [Test]
        public void ConvertLocalTime()
        {
            var datetime = new DateTime(2012, 3, 15, 6, 25, 35, 152, DateTimeKind.Local);
            var jsonTime = JsonConvert.ToJsonDateTimeValue(datetime, false);

            DateTime datetimeConvertBack;
            Assert.IsTrue(JsonConvert.TryParseJsonDateTimeValue(jsonTime, out datetimeConvertBack));
            Assert.AreEqual(datetime, datetimeConvertBack);
        }

        [Test]
        public void ConvertUtcTime()
        {
            var datetime = new DateTime(2012, 3, 15, 6, 25, 35, 152, DateTimeKind.Utc);
            var jsonTime = JsonConvert.ToJsonDateTimeValue(datetime, false);

            DateTime datetimeConvertBack;
            Assert.IsTrue(JsonConvert.TryParseJsonDateTimeValue(jsonTime, out datetimeConvertBack));
            var expected = datetime.ToLocalTime();
            Assert.AreEqual(expected, datetimeConvertBack);
        }
    }
}