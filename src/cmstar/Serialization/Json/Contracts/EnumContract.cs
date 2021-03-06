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
#if NET35
using System.Collections.Generic;
#endif

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The contract for enumerations.
    /// </summary>
    public class EnumContract : JsonContract
    {
        private readonly IEnumNameParser _enumNameParser;

        /// <summary>
        /// Initialize a new instance of <see cref="EnumContract"/>.
        /// </summary>
        /// <param name="type">The type of the enum.</param>
        public EnumContract(Type type)
            : base(type)
        {
            if (!typeof(Enum).IsAssignableFrom(type))
                throw new ArgumentException("The type must be an enum.", "type");

            var parserType = typeof(EnumNameParser<>).MakeGenericType(type);
            _enumNameParser = (IEnumNameParser)Activator.CreateInstance(parserType);
        }

        /// <summary>
        /// Gets or sets a value, which specify whether to serializing a Enum by using it's name.
        /// If it is set to true, a Enum will be serialized to a JSON string with it's name;
        /// otherwise, will be serialized to a number with the index.
        /// The default value is false.
        /// </summary>
        public bool UseEnumName { get; set; }

        protected override void DoWrite(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            if (obj == null)
                throw JsonContractErrors.NullValueNotSupported();

            var e = (Enum)obj;
            if (UseEnumName)
            {
                if (!Enum.IsDefined(UnderlyingType, e))
                {
                    var msg = string.Format("The enumeration name for value {0} is not defined.", e);
                    throw new JsonContractException(msg);
                }

                var name = Enum.GetName(UnderlyingType, e);
                writer.WriteStringValue(name);
            }
            else
            {
                var index = Convert.ToInt32(obj);
                writer.WriteNumberValue(index);
            }
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            reader.Read();

            switch (reader.Token)
            {
                case JsonToken.NumberValue:
                    var value = (double)reader.Value;

                    //the number value must be an integer
                    if (double.IsNaN(value) || double.IsInfinity(value) || Math.Abs(value % 1) > 0)
                    {
                        var msg = string.Format(
                            "Cannot cast the number value {0} to {1}, the value must be an integer.",
                            value, UnderlyingType);
                        throw new JsonContractException(msg);
                    }
                    return Enum.ToObject(UnderlyingType, (int)value);

                case JsonToken.StringValue:
                    object enumValue;
                    if (!_enumNameParser.TryParse((string)reader.Value, out enumValue))
                    {
                        var msg = string.Format(
                            "Cannot cast the string value \"{0}\" to enumeration {1}.", reader.Value, UnderlyingType);
                        throw new JsonContractException(msg);
                    }
                    return enumValue;

                case JsonToken.NullValue:
                    if (state.NullValueHandling == JsonDeserializationNullValueHandling.AsDefaultValue)
                        return Enum.ToObject(UnderlyingType, 0);

                    throw JsonContractErrors.CannotConverType(null, UnderlyingType, null);

                default:
                    throw JsonContractErrors.UnexpectedToken(reader.Token);
            }
        }

        private interface IEnumNameParser
        {
            bool TryParse(string s, out object value);
        }

        private class EnumNameParser<T> : IEnumNameParser where T : struct
        {
#if NET35
            private readonly Dictionary<string, object> _enumNameMap;

            public EnumNameParser()
            {
                var names = Enum.GetNames(typeof(T));
                var values = Enum.GetValues(typeof(T));

                _enumNameMap = new Dictionary<string, object>(names.Length, StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < names.Length; i++)
                {
                    _enumNameMap.Add(names[i], values.GetValue(i));
                }
            }

            public bool TryParse(string s, out object value)
            {
                if (_enumNameMap.TryGetValue(s, out value))
                    return true;

                int i;
                if (int.TryParse(s, out i))
                {
                    value = Enum.ToObject(typeof(T), i);
                    return true;
                }

                return false;
            }
#else
            public bool TryParse(string s, out object value)
            {
                T v;
                if (Enum.TryParse(s, out v))
                {
                    value = v;
                    return true;
                }

                value = null;
                return false;
            }
#endif
        }
    }
}
