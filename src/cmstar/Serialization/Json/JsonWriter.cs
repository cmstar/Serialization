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
using System.Globalization;
using System.IO;

namespace cmstar.Serialization.Json
{
    /// <summary>
    /// Represents a writer that provides forward-only means of generating JSON data.
    /// </summary>
    public class JsonWriter : IDisposable
    {
        /// <summary>
        /// The internal <see cref="TextWriter"/> which the JSON is written to.
        /// </summary>
        protected readonly TextWriter Writer;

        private bool _escapeSolidus = true;
        private bool _autoCloseInternalWriter = true;

        /// <summary>
        /// Initializes a new instance of <see cref="JsonWriter"/> with the given 
        /// instance of <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">
        /// The instance of <see cref="TextWriter"/> which the JSON is written to.
        /// </param>
        public JsonWriter(TextWriter writer)
        {
            ArgAssert.NotNull(writer, "writer");
            Writer = writer;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to close the internal 
        /// <see cref="TextWriter"/> when the <see cref="JsonWriter.Dispose"/> method is called.
        /// The default value is <c>true</c>.
        /// </summary>
        public bool AutoCloseInternalWriter
        {
            get { return _autoCloseInternalWriter; }
            set { _autoCloseInternalWriter = value; }
        }

        /// <summary>
        /// Indicates whether to escape the soliduses('/') in strings (to '\/').
        /// The default value is <c>true</c>.
        /// </summary>
        public bool EscapeSolidus
        {
            get { return _escapeSolidus; }
            set { _escapeSolidus = value; }
        }

        public void Dispose()
        {
            Writer.Flush();

            if (_autoCloseInternalWriter)
            {
                Writer.Dispose();
            }
        }

        /// <summary>
        /// Write a '{' which indicates a beginning of a JSON object.
        /// </summary>
        public virtual void WriteObjectStart()
        {
            Writer.Write('{');
        }

        /// <summary>
        /// Write a '}' which indicates an end of a JSON object.
        /// </summary>
        public virtual void WriteObjectEnd()
        {
            Writer.Write('}');
        }

        /// <summary>
        /// Write a '[' which indicates a beginning of a JSON array.
        /// </summary>
        public virtual void WriteArrayStart()
        {
            Writer.Write('[');
        }

        /// <summary>
        /// Write a ']' which indicates a end of a JSON array.
        /// </summary>
        public virtual void WriteArrayEnd()
        {
            Writer.Write(']');
        }

        /// <summary>
        /// Write a JSON property name like '"name":'.
        /// A <c>null</c> name is treated as an empty string.
        /// </summary>
        /// <param name="name">The property name.</param>
        public virtual void WritePropertyName(string name)
        {
            Writer.Write('"');
            Writer.Write(name);
            Writer.Write("\":");
        }

        /// <summary>
        /// Write a comma for properties or array elements.
        /// </summary>
        public virtual void WriteComma()
        {
            Writer.Write(',');
        }

        /// <summary>
        /// Write a JSON 'undefined'.
        /// </summary>
        public virtual void WriteUndefinedValue()
        {
            Writer.Write("undefined");
        }

        /// <summary>
        /// Write a JSON 'null'.
        /// </summary>
        public virtual void WriteNullValue()
        {
            Writer.Write("null");
        }

        /// <summary>
        /// Write a string value which will not be escaped.
        /// </summary>
        /// <param name="value">The string value.</param>
        public virtual void WriteRawStringValue(string value)
        {
            if (value == null)
            {
                WriteNullValue();
            }
            else
            {
                Writer.Write('"');
                Writer.Write(value);
                Writer.Write('"');
            }
        }

        /// <summary>
        /// Write a string value, the value will be escaped.
        /// </summary>
        /// <param name="value">The string value.</param>
        public virtual void WriteStringValue(string value)
        {
            if (value == null)
            {
                WriteNullValue();
                return;
            }

            Writer.Write('"');
            WriteEscapedStringBody(value);
            Writer.Write('"');
        }

        /// <summary>
        /// Write a boolean value (true/false).
        /// </summary>
        /// <param name="value">The boolean value.</param>
        public virtual void WriteBooleanValue(bool value)
        {
            Writer.Write(value ? "true" : "false");
        }

        /// <summary>
        /// Write a string value that represents a number.
        /// NULL will lead to a JSON 'null'.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public virtual void WriteNumberValue(string value)
        {
            if (value == null)
            {
                WriteNullValue();
            }
            else
            {
                Writer.Write(value);
            }
        }

        public virtual void WriteNumberValue(byte number)
        {
            Writer.Write(number.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumberValue(short number)
        {
            Writer.Write(number.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumberValue(int number)
        {
            Writer.Write(number.ToString(CultureInfo.InvariantCulture));
        }

        [CLSCompliant(false)]
        public virtual void WriteNumberValue(uint number)
        {
            Writer.Write(number.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumberValue(long number)
        {
            Writer.Write(number.ToString(CultureInfo.InvariantCulture));
        }

        [CLSCompliant(false)]
        public virtual void WriteNumberValue(ulong number)
        {
            Writer.Write(number.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumberValue(float number)
        {
            Writer.Write(number.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumberValue(double number)
        {
            Writer.Write(number.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumberValue(decimal number)
        {
            Writer.Write(number.ToString(CultureInfo.InvariantCulture));
        }

        private void WriteEscapedStringBody(string value)
        {
            int len = 0;
            int position = 0;
            for (int i = 0; i < value.Length; )
            {
                char c = value[i];
                string escaped = null;
                switch (c)
                {
                    case '"':
                        escaped = @"\""";
                        break;

                    case '/':
                        if (_escapeSolidus)
                        {
                            escaped = @"\/";
                        }
                        else
                        {
                            len++;
                        }
                        break;

                    case '\\':
                        escaped = @"\\";
                        break;

                    case '\n':
                        escaped = @"\n";
                        break;

                    case '\r':
                        escaped = @"\r";
                        break;

                    case '\t':
                        escaped = @"\t";
                        break;

                    case '\b':
                        escaped = @"\b";
                        break;

                    case '\f':
                        escaped = @"\f";
                        break;

                    default:
                        len++;
                        break;
                }

                i++;

                if (escaped != null)
                {
                    if (len > 0)
                    {
                        Writer.Write(value.ToCharArray(position, len));
                    }

                    Writer.Write(escaped);
                    position = i;
                    len = 0;
                }
            }

            if (len == value.Length)
            {
                Writer.Write(value);
            }
            else if (len > 0)
            {
                Writer.Write(value.ToCharArray(position, len));
            }
        }
    }
}
