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

namespace cmstar.Serialization
{
    [TestFixture]
    public class IndentedTextWriterTests
    {
        [Test]
        public void WriteWithFormattingNone()
        {
            var sb = new StringBuilder();
            var writer = new IndentedTextWriter(new StringWriter(sb), string.Empty);
            writer.NewLine = string.Empty;

            writer.WriteLine(123);
            writer.Write("value");
            writer.WriteLine(1.99);

            writer.IndentIncrease();
            writer.WriteLine(123);
            writer.Write("value");
            writer.WriteLine(1.99);

            writer.IndentDecrease();
            writer.WriteLine(123);
            writer.Write("value");
            writer.WriteLine(1.99);

            var expected = "123value1.99123value1.99123value1.99";
            Assert.AreEqual(expected, sb.ToString());
        }

        [Test]
        public void WriteWithFormattingMultiple()
        {
            var sb = new StringBuilder();
            var writer = new IndentedTextWriter(new StringWriter(sb), string.Empty);
            
            writer.Write("<");
            writer.Write("class");
            writer.WriteLine(">");

            writer.IndentIncrease();
            writer.Write("<numberValue int='");
            writer.Write(123);
            writer.Write("' float='");
            writer.Write(1.99);
            writer.WriteLine("'>");

            writer.IndentIncrease();
            writer.IndentIncrease();
            writer.Write("<stringValue>");
            writer.Write("string");
            writer.WriteLine("</stringValue>");

            writer.IndentDecrease();
            writer.WriteLine("</numberValue>");

            writer.IndentDecrease();
            writer.IndentDecrease();
            writer.WriteLine("</class>");

            var expected =
@"<class>
<numberValue int='123' float='1.99'>
<stringValue>string</stringValue>
</numberValue>
</class>
";
            Assert.AreEqual(expected, sb.ToString());
        }

        [Test]
        public void WriteWithFormattingIndented()
        {
            var sb = new StringBuilder();
            var writer = new IndentedTextWriter(new StringWriter(sb), "  ");

            writer.IndentMark = "  ";
            writer.Write("<");
            writer.Write("class");
            writer.WriteLine(">");

            writer.IndentIncrease();
            writer.Write("<numberValue int='");
            writer.Write(123);
            writer.Write("' float='");
            writer.Write(1.99);
            writer.WriteLine("'>");

            writer.IndentIncrease();
            writer.IndentIncrease();
            writer.Write("<stringValue>");
            writer.Write("string");
            writer.WriteLine("</stringValue>");

            writer.IndentDecrease();
            writer.WriteLine("</numberValue>");

            writer.IndentDecrease();
            writer.IndentDecrease();
            writer.WriteLine("</class>");

            var expected =
@"<class>
  <numberValue int='123' float='1.99'>
      <stringValue>string</stringValue>
    </numberValue>
</class>
";
            Assert.AreEqual(expected, sb.ToString());
        }

        [Test]
        public void WriteEmptyString()
        {
            var sb = new StringBuilder();
            var writer = new IndentedTextWriter(new StringWriter(sb), string.Empty);
            writer.NewLine = string.Empty;
            writer.Write(string.Empty);
            Assert.AreEqual(string.Empty, sb.ToString());

            sb = new StringBuilder();
            writer = new IndentedTextWriter(new StringWriter(sb), string.Empty);
            writer.Write(string.Empty);
            Assert.AreEqual(string.Empty, sb.ToString());

            sb = new StringBuilder();
            writer = new IndentedTextWriter(new StringWriter(sb));
            writer.Write(string.Empty);
            Assert.AreEqual(string.Empty, sb.ToString());
        }

        [Test]
        public void WriteEmptyLine()
        {
            var sb = new StringBuilder();
            var writer = new IndentedTextWriter(new StringWriter(sb), string.Empty);
            writer.NewLine = string.Empty;
            writer.WriteLine();
            Assert.AreEqual(string.Empty, sb.ToString());

            sb = new StringBuilder();
            writer = new IndentedTextWriter(new StringWriter(sb), string.Empty);
            writer.WriteLine();
            Assert.AreEqual(Environment.NewLine, sb.ToString());

            sb = new StringBuilder();
            writer = new IndentedTextWriter(new StringWriter(sb));
            writer.WriteLine();
            Assert.AreEqual(Environment.NewLine, sb.ToString());
        }
    }
}