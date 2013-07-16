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
using System.Text;

namespace cmstar.Serialization.Json
{
    /// <summary>
    /// The exception was thrown when the JSON format is not illegal.
    /// </summary>
    public class JsonFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFormatException"/> with 
        /// with the given message that describes the error.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public JsonFormatException(string message)
            : this(message, -1, -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFormatException"/> with 
        /// with the given message that describes the error and specify the 
        /// position of the error in the JSON.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="lineNumber">
        /// The line number where the format error was found in the JSON.
        /// </param>
        /// <param name="columnNumber">
        /// The column number where the format error was found in the JSON.
        /// </param>
        public JsonFormatException(string message, int lineNumber, int columnNumber)
            : this(message, lineNumber, columnNumber, JsonToken.None, JsonToken.None, JsonToken.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFormatException"/> with 
        /// with the given message that describes the error  and the 
        /// <see cref="JsonToken"/>s of the current state.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="currentToken">
        /// The <see cref="JsonToken"/> which was being read or written while 
        /// the exception was thrown.
        /// </param>
        /// <param name="lastToken">
        /// The lastest <see cref="JsonToken"/> before the <paramref name="currentToken"/>.
        /// </param>
        /// <param name="containerToken">
        /// The <see cref="JsonToken"/> represents the JSON container in which 
        /// the <see cref="CurrentToken"/> was being read or written.
        /// </param>
        public JsonFormatException(string message,
            JsonToken currentToken, JsonToken lastToken, JsonToken containerToken)
            : this(message, -1, -1, currentToken, lastToken, containerToken)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFormatException"/> with 
        /// with the given message that describes the error, the position of the error
        /// in the JSON, and the <see cref="JsonToken"/>s of the current state.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="lineNumber">
        /// The line number where the format error was found in the JSON.
        /// </param>
        /// <param name="columnNumber">
        /// The column number where the format error was found in the JSON.
        /// </param>
        /// <param name="currentToken">
        /// The <see cref="JsonToken"/> which was being read or written while 
        /// the exception was thrown.
        /// </param>
        /// <param name="lastToken">
        /// The lastest <see cref="JsonToken"/> before the <paramref name="currentToken"/>.
        /// </param>
        /// <param name="containerToken">
        /// The <see cref="JsonToken"/> represents the JSON container in which 
        /// the <see cref="CurrentToken"/> was being read or written.
        /// </param>
        public JsonFormatException(string message, int lineNumber, int columnNumber,
            JsonToken currentToken, JsonToken lastToken, JsonToken containerToken)
            : base(message)
        {
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
            CurrentToken = currentToken;
            LastToken = lastToken;
            ContainerToken = containerToken;
        }

        /// <summary>
        /// Gets the line number where the format error was found in the JSON.
        /// Returns -1 if not specfied.
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Gets the column number where the format error was found in the JSON.
        /// Returns -1 if not specfied.
        /// </summary>
        public int ColumnNumber { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="JsonToken"/> which was being read or written
        /// while the exception was thrown.
        /// Returns <see cref="JsonToken.None"/> if not specified.
        /// </summary>
        public JsonToken CurrentToken { get; private set; }

        /// <summary>
        /// Gets or sets the lastest <see cref="JsonToken"/> before the <see cref="CurrentToken"/>.
        /// Returns <see cref="JsonToken.None"/> if not specified.
        /// </summary>
        public JsonToken LastToken { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="JsonToken"/> represents the JSON container
        /// in which the <see cref="CurrentToken"/> was being read or written.
        /// Returns <see cref="JsonToken.None"/> if not specified.
        /// </summary>
        public JsonToken ContainerToken { get; private set; }

        public override string Message
        {
            get
            {
                var msgBuilder = new StringBuilder();
                msgBuilder.AppendLine(base.Message);

                if (LineNumber > 0 || ColumnNumber > 0)
                {
                    msgBuilder.AppendFormat(
                        "Position: line {0} column {1}.", LineNumber, ColumnNumber);
                    msgBuilder.AppendLine();
                }

                string containerDescription;
                switch (ContainerToken)
                {
                    case JsonToken.ArrayStart:
                        containerDescription = "Array";
                        break;

                    case JsonToken.ObjectStart:
                        containerDescription = "Object";
                        break;

                    default:
                        containerDescription = "None";
                        break;
                }
                msgBuilder.AppendFormat(
                    "The current token: {0}, the last token: {1}, the container: {2}.",
                    CurrentToken, LastToken, containerDescription);

                return msgBuilder.ToString();
            }
        }
    }
}
