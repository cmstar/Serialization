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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using cmstar.Util;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The contract for collections.
    /// </summary>
    public class ArrayContract : JsonContract
    {
        public static readonly Type GenericArrayTypeDefinition = typeof(IEnumerable<>);
        public static readonly Type ArrayTypeDefinition = typeof(IEnumerable);

        private readonly Type _elementType;
        private readonly Func<IList, object> _collectionCreator;
        private readonly bool _canAppendElement = true;

        /// <summary>
        /// Initializes a new instance of <see cref="ArrayContract"/>
        /// with the type of the colletion.
        /// </summary>
        /// <param name="type">The type of the colletion.</param>
        public ArrayContract(Type type)
            : base(type)
        {
            var args = ReflectionUtils.GetGenericArguments(type, GenericArrayTypeDefinition);
            if (args == null || args.Length == 0) //non-generic collection
            {
                if (!ArrayTypeDefinition.IsAssignableFrom(type))
                    throw TypeNotSupported("type");

                if (type.IsInterface)
                {
                    if (type != typeof(IList) && type != typeof(ICollection) && type != typeof(IEnumerable))
                        throw TypeNotSupported("type");

                    _collectionCreator = CreateArrayList;
                }
                else
                {
                    _collectionCreator = CreateTypeInstance;
                }
            }
            else //generic collection
            {
                _elementType = args[0];

                if (type.IsArray)
                {
                    _collectionCreator = CreateArray;
                }
                else if (type.IsInterface)
                {
                    if (!type.IsGenericType)
                        throw TypeNotSupported("type");

                    var genericTypeDefination = type.GetGenericTypeDefinition();
                    if (genericTypeDefination != typeof(IList<>)
                        && genericTypeDefination != typeof(ICollection<>)
                        && genericTypeDefination != typeof(IEnumerable<>))
                    {
                        throw TypeNotSupported("type");
                    }

                    _collectionCreator = CreateGenericList;
                }
                else
                {
                    _collectionCreator = CreateTypeInstance;

                    // check if the type implementes ICollection<>, while deserializing, elements are 
                    // appended to the collection using the 'Add' method, which is defined in 
                    // the ICollection<> interface; if the type only implementes IEnumerable<>, it
                    // can not be deserialized except the collection is empty
                    _canAppendElement = ReflectionUtils.GetGenericArguments(type, GenericArrayTypeDefinition) != null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="JsonContract"/> used by the type of elements in the array.
        /// If it is set to null, the contract will be resolved while elements are being written
        /// to the JSON.
        /// If <see cref="ElementContract"/> is not set up, a non-empty JSON array cannot be 
        /// deserialized and the <see cref="JsonContract.Read"/> method will throw an exception.
        /// </summary>
        public JsonContract ElementContract { get; set; }

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

            writer.WriteArrayStart();
            if (ElementContract == null)
            {
                WriteWeekTypeElements(writer, state, contractResolver, obj);
            }
            else
            {
                WriteStrongTypeElements(writer, state, contractResolver, obj);
            }
            writer.WriteArrayEnd();
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            reader.Read();

            if (reader.Token == JsonToken.NullValue)
                return null;

            if (reader.Token != JsonToken.ArrayStart)
                throw JsonContractErrors.UnexpectedToken(JsonToken.ArrayStart, reader.Token);

            if (ElementContract == null)
            {
                //if the collection is not generic, only empty collection can be deserialized
                if (reader.Read() && reader.Token == JsonToken.ArrayEnd)
                    return _collectionCreator(null);

                throw new JsonContractException("The type of collection elements was not specified.");
            }

            if (!_canAppendElement)
            {
                // if the collection is not generic, only empty collection can be deserialized
                if (reader.Read() && reader.Token == JsonToken.ArrayEnd)
                    return _collectionCreator(null);

                throw new JsonContractException("The type can not be deserialized.");
            }

            var buffer = new ArrayList();
            while (true)
            {
                var nextToken = reader.PeekNextToken();
                if (nextToken == JsonToken.ArrayEnd)
                {
                    reader.Read();
                    break;
                }

                if (nextToken == JsonToken.Comma)
                {
                    reader.Read();
                }
                else
                {
                    var element = ElementContract.Read(reader, state);
                    buffer.Add(element);
                }
            }

            return CreateGenericCollection(buffer);
        }

        protected object CreateGenericCollection(IList elements)
        {
            if (_elementType == null)
                throw JsonContractErrors.CannotCreateInstance(UnderlyingType, null);

            try
            {
                return _collectionCreator(elements);
            }
            catch (Exception ex)
            {
                throw new JsonContractException("Failed to create the collection.", ex);
            }
        }

        private object CreateTypeInstance(IList elements)
        {
            var collection = Activator.CreateInstance(UnderlyingType);
            if (elements != null && elements.Count > 0)
            {
                AppendElements(collection, _elementType, elements);
            }
            return collection;
        }

        private object CreateArray(IList elements)
        {
            IList array = Array.CreateInstance(_elementType, elements.Count);
            for (int i = 0; i < elements.Count; i++)
            {
                array[i] = elements[i];
            }
            return array;
        }

        private object CreateGenericList(IList elements)
        {
            var listType = typeof(List<>).MakeGenericType(new[] { _elementType });
            var list = Activator.CreateInstance(listType);
            if (elements.Count > 0)
            {
                AppendElements(list, ElementContract.UnderlyingType, elements);
            }
            return list;
        }

        private object CreateArrayList(IList elements)
        {
            return new ArrayList();
        }

        private void AppendElements(object genericCollection, Type genericType, IEnumerable elements)
        {
            var method = GetType()
                .GetMethod("PerformAppending", BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(new[] { genericType });

            method.Invoke(this, new[] { genericCollection, elements });
        }

        // ReSharper disable UnusedMember.Local
        private void PerformAppending<T>(ICollection<T> collection, IEnumerable elements)
        {
            foreach (var element in elements)
            {
                collection.Add((T)element);
            }
        }
        // ReSharper restore UnusedMember.Local

        private void WriteStrongTypeElements(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            var first = true;
            foreach (var e in (IEnumerable)obj)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.WriteComma();
                }

                ElementContract.Write(writer, state, contractResolver, e);
            }
        }

        private void WriteWeekTypeElements(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            JsonContract lastContract = null;
            Type lastElementType = null;

            var first = true;
            foreach (var e in (IEnumerable)obj)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.WriteComma();
                }

                if (e == null)
                {
                    writer.WriteNullValue();
                    continue;
                }

                var elementType = e.GetType();
                if (elementType != lastElementType)
                {
                    lastContract = contractResolver.ResolveContract(elementType);
                    lastElementType = elementType;
                }

                lastContract.Write(writer, state, contractResolver, e);
            }
        }

        private ArgumentException TypeNotSupported(string paramName)
        {
            return new ArgumentException(
                "The given type is not supported in this contract.", paramName);
        }
    }
}
