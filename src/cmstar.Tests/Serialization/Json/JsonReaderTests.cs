﻿#region Licence
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
            ReadScalarOnly("'The String'", JsonToken.StringValue, "The String");
            ReadScalarOnly("\"Say\\t\\\"Hello\\r\\nWorld\\\".\"",
                JsonToken.StringValue, "Say\t\"Hello\r\nWorld\".");
            ReadScalarOnly(@"""\ud869\udea5""", JsonToken.StringValue, "𪚥");

            // test unmatched quote char
            Assert.Throws<JsonFormatException>(() => ReadScalarOnly("\"The String'", JsonToken.StringValue, "The String"));
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
""string"": ""s"", ""$number""   :  
-Infinity  ,
""null_prop"":null}";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ObjectStart, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Container);
                AssertPosition(r, 1, 5);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.PropertyName, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Container);
                Assert.AreEqual("string", r.Value);
                AssertPosition(r, 2, 10);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.StringValue, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Container);
                Assert.AreEqual("s", r.Value);
                AssertPosition(r, 2, 14);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Container);
                AssertPosition(r, 2, 15);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.PropertyName, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Container);
                Assert.AreEqual("$number", r.Value);
                AssertPosition(r, 2, 29);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.NumberValue, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Container);
                Assert.AreEqual(double.NegativeInfinity, r.Value);
                AssertPosition(r, 3, 10);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Container);
                AssertPosition(r, 3, 13);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.PropertyName, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Container);
                Assert.AreEqual("null_prop", r.Value);
                AssertPosition(r, 4, 13);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.NullValue, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Container);
                Assert.AreEqual(null, r.Value);
                AssertPosition(r, 4, 17);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ObjectEnd, r.Token);
                Assert.AreEqual(JsonToken.None, r.Container);
                AssertPosition(r, 4, 18);

                Assert.IsFalse(r.Read());
            }
        }

        [Test]
        public void ReadInvalidPropertyName()
        {
            var json = @"{'name:123}";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ObjectStart, r.Token);
                Assert.AreEqual(JsonToken.ObjectStart, r.Container);

                Assert.Throws<JsonFormatException>(() => r.Read());
            }

            json = @"{\na me :123}";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                Assert.Throws<JsonFormatException>(() => r.Read());
            }

            json = @"{name"" :123}";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                Assert.Throws<JsonFormatException>(() => r.Read());
            }

            json = @"{na;me :123}";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                Assert.Throws<JsonFormatException>(() => r.Read());
            }
        }

        [Test]
        public void ReadQuotedPropertyName()
        {
            var json = @"{""na*me"":123}";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                AssertReading(r, JsonToken.PropertyName, true, "na*me", JsonToken.ObjectStart);
            }

            json = @"{'na me':123}";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                AssertReading(r, JsonToken.PropertyName, true, "na me", JsonToken.ObjectStart);
            }

            json = @"{  '___\r\r___\n\n___'  :123}";
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                AssertReading(r, JsonToken.PropertyName, true, "___\r\r___\n\n___", JsonToken.ObjectStart);
            }
        }

        [Test]
        public void ReadUnQuotedPropertyName()
        {
            var json = @"{ $the_name :123}";
            using (var r = CreateReader(json))
            {
                AssertReading(r, JsonToken.ObjectStart, false, null, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "$the_name", JsonToken.ObjectStart);
                AssertReading(r, JsonToken.NumberValue, true, 123D, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ObjectEnd, false, null, JsonToken.None);
            }

            json = @"{ $_$ :123}";
            using (var r = CreateReader(json))
            {
                AssertReading(r, JsonToken.ObjectStart, false, null, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "$_$", JsonToken.ObjectStart);
            }

            json = @"{ abcd1234 :123}";
            using (var r = CreateReader(json))
            {
                AssertReading(r, JsonToken.ObjectStart, false, null, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "abcd1234", JsonToken.ObjectStart);
            }
        }

        [Test]
        public void ReadPureComment()
        {
            void DoRead(string value)
            {
                Console.WriteLine("Testing... " + value);

                using (var r = CreateReader(value))
                {
                    Assert.IsFalse(r.Read(), "Read");
                    Assert.AreEqual(JsonToken.None, r.Token);
                }

                using (var r = CreateReader(value))
                {
                    Assert.AreEqual(JsonToken.None, r.PeekNextToken(), "Peek");
                    Assert.IsFalse(r.Read());
                }
            }

            DoRead("//");
            DoRead("  //");
            DoRead("//  ");
            DoRead("\n// ");

            DoRead("/* comment */");
            DoRead(" /* \r\n*/  ");
            DoRead("  /*/  */ ");

            DoRead(" ///* */");
            DoRead("/* comment1 */ \r // comment2 \n // comment3 \r\n /**/");
        }

        [Test]
        public void ReadCommentMixed()
        {
            var json =
@" // this is comment
//
/*{*}*/{
    array:[/**/]
    /*c*/,
//line1
num:/*comment*/NaN,
//line2    
    ""object"":{
        // line
    }
}//";
            using (var r = CreateReader(json))
            {
                AssertPeeking(r, JsonToken.ObjectStart, 3, 8, JsonToken.None);
                AssertReading(r, JsonToken.ObjectStart, false, null, JsonToken.ObjectStart, 3, 9);

                // array
                AssertPeeking(r, JsonToken.PropertyName, 4, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "array", JsonToken.ObjectStart, 4, 11);

                AssertPeeking(r, JsonToken.ArrayStart, 4, 11, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ArrayStart, false, null, JsonToken.ArrayStart, 4, 12);

                AssertPeeking(r, JsonToken.ArrayEnd, 4, 16, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.ArrayEnd, false, null, JsonToken.ObjectStart, 4, 17);

                AssertPeeking(r, JsonToken.Comma, 5, 10, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ObjectStart, 5, 11);

                // num
                AssertPeeking(r, JsonToken.PropertyName, 7, 1, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "num", JsonToken.ObjectStart, 7, 5);

                AssertPeeking(r, JsonToken.NumberValue, 7, 16, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.NumberValue, true, double.NaN, JsonToken.ObjectStart, 7, 19);

                AssertPeeking(r, JsonToken.Comma, 7, 19, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ObjectStart, 7, 20);

                // object
                AssertPeeking(r, JsonToken.PropertyName, 9, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "object", JsonToken.ObjectStart, 9, 14);

                AssertPeeking(r, JsonToken.ObjectStart, 9, 14, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ObjectStart, false, null, JsonToken.ObjectStart, 9, 15);

                AssertPeeking(r, JsonToken.ObjectEnd, 11, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ObjectEnd, false, null, JsonToken.ObjectStart, 11, 6);

                AssertPeeking(r, JsonToken.ObjectEnd, 12, 1, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ObjectEnd, false, null, JsonToken.None, 12, 2);

                Assert.IsFalse(r.Read());
                Assert.AreEqual(12, r.LineNumber, "final line");
                Assert.AreEqual(4, r.ColumnNumber, "final column");
            }
        }

        [Test]
        public void ReadArraySimple()
        {
            var json =
@"[  -5.5E-2
,""Line1\nLine2"", null, .0  ,  'sr'
]";
            using (var r = CreateReader(json))
            {
                AssertReading(r, JsonToken.ArrayStart, false, null, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.NumberValue, true, -0.055D, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.StringValue, true, "Line1\nLine2", JsonToken.ArrayStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.NullValue, true, null, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.NumberValue, true, 0D, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.StringValue, true, "sr", JsonToken.ArrayStart);
                AssertReading(r, JsonToken.ArrayEnd, false, null, JsonToken.None);

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
                Assert.AreEqual(JsonToken.ArrayStart, r.Container);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayStart, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Container);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayEnd, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Container);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Container);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayStart, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Container);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayEnd, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Container);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.Comma, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Container);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayStart, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Container);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.NumberValue, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Container);
                Assert.AreEqual(1D, r.Value);
                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayEnd, r.Token);
                Assert.AreEqual(JsonToken.ArrayStart, r.Container);

                Assert.IsTrue(r.Read());
                Assert.AreEqual(JsonToken.ArrayEnd, r.Token);
                Assert.AreEqual(JsonToken.None, r.Container);
                Assert.IsFalse(r.Read());
            }
        }

        [Test]
        public void ReadObjectMixed()
        {
            var json =
@"  {
    ""array"":[1, ""s"", 'r'],
    '$number$' : NaN,
    ""object"":{
        ""n"":5e-3,
        'a':[]
    }
}";
            using (var r = CreateReader(json))
            {
                AssertPeeking(r, JsonToken.ObjectStart, 1, 3, JsonToken.None);
                AssertReading(r, JsonToken.ObjectStart, false, null, JsonToken.ObjectStart, 1, 4);

                AssertPeeking(r, JsonToken.PropertyName, 2, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "array", JsonToken.ObjectStart, 2, 13);

                //array
                AssertPeeking(r, JsonToken.ArrayStart, 2, 13, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ArrayStart, false, null, JsonToken.ArrayStart, 2, 14);

                AssertPeeking(r, JsonToken.NumberValue, 2, 14, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.NumberValue, true, 1D, JsonToken.ArrayStart, 2, 15);

                AssertPeeking(r, JsonToken.Comma, 2, 15, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ArrayStart, 2, 16);

                AssertPeeking(r, JsonToken.StringValue, 2, 17, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.StringValue, true, "s", JsonToken.ArrayStart, 2, 20);

                AssertPeeking(r, JsonToken.Comma, 2, 20, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ArrayStart, 2, 21);

                AssertPeeking(r, JsonToken.StringValue, 2, 22, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.StringValue, true, "r", JsonToken.ArrayStart, 2, 25);

                AssertPeeking(r, JsonToken.ArrayEnd, 2, 25, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.ArrayEnd, false, null, JsonToken.ObjectStart, 2, 26);

                AssertPeeking(r, JsonToken.Comma, 2, 26, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ObjectStart, 2, 27);

                //number
                AssertPeeking(r, JsonToken.PropertyName, 3, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "$number$", JsonToken.ObjectStart, 3, 17);

                AssertPeeking(r, JsonToken.NumberValue, 3, 18, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.NumberValue, true, double.NaN, JsonToken.ObjectStart, 3, 21);

                AssertPeeking(r, JsonToken.Comma, 3, 21, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ObjectStart, 3, 22);

                //object
                AssertPeeking(r, JsonToken.PropertyName, 4, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "object", JsonToken.ObjectStart, 4, 14);

                AssertPeeking(r, JsonToken.ObjectStart, 4, 14, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ObjectStart, false, null, JsonToken.ObjectStart, 4, 15);

                AssertPeeking(r, JsonToken.PropertyName, 5, 9, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "n", JsonToken.ObjectStart, 5, 13);

                AssertPeeking(r, JsonToken.NumberValue, 5, 13, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.NumberValue, true, 0.005D, JsonToken.ObjectStart, 5, 17);

                AssertPeeking(r, JsonToken.Comma, 5, 17, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.Comma, false, null, JsonToken.ObjectStart, 5, 18);

                AssertPeeking(r, JsonToken.PropertyName, 6, 9, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.PropertyName, true, "a", JsonToken.ObjectStart, 6, 13);

                AssertPeeking(r, JsonToken.ArrayStart, 6, 13, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ArrayStart, false, null, JsonToken.ArrayStart, 6, 14);

                AssertPeeking(r, JsonToken.ArrayEnd, 6, 14, JsonToken.ArrayStart);
                AssertReading(r, JsonToken.ArrayEnd, false, null, JsonToken.ObjectStart, 6, 15);

                AssertPeeking(r, JsonToken.ObjectEnd, 7, 5, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ObjectEnd, false, null, JsonToken.ObjectStart, 7, 6);

                AssertPeeking(r, JsonToken.ObjectEnd, 8, 1, JsonToken.ObjectStart);
                AssertReading(r, JsonToken.ObjectEnd, false, null, JsonToken.None, 8, 2);

                Assert.IsFalse(r.Read());
            }
        }

        private void AssertPeeking(
            JsonReader reader, JsonToken token, int lineNumber, int columnNumber, JsonToken container)
        {
            var message = $"Peek token {token} at [{lineNumber}, {columnNumber}]";

            Assert.AreEqual(token, reader.PeekNextToken(), message);
            AssertPosition(reader, lineNumber, columnNumber);
            Assert.AreEqual(container, reader.Container, message);
        }

        private void AssertReading(JsonReader reader, JsonToken token,
            bool assertValue, object value, JsonToken container,
            int lineNumber = -1, int columnNumber = -1)
        {
            var message = $"Read token {token} at [{lineNumber}, {columnNumber}]";

            Assert.IsTrue(reader.Read(), message);
            Assert.AreEqual(token, reader.Token, message);

            if (assertValue)
            {
                Assert.AreEqual(value, reader.Value, message);
            }

            Assert.AreEqual(container, reader.Container, message);

            if (lineNumber > 0)
            {
                AssertPosition(reader, lineNumber, columnNumber);
            }
        }

        private void AssertPosition(JsonReader reader, int lineNumber, int columnNumber)
        {
            Assert.AreEqual(lineNumber, reader.LineNumber, $"Assertion of the line number of [{lineNumber}, {columnNumber}].");
            Assert.AreEqual(columnNumber, reader.ColumnNumber, $"Assertion of the column number of [{lineNumber}, {columnNumber}].");
        }

        private void ReadScalarOnly(string json, JsonToken tokenExpected, object valueExpected)
        {
            Console.WriteLine("Testing value {0}", json);
            using (var r = CreateReader(json))
            {
                Assert.IsTrue(r.Read());
                Assert.AreEqual(tokenExpected, r.Token);
                Assert.AreEqual(valueExpected, r.Value);
                Assert.AreEqual(JsonToken.None, r.Container);
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