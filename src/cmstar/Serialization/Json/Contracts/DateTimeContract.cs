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

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The default contract for <see cref="DateTime"/>.
    /// It shares the format with the the contract for <see cref="DateTimeOffset"/>.
    /// The time read is always a local time.
    /// </summary>
    public class DateTimeContract : JsonContract
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DateTimeContract"/>.
        /// </summary>
        public DateTimeContract()
            : base(typeof(DateTime))
        {
        }

        protected override void DoWrite(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            if (obj == null)
                throw JsonContractErrors.NullValueNotSupported();

            var dateTimeOffsetContract = contractResolver.ResolveContract(typeof(DateTimeOffset));
            var value = (DateTime)obj;
            dateTimeOffsetContract.Write(writer, state, contractResolver, (DateTimeOffset)value);
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            reader.Read();

            if (reader.Token == JsonToken.NullValue
                && state.NullValueHandling == JsonDeserializationNullValueHandling.AsDefaultValue)
            {
                return new DateTime();
            }

            if (reader.Token != JsonToken.StringValue)
                throw JsonContractErrors.UnexpectedToken(JsonToken.StringValue, reader.Token);

            if (!TryParseDateTime((string)reader.Value, out var d))
            {
                var msg = string.Format("Cannot convert the value \"{0}\" to a DateTime.", reader.Value);
                throw new JsonContractException(msg);
            }
            return d;
        }

        /// <summary>
        /// Converts the specified string to <see cref="DateTime"/> and returns a value 
        /// that indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">The string represents a <see cref="DateTime"/>.</param>
        /// <param name="dateTime">The result.</param>
        /// <returns>
        /// true if the string was converted successfully; otherwise false.
        /// </returns>
        protected virtual bool TryParseDateTime(string value, out DateTime dateTime)
        {
            if (DateTime.TryParse(value, out dateTime))
                return true;

            if (JsonConvert.TryParseMicrosoftJsonDate(value, out var d))
            {
                dateTime = d.LocalDateTime;
                return true;
            }

            return false;
        }
    }
}
