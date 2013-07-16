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

using System.IO;
using System.Text;
using NUnit.Framework;

namespace cmstar.Serialization.Json
{
    [TestFixture]
    public class JsonWriterTests
    {
        [Test]
        public void WriteEmptyObject()
        {
            var sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.WriteObjectStart();
                w.WriteObjectEnd();
                Assert.AreEqual("{}", sb.ToString());
            }
        }

        [Test]
        public void WriteEmptyArray()
        {
            var sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.WriteArrayStart();
                w.WriteArrayEnd();
                Assert.AreEqual("[]", sb.ToString());
            }
        }

        [Test]
        public void WriteString()
        {
            var sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.WriteStringValue("123456");
                Assert.AreEqual(@"""123456""", sb.ToString());
            }

            sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.WriteStringValue("\r\n\t");
                Assert.AreEqual(@"""\r\n\t""", sb.ToString());
            }

            sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.WriteStringValue("\t\b123");
                Assert.AreEqual(@"""\t\b123""", sb.ToString());
            }

            sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.WriteStringValue("123\r\n");
                Assert.AreEqual(@"""123\r\n""", sb.ToString());
            }

            sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.WriteStringValue("\t12\t3\"4\f\t");
                Assert.AreEqual(@"""\t12\t3\""4\f\t""", sb.ToString());
            }

            sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.WriteStringValue(@"\/\/");
                Assert.AreEqual(@"""\\\/\\\/""", sb.ToString());
            }

            sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
                w.EscapeSolidus = false;
                w.WriteStringValue(@"\/\/");
                Assert.AreEqual(@"""\\/\\/""", sb.ToString());
            }
        }

        [Test]
        public void WriteFullJson()
        {
            var sb = new StringBuilder();
            using (var w = CreateWriterWithoutFormatting(sb))
            {
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

                w.WriteObjectEnd();

                var expected = "{`intProp`:123,`floatProp`:1.116,`nullProp`:null,`stringProp`:`stringValue`,"
                    + @"`arrayProp`:[`s`,`1\t2`],`escapedStringProp`:`stringValue\tV\r\nsay \`hello\`.`}";
                expected = expected.Replace("`", "\"");
                Assert.AreEqual(expected, sb.ToString());
            }
        }

        protected virtual JsonWriter CreateWriterWithoutFormatting(StringBuilder stringBuilder)
        {
            var indentedTextWriter = new IndentedTextWriter(new StringWriter(stringBuilder), string.Empty);
            indentedTextWriter.NewLine = string.Empty;
            return new JsonWriter(indentedTextWriter);
        }
    }
}

