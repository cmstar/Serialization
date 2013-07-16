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
using System.IO;
using NUnit.Framework;

namespace cmstar.Serialization.Json
{
    [TestFixture]
    public class JsonReaderTests
    {
        [Test]
        public void ReadScalarString()
        {
            ReadScalarOnly("\"The String\"", JsonToken.StringValue, "The String");
            ReadScalarOnly("\"Say\\t\\\"Hello\\r\\nWorld\\\".\"",
                JsonToken.StringValue, "Say\t\"Hello\r\nWorld\".");
            ReadScalarOnly(@"""\ud869\udea5""", JsonToken.StringValue, "𪚥");
        }

        [Test]
        public void ReadScalarNumber()
        {
            ReadScalarOnly("123", JsonToken.NumberValue, 123D);
            ReadScalarOnly("123.456", JsonToken.NumberValue, 123.456D);
            ReadScalarOnly(".33", JsonToken.NumberValue, 0.33D);
            ReadScalarOnly("-15.26", JsonToken.NumberValue, -15.26D);
            ReadScalarOnly("1.24e4", JsonToken.NumberValue, 12400D);
            ReadScalarOnly("-15.3E-2", JsonToken.NumberValue, -0.153D);
            ReadScalarOnly("NaN", JsonToken.NumberValue, double.NaN);
            ReadScalarOnly("Infinity", JsonToken.NumberValue, double.PositiveInfinity);
            ReadScalarOnly("-Infinity", JsonToken.NumberValue, double.NegativeInfinity);
        }

        [Test]
        public void ReadScalarNullAndUndefined()
        {
            ReadScalarOnly("null", JsonToken.NullValue, null);
            ReadScalarOnly("undefined", JsonToken.UndefinedValue, null);
        }

        [Test]
        public void ReadObjectSimple()
        {
            var json =
@"   {
""string"": ""s"", ""number""   :  
-Infinity  ,
""null"":null}";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ObjectStart, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Containter);
                AssertPosition(r, 1, 5);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.PropertyName, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Containter);
                Assert.AreEqual("string", r.Value);
                AssertPosition(r, 2, 10);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.StringValue, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Containter);
                Assert.AreEqual("s", r.Value);
                AssertPosition(r, 2, 14);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Containter);
                AssertPosition(r, 2, 15);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.PropertyName, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Containter);
                Assert.AreEqual("number", r.Value);
                AssertPosition(r, 2, 28);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.NumberValue, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Containter);
                Assert.AreEqual(double.NegativeInfinity, r.Value);
                AssertPosition(r, 3, 10);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Containter);
                AssertPosition(r, 3, 13);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.PropertyName, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Containter);
                Assert.AreEqual("null", r.Value);
                AssertPosition(r, 4, 8);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.NullValue, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Containter);
                Assert.AreEqual(null, r.Value);
                AssertPosition(r, 4, 12);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ObjectEnd, r.Token);
                Assert.AreEqual(JsonToken.None, r.Containter);
                AssertPosition(r, 4, 13);

                Assert.IsFalse(r.Read());
            }
        }

        [Test]
        public void ReadArraySimple()
        {
            var json =
@"[  -5.5E-2
,""Line1\nLine2"", null, .0
]";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayStart, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.NumberValue, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);
                Assert.AreEqual(-0.055D, r.Value);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.StringValue, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);
                Assert.AreEqual("Line1\nLine2", r.Value);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.NullValue, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);
                Assert.AreEqual(null, r.Value);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.NumberValue, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);
                Assert.AreEqual(0D, r.Value);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayEnd, r.Token);
                Assert.AreEqual(JsonToken.None, r.Containter);

                Assert.IsFalse(r.Read());
            }
        }

        [Test]
        public void ReadArrayEmbedded()
        {
            var json = @"[ [], [], [1] ]";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayStart, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayStart, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayEnd, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayStart, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayEnd, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayStart, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.NumberValue, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);
                Assert.AreEqual(1D, r.Value);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayEnd, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Containter);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayEnd, r.Token);
                Assert.AreEqual(JsonToken.None, r.Containter);
                Assert.IsFalse(r.Read());
            }
        }

        [Test]
        public void ReadObjectMixed()
        {
            var json =
@"  {
    ""array"":[1, ""s""],
    ""number"":NaN,
    ""object"":{
        ""n"":5e-3,
        ""a"":[]
    }
}";
            using (var r = CreateReader(json))
            {
                AssertPeeking(r, JsonToken.ObjectStart, 1, 3, JsonToken.None);
                AssertReading(r, JsonToken.ObjectStart, false, null, 1, 4, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.PropertyName, 2, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "array", 2, 13, JsonToken.ObjectStart);

                //array
                AssertPeeking(r, JsonToken.ArrayStart, 2, 13, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ArrayStart, false, null, 2, 14, JsonToken.ArrayStart);

                AssertPeeking(r, JsonToken.NumberValue, 2, 14, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.NumberValue, true, 1D, 2, 15, JsonToken.ArrayStart);

                AssertPeeking(r, JsonToken.Comma, 2, 15, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.Comma, false, null, 2, 16, JsonToken.ArrayStart);

                AssertPeeking(r, JsonToken.StringValue, 2, 17, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.StringValue, true, "s", 2, 20, JsonToken.ArrayStart);

                AssertPeeking(r, JsonToken.ArrayEnd, 2, 20, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.ArrayEnd, false, null, 2, 21, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.Comma, 2, 21, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.Comma, false, null, 2, 22, JsonToken.ObjectStart);

                //number
                AssertPeeking(r, JsonToken.PropertyName, 3, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "number", 3, 14, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.NumberValue, 3, 14, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.NumberValue, true, double.NaN, 3, 17, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.Comma, 3, 17, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.Comma, false, null, 3, 18, JsonToken.ObjectStart);

                //object
                AssertPeeking(r, JsonToken.PropertyName, 4, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "object", 4, 14, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.ObjectStart, 4, 14, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ObjectStart, false, null, 4, 15, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.PropertyName, 5, 9, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "n", 5, 13, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.NumberValue, 5, 13, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.NumberValue, true, 0.005D, 5, 17, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.Comma, 5, 17, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.Comma, false, null, 5, 18, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.PropertyName, 6, 9, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "a", 6, 13, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.ArrayStart, 6, 13, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ArrayStart, false, null, 6, 14, JsonToken.ArrayStart);

                AssertPeeking(r, JsonToken.ArrayEnd, 6, 14, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.ArrayEnd, false, null, 6, 15, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.ObjectEnd, 7, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ObjectEnd, false, null, 7, 6, JsonToken.ObjectStart);

                AssertPeeking(r, JsonToken.ObjectEnd, 8, 1, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ObjectEnd, false, null, 8, 2, JsonToken.None);

                Assert.IsFalse(r.Read());
            }
        }

        private void AssertPeeking(
            JsonReader reader, JsonToken token, int lineNumber, int columnNumber, JsonToken container)
        {
            Assert.AreEqual(token, reader.PeekNextToken());
            AssertPosition(reader, lineNumber, columnNumber);
            Assert.AreEqual(container, reader.Containter);
        }

        private void AssertReading(
            JsonReader reader, JsonToken token, bool asserValue, object value,
            int lineNumber, int columnNumber, JsonToken container)
        {
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(token, reader.Token);
            if (asserValue)
            {
                Assert.AreEqual(value, reader.Value);
            }
            AssertPosition(reader, lineNumber, columnNumber);
            Assert.AreEqual(container, reader.Containter);
        }

        private void AssertPosition(JsonReader reader, int lineNumber, int columnNumber)
        {
            Assert.AreEqual(lineNumber, reader.LineNumber, "Assertion of the line number.");
            Assert.AreEqual(columnNumber, reader.ColumnNumber, "Assertion of the column number.");
        }

        private void ReadScalarOnly(string json, JsonToken tokenExpected, object valueExpected)
        {
            Console.WriteLine("Testing value {0}", json);
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                Assert.AreEqual(tokenExpected, r.Token);
                Assert.AreEqual(valueExpected, r.Value);
                Assert.AreEqual(JsonToken.None, r.Containter);
                Assert.IsFalse(r.Read());
            }
        }

        private JsonReader CreateReader(string json)
        {
            var textReader = new StringReader(json);
            var reader = new JsonReader(textReader);
            return reader;
        }
    }
}