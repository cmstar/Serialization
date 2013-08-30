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
using cmstar.RapidReflection.Emit;
using cmstar.Util;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The <see cref="JsonContract"/> for objects or stucts.
    /// The contract maps propertys in JSON from/to propertys/fields of CLR objects.
    /// </summary>
    public class ObjectContract : JsonContract
    {
        private readonly ContractMemberCollection _members = new ContractMemberCollection();
        private readonly Func<object> _instanceCreator;
        private readonly Func<object[], object> _anonymousInstanceCreator;
        private readonly bool _underlyingTypeIsAnonymous;

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectContract"/>
        /// with the given underlying type.
        /// </summary>
        /// <param name="type">The underlying type.</param>
        public ObjectContract(Type type)
            : base(type)
        {
            _underlyingTypeIsAnonymous = ReflectionUtils.IsAnonymousType(type);

            if (_underlyingTypeIsAnonymous)
            {
                var contructor = type.GetConstructors()[0];
                _anonymousInstanceCreator = ConstructorInvokerGenerator.CreateDelegate(contructor);
            }
            else
            {
                _instanceCreator = ConstructorInvokerGenerator.CreateDelegate(type);
            }
        }

        /// <summary>
        /// Gets the colleciton of <see cref="ContractMemberInfo"/> which describes
        /// how to serialize the properties or fields of the underlying type.
        /// </summary>
        public ContractMemberCollection Members
        {
            get { return _members; }
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
                return;
            }

            writer.WriteObjectStart();

            var first = true;
            foreach (var member in Members)
            {
                //only write the members which has a getter
                if (member.ValueGetter == null)
                    continue;

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

            if (_underlyingTypeIsAnonymous)
                return ReadAnononymouseInstance(reader, state);

            return ReadOnymousInstance(reader, state);
        }

        private object ReadAnononymouseInstance(JsonReader reader, JsonDeserializingState state)
        {
            var args = new object[Members.Count];

            while (reader.Read())
            {
                if (reader.Token == JsonToken.ObjectEnd)
                    break;

                if (reader.Token == JsonToken.Comma)
                    continue;

                if (reader.Token != JsonToken.PropertyName)
                    throw JsonContractErrors.UnexpectedToken(reader.Token);

                ContractMemberInfo member;
                int index;
                if (!Members.TryGetValueAndIndex((string)reader.Value, out member, out index))
                    continue;

                var value = member.Contract.Read(reader, state);
                args[index] = value;
            }

            var instance = _anonymousInstanceCreator(args);
            return instance;
        }

        private object ReadOnymousInstance(JsonReader reader, JsonDeserializingState state)
        {
            var instance = _instanceCreator();

            while (reader.Read())
            {
                if (reader.Token == JsonToken.ObjectEnd)
                    break;

                if (reader.Token == JsonToken.Comma)
                    continue;

                if (reader.Token != JsonToken.PropertyName)
                    throw JsonContractErrors.UnexpectedToken(reader.Token);

                ContractMemberInfo member;
                if (!Members.TryGetValue((string)reader.Value, out member))
                    continue;

                var value = member.Contract.Read(reader, state);

                //ignores a member without a setter
                if (member.ValueSetter == null)
                    continue;

                member.ValueSetter(instance, value);
            }

            return instance;
        }
    }
}
