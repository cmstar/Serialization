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

namespace cmstar.Serialization
{
    /// <summary>
    /// Provides a text writer that can indent new lines.
    /// </summary>
    public class IndentedTextWriter : TextWriter
    {
        /// <summary>
        /// The max indent level allowed in the <see cref="IndentedTextWriter"/>.
        /// </summary>
        public const int MaxIndentLevel = 16;

        /// <summary>
        /// The default string for indentations.
        /// </summary>
        public const string DefaultIndentMark = "    ";

        private readonly TextWriter _writer;
        private string _indentMark;
        private int _indentLevel;
        private bool _isLineStart = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentedTextWriter"/> 
        /// using the specified text writer and default indent mark.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> for output.</param>
        public IndentedTextWriter(TextWriter textWriter)
            : this(textWriter, DefaultIndentMark)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentedTextWriter"/> 
        /// using the specified text writer and indent mark.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> for output.</param>
        /// <param name="indentMark">The indent mark used for indentations.</param>
        public IndentedTextWriter(TextWriter textWriter, string indentMark)
        {
            ArgAssert.NotNull(textWriter, "textWriter");

            _writer = textWriter;
            _indentMark = (indentMark ?? string.Empty);
        }

        public override Encoding Encoding
        {
            get { return _writer.Encoding; }
        }

        public override string NewLine
        {
            get
            {
                return _writer.NewLine;
            }
            set
            {
                _writer.NewLine = value;
            }
        }

        public override IFormatProvider FormatProvider
        {
            get
            {
                return _writer.FormatProvider;
            }
        }

        /// <summary>
        /// Gets the string used for indentations in the <see cref="IndentedTextWriter"/>¡£
        /// </summary>
        public string IndentMark
        {
            get { return _indentMark; }
            set { _indentMark = value; }
        }

        /// <summary>
        /// Gets the indent level.
        /// </summary>
        public int IndentLevel
        {
            get { return _indentLevel; }
        }

        /// <summary>
        /// Increases the indent level by 1.
        /// The indent level will not change if already reached the <see cref="MaxIndentLevel"/>.
        /// </summary>
        public void IndentIncrease()
        {
            if (_indentLevel <= MaxIndentLevel)
                _indentLevel++;
        }

        /// <summary>
        /// Decreases the indent level by 1.
        /// The indent level will not change if it is zero.
        /// </summary>
        public void IndentDecrease()
        {
            if (_indentLevel > 0)
                _indentLevel--;
        }

        public override void Close()
        {
            _writer.Close();
        }

        public override void Flush()
        {
            _writer.Flush();
        }

        public override void Write(string s)
        {
            WriteIndentMarks();
            _writer.Write(s);
        }

        public override void Write(bool value)
        {
            WriteIndentMarks();
            _writer.Write(value);
        }

        public override void Write(char value)
        {
            WriteIndentMarks();
            _writer.Write(value);
        }

        public override void Write(char[] buffer)
        {
            WriteIndentMarks();
            _writer.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            WriteIndentMarks();
            _writer.Write(buffer, index, count);
        }

        public override void Write(double value)
        {
            WriteIndentMarks();
            _writer.Write(value);
        }

        public override void Write(float value)
        {
            WriteIndentMarks();
            _writer.Write(value);
        }

        public override void Write(int value)
        {
            WriteIndentMarks();
            _writer.Write(value);
        }

        public override void Write(long value)
        {
            WriteIndentMarks();
            _writer.Write(value);
        }

        public override void Write(object value)
        {
            WriteIndentMarks();
            _writer.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            WriteIndentMarks();
            _writer.Write(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            WriteIndentMarks();
            _writer.Write(format, arg0, arg1);
        }

        public override void Write(string format, params object[] arg)
        {
            WriteIndentMarks();
            _writer.Write(format, arg);
        }


        public override void WriteLine(string s)
        {
            WriteIndentMarks();
            _writer.WriteLine(s);
            _isLineStart = true;
        }

        public override void WriteLine()
        {
            WriteIndentMarks();
            _writer.WriteLine();
            _isLineStart = true;
        }

        public override void WriteLine(bool value)
        {
            WriteIndentMarks();
            _writer.WriteLine(value);
            _isLineStart = true;
        }

        public override void WriteLine(char value)
        {
            WriteIndentMarks();
            _writer.WriteLine(value);
            _isLineStart = true;
        }

        public override void WriteLine(char[] buffer)
        {
            WriteIndentMarks();
            _writer.WriteLine(buffer);
            _isLineStart = true;
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            WriteIndentMarks();
            _writer.WriteLine(buffer, index, count);
            _isLineStart = true;
        }

        public override void WriteLine(double value)
        {
            WriteIndentMarks();
            _writer.WriteLine(value);
            _isLineStart = true;
        }

        public override void WriteLine(float value)
        {
            WriteIndentMarks();
            _writer.WriteLine(value);
            _isLineStart = true;
        }

        public override void WriteLine(int value)
        {
            WriteIndentMarks();
            _writer.WriteLine(value);
            _isLineStart = true;
        }

        public override void WriteLine(long value)
        {
            WriteIndentMarks();
            _writer.WriteLine(value);
            _isLineStart = true;
        }

        public override void WriteLine(object value)
        {
            WriteIndentMarks();
            _writer.WriteLine(value);
            _isLineStart = true;
        }

        public override void WriteLine(string format, object arg0)
        {
            WriteIndentMarks();
            _writer.WriteLine(format, arg0);
            _isLineStart = true;
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            WriteIndentMarks();
            _writer.WriteLine(format, arg0, arg1);
            _isLineStart = true;
        }

        public override void WriteLine(string format, params object[] arg)
        {
            WriteIndentMarks();
            _writer.WriteLine(format, arg);
            _isLineStart = true;
        }

        [CLSCompliant(false)]
        public override void WriteLine(uint value)
        {
            WriteIndentMarks();
            _writer.WriteLine(value);
            _isLineStart = true;
        }

        private void WriteIndentMarks()
        {
            if (_isLineStart)
            {
                for (var i = 0; i < _indentLevel; i++)
                    _writer.Write(_indentMark);
            }

            _isLineStart = false;
        }
    }
}