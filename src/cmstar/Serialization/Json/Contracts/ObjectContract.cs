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
using System.Collections.Generic;
using System.Reflection;
using cmstar.RapidReflection.Emit;
using cmstar.Util;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The <see cref="JsonContract"/> for objects or structs.
    /// The contract maps properties in JSON from/to propertys/fields of CLR objects.
    /// </summary>
    public class ObjectContract : JsonContract
    {
        private readonly ContractMemberCollection _members;
        private readonly Func<object> _instanceCreator;
        private readonly Func<object[], object> _anonymousInstanceCreator;
        private readonly bool _underlyingTypeIsAnonymous;
        private readonly Dictionary<string, IndexType> _constructorArgumentIndexTypes;
        
        /// <summary>
        /// Initializes a new instance of <see cref="ObjectContract"/>
        /// with the given underlying type.
        /// </summary>
        /// <param name="type">The underlying type.</param>
        public ObjectContract(Type type)
            : this(type, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectContract"/>
        /// with the given underlying type.
        /// </summary>
        /// <param name="type">The underlying type.</param>
        /// <param name="memberNameComparer"></param>
        public ObjectContract(Type type, IEqualityComparer<string> memberNameComparer)
            : base(type)
        {
            _members = new ContractMemberCollection(memberNameComparer);
            _underlyingTypeIsAnonymous = ReflectionUtils.IsAnonymousType(type);

            if (_underlyingTypeIsAnonymous)
            {
                var contructor = type.GetConstructors()[0];
                _anonymousInstanceCreator = ConstructorInvokerGenerator.CreateDelegate(contructor);

                var args = contructor.GetParameters();
                _constructorArgumentIndexTypes = new Dictionary<string, IndexType>(args.Length);
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    var indexType = new IndexType { Index = i, Type = arg.ParameterType };
                    _constructorArgumentIndexTypes.Add(arg.Name, indexType);
                }
            }
            else if (type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null)
            {
                _instanceCreator = ConstructorInvokerGenerator.CreateDelegate(type);
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="ContractMemberInfo"/> which describes
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

            // try serialize members of derived types
            var objType = obj.GetType();
            if (objType != UnderlyingType)
            {
                var contract = contractResolver.ResolveContract(objType);
                contract.Write(writer, state, contractResolver, obj);
                return;
            }

            writer.WriteObjectStart();

            var first = true;
            foreach (var member in _members)
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

            // 1 treat JS undefined as CLR null;
            // 2 anything can be an object, if the underlying type is Object, directly returns the value read
            switch (reader.Token)
            {
                case JsonToken.NullValue:
                case JsonToken.UndefinedValue:
                    return null;

                case JsonToken.StringValue:
                case JsonToken.NumberValue:
                case JsonToken.BooleanValue:
                    if (UnderlyingType == typeof(object))
                        return reader.Value;

                    throw JsonContractErrors.UnexpectedToken(JsonToken.ObjectStart, reader.Token);

                case JsonToken.ObjectStart:
                    return _underlyingTypeIsAnonymous
                        ? ReadAnononymousInstance(reader, state)
                        : ReadOnymousInstance(reader, state);

                default:
                    throw JsonContractErrors.UnexpectedToken(JsonToken.ObjectStart, reader.Token);
            }
        }

        private object ReadAnononymousInstance(JsonReader reader, JsonDeserializingState state)
        {
            //read and store the argument values first
            var argumentsPresented = new Dictionary<IndexType, object>();

            while (reader.Read())
            {
                if (reader.Token == JsonToken.ObjectEnd)
                    break;

                if (reader.Token == JsonToken.Comma)
                    continue;

                if (reader.Token != JsonToken.PropertyName)
                    throw JsonContractErrors.UnexpectedToken(reader.Token);

                var name = (string)reader.Value;
                ContractMemberInfo member;
                if (!_members.TryGetValue(name, out member))
                {
                    SkipPropertyValue(reader);
                    continue;
                }

                var value = member.Contract.Read(reader, state);
                var indexType = _constructorArgumentIndexTypes[name];
                argumentsPresented.Add(indexType, value);
            }

            //build the argument array for the constructor,
            //determine if any property was presented in the JSON,
            //if not, a default value of the argument type should be used
            var args = new object[_members.Count];

            foreach (var indexType in _constructorArgumentIndexTypes.Values)
            {
                object argumentValue;
                if (argumentsPresented.TryGetValue(indexType, out argumentValue))
                {
                    args[indexType.Index] = argumentValue;
                }
                else if (indexType.Type.IsValueType)
                {
                    //Activator.CreateInstance should be slow, any good idea?
                    args[indexType.Index] = Activator.CreateInstance(indexType.Type);
                }
            }

            var instance = _anonymousInstanceCreator(args);
            return instance;
        }

        private object ReadOnymousInstance(JsonReader reader, JsonDeserializingState state)
        {
            if (_instanceCreator == null)
                throw JsonContractErrors.CannotCreateInstance(UnderlyingType, null);

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
                if (!_members.TryGetValue((string)reader.Value, out member))
                {
                    SkipPropertyValue(reader);
                    continue;
                }

                try
                {
                    var value = member.Contract.Read(reader, state);

                    // ignores a member without a setter
                    if (member.ValueSetter == null)
                        continue;

                    member.ValueSetter(instance, value);
                }
                catch (Exception ex)
                {
                    string memberName;
                    Type memberType;

                    if (member.IsProperty)
                    {
                        var propInfo = (PropertyInfo)member.MemberInfo;
                        memberName = propInfo.Name;
                        memberType = propInfo.PropertyType;
                    }
                    else
                    {
                        var fieldInfo = (FieldInfo)member.MemberInfo;
                        memberName = fieldInfo.Name;
                        memberType = fieldInfo.FieldType;
                    }

                    var msg = string.Format(
                        "Error on setting value for {0} ({1}) on type {2}.",
                        memberName, memberType, UnderlyingType);

                    throw new JsonContractException(msg, ex);
                }
            }

            return instance;
        }

        // keeps the index and type of an parameter of the type constructor
        private class IndexType : IEqualityComparer<IndexType>
        {
            public int Index;
            public Type Type;

            public bool Equals(IndexType x, IndexType y)
            {
                return x.Index == y.Index;
            }

            public int GetHashCode(IndexType obj)
            {
                return Index.GetHashCode();
            }
        }
    }
}
