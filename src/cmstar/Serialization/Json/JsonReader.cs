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
using System.Text;

namespace cmstar.Serialization.Json
{
    /// <summary>
    /// A reader for reading a JSON from a <see cref="TextReader"/>, 
    /// the reading action is forward-only.
    /// While reading a Json, the <see cref="JsonReader"/> will validate the format,
    /// and throws <see cref="JsonFormatException"/> if the format is not correct.
    /// </summary>
    public class JsonReader : IDisposable
    {
        private readonly TextReader _reader;
        private readonly JsonTokenStack _containerTokenStack = new JsonTokenStack();
        private int _lineNumber = 1;
        private int _columnNumber = 1;
        private bool _autoCloseInternalReader = true;

        /// <summary>
        /// Initializes a instance of <see cref="JsonReader"/> 
        /// and specified the internal instance of <see cref="TextReader"/>.
        /// </summary>
        /// <param name="textReader">
        /// The instance of <see cref="TextReader"/> whith the JSON text is read from.
        /// </param>
        public JsonReader(TextReader textReader)
        {
            ArgAssert.NotNull(textReader, "textReader");
            _reader = textReader;
        }

        /// <summary>
        /// Gets the line number of current position 
        /// the <see cref="JsonReader"/> reached in the JSON.
        /// </summary>
        public int LineNumber
        {
            get
            {
                return _lineNumber;
            }
        }

        /// <summary>
        /// Gets the column number of current position 
        /// the <see cref="JsonReader"/> reached in the JSON.
        /// </summary>
        public int ColumnNumber
        {
            get
            {
                return _columnNumber;
            }
        }

        /// <summary>
        /// Gets the <see cref="JsonToken"/> finally read.
        /// The the <see cref="JsonReader"/> has not started the read or has reached the end of the Json,
        /// <see cref="JsonToken.None"/> would be returned.
        /// </summary>
        public JsonToken Token { get; private set; }

        /// <summary>
        /// Gets the <see cref="JsonToken"/> that indicates which JSON container the current position is in.
        /// </summary>
        public JsonToken Containter
        {
            get
            {
                return _containerTokenStack.Top;
            }
        }

        /// <summary>
        /// Gets the value finally read.
        /// </summary>
        /// <remarks>
        /// It's a <see cref="bool"/> if the value of the <see cref="Token"/> property is <see cref="JsonToken.BooleanValue"/>:
        /// and a <see cref="string"/> for <see cref="JsonToken.StringValue"/>;
        /// a <see cref="double"/> for <see cref="JsonToken.NumberValue"/>;
        /// <c>NULL</c> for <see cref="JsonToken.NullValue"/> and <see cref="JsonToken.UndefinedValue"/>.
        /// 
        /// If the <see cref="Token"/> property is <see cref="JsonToken.PropertyName"/>,
        /// the value is the propery name (a string).
        ///  
        /// In other case, the value has no meaning.
        /// </remarks>
        public object Value { get; private set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to close the internal 
        /// <see cref="TextReader"/> when the <see cref="JsonReader.Dispose"/> method is called.
        /// The default value is <c>true</c>.
        /// </summary>
        public bool AutoCloseInternalReader
        {
            get { return _autoCloseInternalReader; }
            set { _autoCloseInternalReader = value; }
        }

        public void Dispose()
        {
            if (_autoCloseInternalReader)
            {
                _reader.Dispose();
            }
        }

        /// <summary>
        /// Try to determind the next <see cref="JsonToken"/> in the Json
        /// without changing current <see cref="JsonToken"/> and value.
        /// </summary>
        /// <returns>
        /// The next <see cref="JsonToken"/> in the JSON.
        /// Returns <see cref="JsonToken.None"/> if the reader already reached the end.
        /// </returns>
        public JsonToken PeekNextToken()
        {
            var c = PeekNonSpace();
            if (c < 0)
                return JsonToken.None;

            switch (c)
            {
                case '{':
                    return JsonToken.ObjectStart;

                case '}':
                    return JsonToken.ObjectEnd;

                case '[':
                    return JsonToken.ArrayStart;

                case ']':
                    return JsonToken.ArrayEnd;

                case ',':
                    return JsonToken.Comma;

                case '"':
                    if (_containerTokenStack.Top == JsonToken.ObjectStart
                        && (Token == JsonToken.Comma || Token == JsonToken.ObjectStart))
                    {
                        return JsonToken.PropertyName;
                    }
                    else
                    {
                        return JsonToken.StringValue;
                    }

                case 't': //true
                case 'f': //false
                    return JsonToken.BooleanValue;

                case 'n': //null
                    return JsonToken.NullValue;

                case 'u': //undefined
                    return JsonToken.UndefinedValue;

                default:
                    return JsonToken.NumberValue;
            }
        }

        /// <summary>
        /// Reads next token from the JSON.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a token was read; 
        /// otherwise <c>false</c>, it means the <see cref="JsonReader"/> reached the end of the JSON.
        /// </returns>
        public bool Read()
        {
            int c = NextNonSpace();
            if (c < 0)
            {
                if (_containerTokenStack.Count != 0 || Token == JsonToken.Comma)
                    throw FormatError("The JSON does not end correctly.", JsonToken.None);

                Token = JsonToken.None;
                return false;
            }

            switch (c)
            {
                case '{':
                    ReachObjectStart();
                    break;

                case '}':
                    ReachObjectEnd();
                    break;

                case '[':
                    ReachArrayStart();
                    break;

                case ']':
                    ReachArrayEnd();
                    break;

                case ',':
                    ReachComma();
                    break;

                case '"':
                    if (_containerTokenStack.Top == JsonToken.ObjectStart
                        && (Token == JsonToken.Comma || Token == JsonToken.ObjectStart))
                    {
                        ReadPropertyName();
                    }
                    else
                    {
                        ReadStringValue();
                    }
                    break;

                default:
                    ReadNonStringValue(c);
                    break;
            }
            return true;
        }

        private void ReachObjectStart()
        {
            ValidateTokenState(JsonToken.ObjectStart);

            _containerTokenStack.Push(JsonToken.ObjectStart);
            Token = JsonToken.ObjectStart;
        }

        private void ReachObjectEnd()
        {
            ValidateTokenState(JsonToken.ObjectEnd);

            _containerTokenStack.Pop();
            Token = JsonToken.ObjectEnd;
        }

        private void ReachArrayStart()
        {
            ValidateTokenState(JsonToken.ArrayStart);

            _containerTokenStack.Push(JsonToken.ArrayStart);
            Token = JsonToken.ArrayStart;
        }

        private void ReachArrayEnd()
        {
            ValidateTokenState(JsonToken.ArrayEnd);

            _containerTokenStack.Pop();
            Token = JsonToken.ArrayEnd;
        }

        private void ReachComma()
        {
            ValidateTokenState(JsonToken.Comma);

            Token = JsonToken.Comma;
        }

        private void ReadPropertyName()
        {
            ValidateTokenState(JsonToken.PropertyName);

            var value = ParseString();

            //check there's a ':' follows the property name
            var next = NextNonSpace();
            if (next != ':')
                throw FormatError("Incorrect propery name format.", JsonToken.PropertyName);

            Value = value;
            Token = JsonToken.PropertyName;
        }

        private void ReadNonStringValue(int firstChar)
        {
            switch (firstChar)
            {
                case 't': //true
                    MatchConstantString("rue", JsonToken.BooleanValue);
                    ValidateTokenState(JsonToken.BooleanValue);
                    Value = true;
                    Token = JsonToken.BooleanValue;
                    break;

                case 'f': //false
                    MatchConstantString("alse", JsonToken.BooleanValue);
                    ValidateTokenState(JsonToken.BooleanValue);
                    Value = false;
                    Token = JsonToken.BooleanValue;
                    break;

                case 'n': //null
                    MatchConstantString("ull", JsonToken.NullValue);
                    ValidateTokenState(JsonToken.NullValue);
                    Value = null;
                    Token = JsonToken.NullValue;
                    break;

                case 'u': //undefined
                    MatchConstantString("ndefined", JsonToken.UndefinedValue);
                    ValidateTokenState(JsonToken.UndefinedValue);
                    Value = null;
                    Token = JsonToken.UndefinedValue;
                    break;

                default:
                    var number = ParseNumber(firstChar);
                    ValidateTokenState(JsonToken.NumberValue);
                    Value = number;
                    Token = JsonToken.NumberValue;
                    break;
            }
        }

        private void ReadStringValue()
        {
            ValidateTokenState(JsonToken.StringValue);

            Value = ParseString();
            Token = JsonToken.StringValue;
        }

        private double ParseNumber(int firstChar)
        {
            switch (firstChar)
            {
                case '-':
                    if (_reader.Peek() == 'I')
                    {
                        MatchConstantString("Infinity", JsonToken.NumberValue);
                        return double.NegativeInfinity;
                    }
                    return ParseRealNumber('-');

                case 'I': //Infinity
                    MatchConstantString("nfinity", JsonToken.NumberValue);
                    return double.PositiveInfinity;

                case 'N': //NaN
                    MatchConstantString("aN", JsonToken.NumberValue);
                    return double.NaN;

                default:
                    return ParseRealNumber(firstChar);
            }
        }

        private double ParseRealNumber(int firstChar)
        {
            var buffer = new StringBuilder();
            buffer.Append((char)firstChar);

            while (true)
            {
                var next = _reader.Peek();
                if (next < 0)
                    break;

                var match = false;
                switch (next)
                {
                    case '.':
                    case '+':
                    case '-':
                    case 'e':
                    case 'E':
                        match = true;
                        break;

                    default:
                        if (next >= '0' && next <= '9')
                            match = true;

                        break;
                }

                if (!match)
                    break;

                buffer.Append((char)next);
                Next();
            }

            double number;
            if (!double.TryParse(buffer.ToString(), out number))
                throw FormatError("Incorrect number format.", JsonToken.NumberValue);

            return number;
        }

        private string ParseString()
        {
            var escaped = false;
            var buffer = new StringBuilder();
            while (true)
            {
                int c = Next();
                if (c < 0)
                    throw FormatError("The string value does not end correctly.", JsonToken.StringValue);

                if (escaped)
                {
                    switch (c)
                    {
                        case '"':
                        case '\\':
                        case '/':
                            buffer.Append((char)c);
                            break;

                        case 'b':
                            buffer.Append('\b');
                            break;

                        case 'f':
                            buffer.Append('\f');
                            break;

                        case 'n':
                            buffer.Append('\n');
                            break;

                        case 'r':
                            buffer.Append('\r');
                            break;

                        case 't':
                            buffer.Append('\t');
                            break;

                        case 'u':
                            buffer.Append(ReadStringInternalUnicodeCharacter());
                            break;

                        default:
                            var msg = string.Format("Illegal escaped character \"\\{0}\".", (char)c);
                            throw FormatError(msg, JsonToken.StringValue);
                    }

                    escaped = false;
                    continue;
                }

                if (c == '"')
                    return buffer.ToString();

                if (c == '\\')
                {
                    escaped = true;
                }
                else
                {
                    buffer.Append((char)c);
                }
            }
        }

        private char ReadStringInternalUnicodeCharacter()
        {
            var hex = new char[4];
            for (int i = 0; i < 4; i++)
            {
                var c = Next();
                if (c < 0)
                    throw FormatError("The string value does not end correctly.", JsonToken.StringValue);

                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                {
                    hex[i] = (char)c;
                }
                else
                {
                    var msg = string.Format(
                        "Illegal character \"{0}\", a hexadecimal digit is needed.", (char)c);
                    throw FormatError(msg, JsonToken.StringValue);
                }
            }

            return Convert.ToChar(int.Parse(new string(hex), NumberStyles.HexNumber));
        }

        private void MatchConstantString(string expected, JsonToken tokenReached)
        {
            foreach (var c in expected)
            {
                if (c != Next())
                    throw FormatError("Incorrect JSON value.", tokenReached);
            }
        }

        private int Next()
        {
            int value = _reader.Read();
            if (value >= 0)
            {
                if (value == '\n' || (value == '\r' && _reader.Peek() != '\n'))
                {
                    //switch to a new line
                    _columnNumber = 1;
                    _lineNumber++;
                }
                else
                {
                    _columnNumber++;
                }
            }
            return value;
        }

        private int NextNonSpace()
        {
            while (true)
            {
                int next = Next();
                if (next < 0 || !char.IsWhiteSpace((char)next))
                    return next;
            }
        }

        private int PeekNonSpace()
        {
            while (true)
            {
                int next = _reader.Peek();
                if (next < 0 || !char.IsWhiteSpace((char)next))
                    return next;

                Next();
            }
        }

        private void ValidateTokenState(JsonToken tokenReached)
        {
            if (JsonTokenValidator.Validate(tokenReached, Token, _containerTokenStack.Top))
                return;

            throw FormatError("Illegal token reached.", tokenReached);
        }

        private JsonFormatException FormatError(string message, JsonToken tokenReached)
        {
            return new JsonFormatException(
                message, LineNumber, ColumnNumber, tokenReached, Token, _containerTokenStack.Top);
        }
    }
}
