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
using System.Text;
using NUnit.Framework;

namespace cmstar.Serialization.Json
{
    [TestFixture]
    public class JsonWriterImprovedTests : JsonWriterTests
    {
        [Test]
        public void CommaStateInArray()
        {
            var sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                //simple
                w.WriteArrayStart();
                Assert.Throws<JsonFormatException>(w.WriteComma);

                w.WriteNullValue();
                w.WriteComma();
                Assert.Throws<JsonFormatException>(w.WriteComma);

                w.WriteNumberValue(123);
                w.WriteComma();
                Assert.Throws<JsonFormatException>(w.WriteComma);

                w.WriteNullValue();
                w.WriteArrayEnd();
                Assert.Throws<JsonFormatException>(w.WriteComma);
                Console.WriteLine(sb.ToString());
            }

            // embedded array
            sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.WriteArrayStart();

                w.WriteArrayStart();
                Assert.Throws<JsonFormatException>(w.WriteComma);
                w.WriteArrayEnd();
                w.WriteComma();
                Assert.Throws<JsonFormatException>(w.WriteComma);

                w.WriteObjectStart();
                Assert.Throws<JsonFormatException>(w.WriteComma);
                w.WriteObjectEnd();
                w.WriteArrayEnd();
                Assert.Throws<JsonFormatException>(w.WriteComma);
                Console.WriteLine(sb.ToString());
            }
        }

        [Test]
        public void CommaStateOfScalar()
        {
            var sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                //empty
                Assert.Throws<JsonFormatException>(w.WriteComma);

                //primitive
                w.WriteNullValue();
                Assert.Throws<JsonFormatException>(w.WriteComma);
            }
        }

        [Test]
        public void CommaStateInObject()
        {
            var sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.WriteObjectStart();
                w.WritePropertyName("p");
                Assert.Throws<JsonFormatException>(w.WriteComma);
                w.WriteNumberValue(123);
                w.WriteComma();
                Assert.Throws<JsonFormatException>(w.WriteComma);
                w.WritePropertyName("p2");
                Assert.Throws<JsonFormatException>(w.WriteComma);
                w.WriteNullValue();
                w.WriteObjectEnd();
                Assert.Throws<JsonFormatException>(w.WriteComma);
                Console.WriteLine(sb.ToString());
            }
        }

        [Test]
        public void WriteSimpleArrayIndented()
        {
            var sb = new StringBuilder();
            var w = CreateWriterWithFormattingIndented(sb);
            w.WriteArrayStart();

            w.WriteNumberValue(123);
            w.WriteComma();

            w.WriteStringValue("string");
            w.WriteComma();

            w.WriteNumberValue(321);
            w.WriteArrayEnd();

            var expected =
@"[
    123,
    ""string"",
    321
]";
            var result = sb.ToString();
            Console.WriteLine(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void WriteEmbeddedArray()
        {
            var sb = new StringBuilder();
            var w = CreateWriterWithFormattingIndented(sb);
            w.WriteArrayStart();

            w.WriteArrayStart();
            w.WriteArrayEnd();
            w.WriteComma();

            w.WriteArrayStart();
            w.WriteArrayEnd();
            w.WriteComma();

            w.WriteArrayStart();
            w.WriteArrayEnd();
            w.WriteComma();

            w.WriteArrayStart();
            w.WriteArrayEnd();
            w.WriteArrayEnd();

            var expected =
@"[
    [
    ],[
    ],[
    ],[
    ]
]";
            var result = sb.ToString();
            Console.WriteLine(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void WriteSimpleOjbectIndented()
        {
            var sb = new StringBuilder();
            var w = CreateWriterWithFormattingIndented(sb);
            w.WriteObjectStart();

            w.WritePropertyName("intValue");
            w.WriteNumberValue(123);
            w.WriteComma();

            w.WritePropertyName("stringValue");
            w.WriteStringValue("Hello\r\nWorld!");
            w.WriteComma();

            w.WritePropertyName("doubleValue");
            w.WriteNumberValue(123.321);
            w.WriteObjectEnd();

            var expected =
@"{
    ""intValue"":123,
    ""stringValue"":""Hello\r\nWorld!"",
    ""doubleValue"":123.321
}";
            var result = sb.ToString();
            Console.WriteLine(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void WriteArrayWithObjectElements()
        {
            var sb = new StringBuilder();
            var w = CreateWriterWithFormattingIndented(sb);
            w.WriteArrayStart();
            for (int i = 0; i < 2; i++)
            {
                if (i > 0)
                    w.WriteComma();

                w.WriteArrayStart();

                w.WriteObjectStart();
                w.WriteObjectEnd();
                w.WriteComma();

                w.WriteObjectStart();
                w.WritePropertyName("name");
                w.WriteStringValue("v1");
                w.WriteObjectEnd();
                w.WriteComma();

                w.WriteObjectStart();
                w.WriteObjectEnd();
                w.WriteComma();

                w.WriteObjectStart();
                w.WritePropertyName("name");
                w.WriteStringValue("v2");
                w.WriteObjectEnd();
                w.WriteArrayEnd();
            }
            w.WriteArrayEnd();

            var expected =
@"[
    [
        {
        },{
            ""name"":""v1""
        },{
        },{
            ""name"":""v2""
        }
    ],[
        {
        },{
            ""name"":""v1""
        },{
        },{
            ""name"":""v2""
        }
    ]
]";
            var result = sb.ToString();
            Console.WriteLine(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void WriteComplexJson()
        {
            var sb = new StringBuilder();
            var w = CreateWriterWithFormattingIndented(sb);
            w.WriteObjectStart();

            w.WritePropertyName("intProp");
            w.WriteNumberValue(123);
            w.WriteComma();

            w.WritePropertyName("floatProp");
            w.WriteNumberValue(1.116);
            w.WriteComma();

            w.WritePropertyName("nullProp");
            w.WriteNullValue();
            w.WriteComma();

            w.WritePropertyName("stringProp");
            w.WriteStringValue("stringValue");
            w.WriteComma();

            //array
            w.WritePropertyName("arrayProp");
            w.WriteArrayStart();

            w.WriteStringValue("s");
            w.WriteComma();
            w.WriteStringValue("1\t2");

            w.WriteArrayEnd();
            w.WriteComma();
            //

            w.WritePropertyName("escapedStringProp");
            w.WriteStringValue("stringValue\tV\r\nsay \"hello\".");
            w.WriteComma();

            w.WritePropertyName("emptyArray");
            w.WriteArrayStart();
            w.WriteArrayEnd();
            w.WriteObjectEnd();

            var expected =
@"{
    ""intProp"":123,
    ""floatProp"":1.116,
    ""nullProp"":null,
    ""stringProp"":""stringValue"",
    ""arrayProp"":[
        ""s"",
        ""1\t2""
    ],
    ""escapedStringProp"":""stringValue\tV\r\nsay \""hello\""."",
    ""emptyArray"":[
    ]
}";
            var result = sb.ToString();
            Console.WriteLine(result);
            Assert.AreEqual(expected, result);
        }

        protected override JsonWriter CreateWriterWithoutFormatting(StringBuilder stringBuilder)
        {
            var indentedTextWriter = new IndentedTextWriter(new StringWriter(stringBuilder), string.Empty);
            indentedTextWriter.NewLine = string.Empty;
            return new JsonWriterImproved(indentedTextWriter);
        }

        private JsonWriterImproved CreateWriterWithFormattingIndented(StringBuilder stringBuilder)
        {
            var indentedTextWriter = new IndentedTextWriter(new StringWriter(stringBuilder));
            var writer = new JsonWriterImproved(indentedTextWriter);
            return writer;
        }
    }
}