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
    /// A contract that specifies how to serialize/deserialize JSONs.
    /// This is an abstract class.
    /// </summary>
    public abstract class JsonContract
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JsonContract"/>
        /// with the given underlying type.
        /// </summary>
        /// <param name="type">The underlying type.</param>
        protected JsonContract(Type type)
        {
            ArgAssert.NotNull(type, "type");
            UnderlyingType = type;
        }

        /// <summary>
        /// Gets the underlying type of the current <see cref="JsonContract"/>.
        /// </summary>
        public Type UnderlyingType { get; private set; }

        /// <summary>
        /// Writes the JSON represents the given object to the <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">
        /// The instance of <see cref="JsonWriter"/> the JSON will be written to.
        /// </param>
        /// <param name="state">
        /// A instance of <see cref="JsonSerializingState"/> that may contains some 
        /// options for serializing.
        /// </param>
        /// <param name="contractResolver">
        /// A <see cref="IJsonContractResolver"/> which is used to resolve
        /// <see cref="JsonContract"/>s during the serialization.
        /// </param>
        /// <param name="obj">
        /// The object for generating the JSON. 
        /// </param>
        public void Write(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            ArgAssert.NotNull(writer, "writer");
            ArgAssert.NotNull(state, "state");
            ArgAssert.NotNull(contractResolver, "contractResolver");

            if (obj == null || !state.CheckCycleReference)
            {
                DoWrite(writer, state, contractResolver, obj);
            }
            else
            {
                if (state.SerializingObjectStack.Contains(obj))
                    throw new JsonContractException("There is a cycle object reference.");

                state.SerializingObjectStack.Push(obj);
                DoWrite(writer, state, contractResolver, obj);
                state.SerializingObjectStack.Pop();
            }
        }

        /// <summary>
        /// Read the JSON represents a intance of the <see cref="UnderlyingType"/> from
        /// the <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">
        /// The instance of <see cref="JsonReader"/> the JSON will be read from.
        /// </param>
        /// <param name="state">
        /// A instance of <see cref="JsonDeserializingState"/> that may contains some 
        /// options for deserializing.
        /// </param>
        /// <returns>An intance of the <see cref="UnderlyingType"/>.</returns>
        public object Read(JsonReader reader, JsonDeserializingState state)
        {
            ArgAssert.NotNull(reader, "reader");
            return DoRead(reader, state);
        }

        /// <summary>
        /// Performs the writing.
        /// This method will be called by the <see cref="Write"/> method.
        /// </summary>
        /// <param name="writer">
        /// The instance of <see cref="JsonWriter"/> the JSON will be written to.
        /// </param>
        /// <param name="state">
        /// A instance of <see cref="JsonSerializingState"/> that may contains some 
        /// options for serializing.
        /// </param>
        /// <param name="contractResolver">
        /// A <see cref="IJsonContractResolver"/> which is used to resolve
        /// <see cref="JsonContract"/>s during the serialization.
        /// </param>
        /// <param name="obj">
        /// The object for generating the JSON. 
        /// </param>
        protected abstract void DoWrite(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj);

        /// <summary>
        /// Performs the reading.
        /// This method will be called by the <see cref="Read"/> method.
        /// </summary>
        /// <param name="reader">
        /// The instance of <see cref="JsonReader"/> the JSON will be read from.
        /// </param>
        /// <param name="state">
        /// A instance of <see cref="JsonDeserializingState"/> that may contains some 
        /// options for deserializing.
        /// </param>
        /// <returns>An intance of the <see cref="UnderlyingType"/>.</returns>
        protected abstract object DoRead(JsonReader reader, JsonDeserializingState state);
    }
}
