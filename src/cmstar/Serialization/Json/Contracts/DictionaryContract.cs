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
using System.ComponentModel;
using System.Linq;
using cmstar.Util;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The contract for dictionaries.
    /// It maps the key-value pairs in the dictionary to JSON properties.
    /// </summary>
    public class DictionaryContract : JsonContract
    {
        /// <summary>
        /// The type definition for generic dictionaries.
        /// </summary>
        public static readonly Type GenericDictionaryTypeDefinition = typeof(IDictionary<,>);

        /// <summary>
        /// The type definition for non-generic dictionaries.
        /// </summary>
        public static readonly Type DictionaryTypeDefinition = typeof(IDictionary);

        private readonly Type _keyType; //the type of the dictionary keys
        private readonly Type _valueType; //the type of the dictionary values
        private readonly Func<object> _dictionaryCreator; // method for creating an instance of the dictionary
        private readonly IDictionaryManager _dictionaryManager;
        private readonly bool _isGenericDictionary;
        private TypeConverter _keyConverter;

        /// <summary>
        /// Initializes an new instance of <see cref="DictionaryContract"/>
        /// with the given underlying type.
        /// </summary>
        /// <param name="type">The underlying type.</param>
        public DictionaryContract(Type type)
            : base(type)
        {
            var args = ReflectionUtils.GetGenericArguments(type, GenericDictionaryTypeDefinition);
            if (args != null)
            {
                _keyType = args[0];
                _valueType = args[1];
                _isGenericDictionary = true;

                if (type.IsInterface)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == GenericDictionaryTypeDefinition)
                        _dictionaryCreator = CreateGenericDictionary;
                }
                else
                {
                    _dictionaryCreator = CreateTypeInstance;
                }

                var dictionaryManagerType = typeof(GenericDictionaryManager<,>).MakeGenericType(args);
                _dictionaryManager = (IDictionaryManager)Activator.CreateInstance(
                    dictionaryManagerType, _keyType, _dictionaryCreator);
            }
            else if (type.GetInterfaces().Contains(DictionaryTypeDefinition))
            {
                _isGenericDictionary = false;
            }
            else // not a dictionary type
            {
                var msg = string.Format("The type {0} is not supported by the contract.", type);
                throw new ArgumentException(msg, "type");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="JsonContract"/> for the values in the dictionary.
        /// If the property is null, the <see cref="JsonContract.Read"/> method would throw an exception.
        /// </summary>
        public JsonContract ValueContract { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TypeConverter"/> for the keys in the dictionary.
        /// The converter is used to convert the key objects to/from strings in the
        /// JSON property names.
        /// If the property is null, the <see cref="DictionaryContract"/> will try to use 
        /// the default converter.
        /// </summary>
        public TypeConverter KeyConverter
        {
            get
            {
                return _keyConverter;
            }
            set
            {
                ArgAssert.NotNull(value, "value");

                if (!value.CanConvertFrom(typeof(string)) || !value.CanConvertTo(typeof(string)))
                {
                    throw new ArgumentException(
                        "The converter should be able to convert the key type from/to a string.",
                        "value");
                }

                _keyConverter = value;
            }
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
            if (_isGenericDictionary)
            {
                WriteGenericDictionary(writer, state, contractResolver, obj);
            }
            else
            {
                WriteWeekTypeDictionary(writer, state, contractResolver, obj);
            }
            writer.WriteObjectEnd();
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            reader.Read();

            if (reader.Token == JsonToken.NullValue)
                return null;

            if (ValueContract == null)
                throw new JsonContractException("The contract for the dictionary values is not specified.");

            if (reader.Token != JsonToken.ObjectStart)
                throw JsonContractErrors.UnexpectedToken(JsonToken.ObjectStart, reader.Token);

            if (_dictionaryCreator == null)
                throw JsonContractErrors.CannotCreateInstance(UnderlyingType, null);

            var buffer = new List<DictionaryEntry>();
            while (reader.Read())
            {
                if (reader.Token == JsonToken.ObjectEnd)
                    break;

                switch (reader.Token)
                {
                    case JsonToken.PropertyName:
                        var propName = (string)reader.Value;
                        var value = ValueContract.Read(reader, state);
                        buffer.Add(new DictionaryEntry(propName, value));
                        break;

                    case JsonToken.Comma:
                        break;

                    default:
                        throw JsonContractErrors.UnexpectedToken(reader.Token);
                }
            }
            return _dictionaryManager.Create(_keyConverter, buffer);
        }

        private string ConvertKeyObjectToString(object key)
        {
            if (_keyConverter == null)
                return JsonConvert.ToString(key);

            var s = (string)_keyConverter.ConvertTo(key, typeof(string));
            return s;
        }

        private object CreateGenericDictionary()
        {
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(new[] { _keyType, _valueType });
            return Activator.CreateInstance(dictionaryType);
        }

        private object CreateTypeInstance()
        {
            return Activator.CreateInstance(UnderlyingType);
        }

        private void WriteGenericDictionary(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            var first = true;
            foreach (var entry in _dictionaryManager.Iterate(obj))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.WriteComma();
                }

                var propName = ConvertKeyObjectToString(entry.Key);
                writer.WritePropertyName(propName);
                ValueContract.Write(writer, state, contractResolver, entry.Value);
            }
        }

        private void WriteWeekTypeDictionary(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            JsonContract lastValueContract = null;
            Type lastValueType = null;

            var first = true;
            foreach (DictionaryEntry entry in (IDictionary)obj)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.WriteComma();
                }

                var propName = ConvertKeyObjectToString(entry.Key);
                writer.WritePropertyName(propName);

                var value = entry.Value;
                if (value == null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    var valueType = value.GetType();
                    if (valueType != lastValueType)
                    {
                        lastValueContract = contractResolver.ResolveContract(valueType);
                        lastValueType = valueType;
                    }

                    lastValueContract.Write(writer, state, contractResolver, value);
                }
            }
        }

        private interface IDictionaryManager
        {
            IEnumerable<DictionaryEntry> Iterate(object dictionary);
            object Create(TypeConverter keyConverter, IEnumerable<DictionaryEntry> buffer);
        }

        private class GenericDictionaryManager<TKey, TValue> : IDictionaryManager
        {
            private readonly Type _keyType; //the type of the dictionary keys
            private readonly bool _keyTypeIsConvertible; //if the key type implements IConvertible
            private readonly Func<object> _dictionaryCreator;

            public GenericDictionaryManager(Type keyType, Func<object> dictionaryCreator)
            {
                _keyType = keyType;
                _dictionaryCreator = dictionaryCreator;
                _keyTypeIsConvertible = _keyType.GetInterfaces().Contains(typeof(IConvertible));
            }

            public IEnumerable<DictionaryEntry> Iterate(object dictionary)
            {
                return ((IDictionary<TKey, TValue>)dictionary)
                    .Select(x => new DictionaryEntry(x.Key, x.Value));
            }

            public object Create(TypeConverter keyConverter, IEnumerable<DictionaryEntry> keyValues)
            {
                var dictionary = (IDictionary<TKey, TValue>)_dictionaryCreator();

                if (_keyType == typeof(string))
                {
                    foreach (var entry in keyValues)
                    {
                        dictionary.Add((TKey)entry.Key, (TValue)entry.Value);
                    }
                }
                else
                {
                    foreach (var entry in keyValues)
                    {
                        var key = ConvertKeyObjectFromString(keyConverter, (string)entry.Key);
                        dictionary.Add((TKey)key, (TValue)entry.Value);
                    }
                }

                return dictionary;
            }

            private object ConvertKeyObjectFromString(TypeConverter keyConverter, string s)
            {
                if (keyConverter == null)
                {
                    if (!_keyTypeIsConvertible)
                    {
                        var msg = string.Format(
                            "Cannot convert the property name to target type {0}.", _keyType);
                        throw new JsonContractException(msg);
                    }

                    return Convert.ChangeType(s, _keyType);
                }
                else
                {
                    var key = keyConverter.ConvertFrom(s);
                    if (key == null)
                    {
                        throw new JsonContractException(
                            "The key object converted from the converter is null.");
                    }

                    if (!_keyType.IsInstanceOfType(key))
                    {
                        var msg = string.Format(
                            "Cannot cast the object with type {0} from the converter to target type {1}.",
                            key.GetType(), _keyType);
                        throw new JsonContractException(msg);
                    }

                    return key;
                }
            }
        }
    }
}
