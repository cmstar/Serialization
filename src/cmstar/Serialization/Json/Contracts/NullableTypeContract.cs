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
    /// The contract for <see cref="Nullable{T}"/>.
    /// </summary>
    public class NullableTypeContract : JsonContract
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NullableTypeContract"/>
        /// with the given underlying type.
        /// </summary>
        /// <param name="type">The underlying type.</param>
        public NullableTypeContract(Type type)
            : base(type)
        {
            if (!type.IsValueType)
            {
                var msg = string.Format("The type {0} is not a value type.", type);
                throw new ArgumentException(msg, "type");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="JsonContract"/> for 
        /// the underlying type of <see cref="Nullable{T}"/>.
        /// </summary>
        public JsonContract UnderlyingTypeContract { get; set; }

        protected override void DoWrite(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            ValidateUnderlyingTypeContract();

            if (obj == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                UnderlyingTypeContract.Write(writer, state, contractResolver, obj);
            }
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            ValidateUnderlyingTypeContract();

            if (reader.PeekNextToken() == JsonToken.NullValue)
            {
                reader.Read();
                return null;
            }

            return UnderlyingTypeContract.Read(reader, state);
        }

        private void ValidateUnderlyingTypeContract()
        {
            if (UnderlyingTypeContract == null)
                throw new InvalidOperationException("The contract of the underlying type is not set.");
        }
    }
}
