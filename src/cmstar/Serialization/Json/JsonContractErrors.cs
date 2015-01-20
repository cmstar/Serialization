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
    public static class JsonContractErrors
    {
        public static JsonContractException UnexpectedToken(JsonToken expected, JsonToken actual)
        {
            var msg = string.Format(
                "Unexpected token, {0} expected, but was {1}.",
                expected, actual);
            return new JsonContractException(msg);
        }

        public static JsonContractException UnexpectedToken(JsonToken actual)
        {
            var msg = string.Format("Unexpected token: {0}.", actual);
            return new JsonContractException(msg);
        }

        public static JsonContractException CannotCreateInstance(Type type, Exception innerException)
        {
            var msg = string.Format("Cannot create an instance of type {0}", type);
            return new JsonContractException(msg, innerException);
        }

        public static JsonContractException NullValueNotSupported()
        {
            var msg = "Null values are not supported in the contract.";
            return new JsonContractException(msg);
        }

        public static JsonContractException TypeNotSupported(Type type)
        {
            var msg = string.Format("The type {0} is not supported by the contract.", type);
            return new JsonContractException(msg);
        }

        public static JsonContractException CannotConverType(object value, Type targetType, Exception innException)
        {
            string valueDescription;
            if (value == null)
            {
                valueDescription = "<null>";
            }
            else
            {
                var type = value.GetType();
                var typeCode = Type.GetTypeCode(type);
                switch (typeCode)
                {
                    case TypeCode.String:
                        valueDescription = string.Concat("'", (string)value, "'");
                        break;

                    case TypeCode.Double:
                        valueDescription = "number " + value;
                        break;

                    case TypeCode.Object:
                        valueDescription = "<object>";
                        break;

                    default:
                        valueDescription = string.Format("{0} ({1})", value, type);
                        break;
                }
            }

            var msg = string.Format("Can not cast {0} to type {1}.", valueDescription, targetType);
            return new JsonContractException(msg, innException);
        }
    }
}
