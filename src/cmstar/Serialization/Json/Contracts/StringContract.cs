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

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The contract for strings.
    /// </summary>
    public class StringContract : JsonContract
    {
        public StringContract()
            : base(typeof(string))
        {
        }

        protected override void DoWrite(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            if (obj == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                var value = JsonConvert.ToString(obj);
                writer.WriteStringValue(value);
            }
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            reader.Read();

            switch (reader.Token)
            {
                case JsonToken.StringValue:
                    return reader.Value;

                case JsonToken.NullValue:
                case JsonToken.UndefinedValue:
                    return null;

                case JsonToken.NumberValue:
                    return reader.Value.ToString();

                case JsonToken.BooleanValue:
                    return ((bool)reader.Value) ? "true" : "false";

                default:
                    throw JsonContractErrors.UnexpectedToken(reader.Token);
            }
        }
    }
}
