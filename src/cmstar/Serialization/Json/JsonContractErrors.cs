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
    internal static class JsonContractErrors
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
    }
}
