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

namespace cmstar.Serialization.Json
{
    /// <summary>
    /// Serializes and deserializes objects into and from the JSON format.
    /// </summary>
    public class JsonSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JsonSerializer"/>.
        /// </summary>
        public JsonSerializer()
            : this(new JsonContractResolver())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonSerializer"/>
        /// with the given implemntation of <see cref="IJsonContractResolver"/>.
        /// </summary>
        /// <param name="contractResolver">
        /// The implemntation of <see cref="IJsonContractResolver"/> which is used for
        /// resolving <see cref="JsonContract"/>s for objects.
        /// </param>
        public JsonSerializer(IJsonContractResolver contractResolver)
        {
            ArgAssert.NotNull(contractResolver, "contractResolver");
            ContractResolver = contractResolver;
        }

        /// <summary>
        /// Gets the <see cref="IJsonContractResolver"/> for the <see cref="JsonSerializer"/>.
        /// </summary>
        public IJsonContractResolver ContractResolver { get; private set; }

        /// <summary>
        /// Indicates whether to enable the cycle reference checking.
        /// The default value is false.
        /// </summary>
        public bool CheckCycleReference { get; set; }

        /// <summary>
        /// Serializes the given object to a JSON.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>The JSON string.</returns>
        public string Serialize(object obj)
        {
            return Serialize(obj, Formatting.None);
        }

        /// <summary>
        /// Serializes the given object to a JSON with given <see cref="Formatting"/>.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="formatting">The value of <see cref="Formatting"/>.</param>
        /// <returns>The JSON string.</returns>
        public string Serialize(object obj, Formatting formatting)
        {
            var stringBuilder = new StringBuilder(256);
            using (var indentedTextWriter = new IndentedTextWriter(new StringWriter(stringBuilder)))
            {
                switch (formatting)
                {
                    case Formatting.None:
                        indentedTextWriter.NewLine = string.Empty;
                        indentedTextWriter.IndentMark = string.Empty;
                        break;

                    case Formatting.Multiple:
                        indentedTextWriter.IndentMark = string.Empty;
                        break;
                }

                using (var jsonWriter = new JsonWriterImproved(indentedTextWriter))
                {
                    jsonWriter.AutoCloseInternalWriter = false;
                    Serialize(obj, jsonWriter);

                    return stringBuilder.ToString();
                }
            }
        }

        /// <summary>
        /// Serialized the given object to a JSON 
        /// and writes the JSON to the specified <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="jsonWriter">
        /// The instance of <see cref="JsonWriter"/> which the JSON will be written to.
        /// It will not be disposed automatically after the method call.
        /// </param>
        public void Serialize(object obj, JsonWriter jsonWriter)
        {
            ArgAssert.NotNull(jsonWriter, "jsonWriter");

            var state = new JsonSerializingState();
            state.CheckCycleReference = CheckCycleReference;

            DoSerialize(obj, jsonWriter, state);
        }

        /// <summary>
        /// Serialized the given object to a JSON 
        /// and writes the JSON to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="textWriter">
        /// The instance of <see cref="TextWriter"/> which the JSON will be written to.
        /// It will not be disposed automatically after the method call.
        /// </param>
        public void Serialize(object obj, TextWriter textWriter)
        {
            ArgAssert.NotNull(textWriter, "textWriter");

            using (var jsonWriter = new JsonWriterImproved(new IndentedTextWriter(textWriter)))
            {
                jsonWriter.AutoCloseInternalWriter = false;
                Serialize(obj, jsonWriter);
            }
        }

        /// <summary>
        /// Serializes the given object to a JSON.
        /// This is a faster version than <see cref="Serialize(object)"/>. It does not include
        /// the validation of JSON format and the check of cycle reference.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>The JSON string.</returns>
        public string FastSerialize(object obj)
        {
            var stringBuilder = new StringBuilder(256);
            var stringWriter = new StringWriter(stringBuilder);

            using (var jsonWriter = new JsonWriter(stringWriter))
            {
                var state = new JsonSerializingState();
                state.CheckCycleReference = false;

                DoSerialize(obj, jsonWriter, state);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Deserializes a string which represents a JSON to a CLR object.
        /// </summary>
        /// <typeparam name="T">The type of the CLR object.</typeparam>
        /// <param name="json">The JSON.</param>
        /// <returns>The object deserialized from the JSON.</returns>
        public T Deserialize<T>(string json)
        {
            return (T)Deserialize(json, typeof(T));
        }

        /// <summary>
        /// Deserializes a string which represents a JSON to a CLR object.
        /// </summary>
        /// <param name="json">The JSON.</param>
        /// <param name="type">The type of the CLR object.</param>
        /// <returns>The object deserialized from the JSON.</returns>
        public object Deserialize(string json, Type type)
        {
            return Deserialize(new StringReader(json), type);
        }

        /// <summary>
        /// Deserializes a JSON read from a <see cref="TextReader"/> to a CLR object.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> from which to read the JSON.</param>
        /// <param name="type">The type of the CLR object.</param>
        /// <returns>The object deserialized from the JSON.</returns>
        public object Deserialize(TextReader textReader, Type type)
        {
            ArgAssert.NotNull(textReader, "reader");
            ArgAssert.NotNull(type, "type");

            using (var jsonReader = new JsonReader(textReader))
            {
                return DoDeserialize(jsonReader, type, new JsonDeserializingState());
            }
        }

        private void DoSerialize(object obj, JsonWriter jsonWriter, JsonSerializingState state)
        {
            var contract = ContractResolver.ResolveContract(obj);
            contract.Write(jsonWriter, state, ContractResolver, obj);
        }

        private object DoDeserialize(JsonReader jsonReader, Type type, JsonDeserializingState state)
        {
            var contract = ContractResolver.ResolveContract(type);
            return contract.Read(jsonReader, state);
        }
    }
}
