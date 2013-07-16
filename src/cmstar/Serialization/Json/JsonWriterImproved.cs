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

namespace cmstar.Serialization.Json
{
    /// <summary>
    /// Represents a writer that provides non-cached and forward-only means of 
    /// generating JSON data.
    /// This writer is an improved implementation of <see cref="JsonWriter"/>,
    /// it can validate the JSON data being written, throws <see cref="JsonFormatException"/> 
    /// on invalid data, and provides the ability of formatting the JSON text automatically.
    /// </summary>
    public class JsonWriterImproved : JsonWriter
    {
        private readonly JsonTokenStack _containerTokenStack = new JsonTokenStack();
        private readonly IndentedTextWriter _indentedTextWriter;
        private JsonToken _lastToken = JsonToken.None;

        public JsonWriterImproved(IndentedTextWriter writer)
            : base(writer)
        {
            _indentedTextWriter = writer;
        }

        public string IndentMark
        {
            get { return _indentedTextWriter.IndentMark; }
            set { _indentedTextWriter.IndentMark = value; }
        }

        public override void WriteObjectStart()
        {
            ValidateTokenState(JsonToken.ObjectStart);

            base.WriteObjectStart();
            Writer.WriteLine();
            _indentedTextWriter.IndentIncrease();

            _lastToken = JsonToken.ObjectStart;
            _containerTokenStack.Push(JsonToken.ObjectStart);
        }

        public override void WriteObjectEnd()
        {
            ValidateTokenState(JsonToken.ObjectEnd);

            _indentedTextWriter.IndentDecrease();
            if (_lastToken != JsonToken.ObjectStart)
            {
                Writer.WriteLine();
            }
            base.WriteObjectEnd();

            _lastToken = JsonToken.ObjectEnd;
            _containerTokenStack.Pop();
        }

        public override void WriteArrayStart()
        {
            ValidateTokenState(JsonToken.ArrayStart);

            base.WriteArrayStart();
            Writer.WriteLine();
            _indentedTextWriter.IndentIncrease();

            _lastToken = JsonToken.ArrayStart;
            _containerTokenStack.Push(JsonToken.ArrayStart);
        }

        public override void WriteArrayEnd()
        {
            ValidateTokenState(JsonToken.ArrayEnd);

            _indentedTextWriter.IndentDecrease();
            if (_lastToken != JsonToken.ArrayStart)
            {
                Writer.WriteLine();
            }
            base.WriteArrayEnd();

            _lastToken = JsonToken.ArrayEnd;
            _containerTokenStack.Pop();
        }

        public override void WritePropertyName(string name)
        {
            ValidateTokenState(JsonToken.PropertyName);

            if (_lastToken == JsonToken.Comma)
            {
                Writer.WriteLine();
            }

            base.WritePropertyName(name);
            _lastToken = JsonToken.PropertyName;
        }

        public override void WriteComma()
        {
            ValidateTokenState(JsonToken.Comma);

            base.WriteComma();
            _lastToken = JsonToken.Comma;
        }

        public override void WriteUndefinedValue()
        {
            PrepareForScalarValue(JsonToken.UndefinedValue);
            base.WriteUndefinedValue();
            _lastToken = JsonToken.UndefinedValue;
        }

        public override void WriteNullValue()
        {
            PrepareForScalarValue(JsonToken.NullValue);
            base.WriteNullValue();
            _lastToken = JsonToken.NullValue;
        }

        public override void WriteRawStringValue(string value)
        {
            PrepareForScalarValue(JsonToken.StringValue);
            base.WriteRawStringValue(value);
            _lastToken = JsonToken.StringValue;
        }

        public override void WriteStringValue(string value)
        {
            PrepareForScalarValue(JsonToken.StringValue);
            base.WriteStringValue(value);
            _lastToken = JsonToken.StringValue;
        }

        public override void WriteBooleanValue(bool value)
        {
            PrepareForScalarValue(JsonToken.BooleanValue);
            base.WriteBooleanValue(value);
            _lastToken = JsonToken.BooleanValue;
        }

        public override void WriteNumberValue(string value)
        {
            PrepareForScalarValue(JsonToken.NumberValue);
            base.WriteNumberValue(value);
            _lastToken = JsonToken.NumberValue;
        }

        public override void WriteNumberValue(byte number)
        {
            PrepareForScalarValue(JsonToken.NumberValue);
            base.WriteNumberValue(number);
            _lastToken = JsonToken.NumberValue;
        }

        public override void WriteNumberValue(short number)
        {
            PrepareForScalarValue(JsonToken.NumberValue);
            base.WriteNumberValue(number);
            _lastToken = JsonToken.NumberValue;
        }

        public override void WriteNumberValue(int number)
        {
            PrepareForScalarValue(JsonToken.NumberValue);
            base.WriteNumberValue(number);
            _lastToken = JsonToken.NumberValue;
        }

        [CLSCompliant(false)]
        public override void WriteNumberValue(uint number)
        {
            PrepareForScalarValue(JsonToken.NumberValue);
            base.WriteNumberValue(number);
            _lastToken = JsonToken.NumberValue;
        }

        public override void WriteNumberValue(long number)
        {
            PrepareForScalarValue(JsonToken.NumberValue);
            base.WriteNumberValue(number);
            _lastToken = JsonToken.NumberValue;
        }

        [CLSCompliant(false)]
        public override void WriteNumberValue(ulong number)
        {
            PrepareForScalarValue(JsonToken.NumberValue);
            base.WriteNumberValue(number);
            _lastToken = JsonToken.NumberValue;
        }

        public override void WriteNumberValue(float number)
        {
            PrepareForScalarValue(JsonToken.NumberValue);
            base.WriteNumberValue(number);
            _lastToken = JsonToken.NumberValue;
        }

        public override void WriteNumberValue(double number)
        {
            PrepareForScalarValue(JsonToken.NumberValue);
            base.WriteNumberValue(number);
            _lastToken = JsonToken.NumberValue;
        }

        public override void WriteNumberValue(decimal number)
        {
            PrepareForScalarValue(JsonToken.NumberValue);
            base.WriteNumberValue(number);
            _lastToken = JsonToken.NumberValue;
        }

        private void PrepareForScalarValue(JsonToken tokenToWrite)
        {
            ValidateTokenState(tokenToWrite);

            if (_containerTokenStack.Top == JsonToken.ArrayStart && _lastToken == JsonToken.Comma)
                Writer.WriteLine();
        }

        private void ValidateTokenState(JsonToken tokenToWrite)
        {
            if (JsonTokenValidator.Validate(tokenToWrite, _lastToken, _containerTokenStack.Top))
                return;

            throw new JsonFormatException(
                "Cannot write the token at the current position.",
                tokenToWrite, _lastToken, _containerTokenStack.Top);
        }
    }
}
