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

namespace cmstar.Serialization.Json
{
    /// <summary>
    /// Defines tokens in JSONs.
    /// </summary>
    public enum JsonToken
    {
        /// <summary>
        /// The token is used for <see cref="JsonWriter"/> when the methods have not been called;
        /// or for <see cref="JsonReader"/> when the reader is not started or has passed the end of JSON.
        /// </summary>
        None,

        /// <summary>
        /// The start of a JSON object ('{').
        /// </summary>
        ObjectStart,

        /// <summary>
        /// The end of a JSON object ('}').
        /// </summary>
        ObjectEnd,

        /// <summary>
        /// The start of a JSON array ('[').
        /// </summary>
        ArrayStart,

        /// <summary>
        /// The start of a JSON array (']').
        /// </summary>
        ArrayEnd,

        /// <summary>
        /// A JSON object property.
        /// </summary>
        PropertyName,

        /// <summary>
        /// The token for javascript null.
        /// </summary>
        NullValue,

        /// <summary>
        /// The token for strings.
        /// </summary>
        StringValue,

        /// <summary>
        /// The token for numbers.
        /// </summary>
        NumberValue,

        /// <summary>
        /// The token for boolean values.
        /// </summary>
        BooleanValue,

        /// <summary>
        /// The token for javascript 'undefined'.
        /// </summary>
        UndefinedValue,

        /// <summary>
        /// The comma used for separating the array elements or properties.
        /// </summary>
        Comma
    }
}
