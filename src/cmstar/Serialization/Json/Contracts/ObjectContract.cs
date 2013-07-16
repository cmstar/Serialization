﻿#region Licence
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
using cmstar.RapidReflection.Emit;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The <see cref="JsonContract"/> for objects or stucts.
    /// The contract maps propertys in JSON from/to propertys/fields of CLR objects.
    /// </summary>
    public class ObjectContract : JsonContract
    {
        private readonly Func<object> _instanceCreator;

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectContract"/>
        /// with the given underlying type.
        /// </summary>
        /// <param name="type">The underlying type.</param>
        public ObjectContract(Type type)
            : base(type)
        {
            Members = new ContractMemberCollection();
            _instanceCreator = ConstructorInvokerGenerator.CreateDelegate(type);
        }

        /// <summary>
        /// Gets the colleciton of <see cref="ContractMemberInfo"/> which describes
        /// how to serialize the properties or fields of the underlying type.
        /// </summary>
        public ContractMemberCollection Members { get; private set; }

        protected override void DoWrite(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            if (obj == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteObjectStart();

            var first = true;
            foreach (var member in Members)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.WriteComma();
                }

                writer.WritePropertyName(member.JsonPropertyName);

                var memberValue = member.ValueGetter(obj);
                member.Contract.Write(writer, state, contractResolver, memberValue);
            }

            writer.WriteObjectEnd();
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            reader.Read();

            if (reader.Token == JsonToken.NullValue)
                return null;

            if (reader.Token != JsonToken.ObjectStart)
                throw JsonContractErrors.UnexpectedToken(JsonToken.ObjectStart, reader.Token);

            var instance = _instanceCreator();
            while (reader.Read())
            {
                if (reader.Token == JsonToken.ObjectEnd)
                    break;

                switch (reader.Token)
                {
                    case JsonToken.Comma:
                        break;

                    case JsonToken.PropertyName:
                        ContractMemberInfo member;
                        if (Members.TryGetContractMember((string)reader.Value, out member))
                        {
                            var value = member.Contract.Read(reader, state);
                            member.ValueSetter(instance, value);
                        }
                        break;

                    default:
                        throw JsonContractErrors.UnexpectedToken(reader.Token);
                }
            }

            return instance;
        }
    }
}
