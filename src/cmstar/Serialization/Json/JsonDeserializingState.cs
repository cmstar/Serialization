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
    /// Keeps the state for deserializing.
    /// </summary>
    public class JsonDeserializingState
    {
        /// <summary>
        /// The default instance of the <seealso cref="JsonDeserializingState"/> class.
        /// Change the values of the members to customize the default behavior of the deserialization.
        /// </summary>
        public static readonly JsonDeserializingState Default = new JsonDeserializingState();

        /// <summary>
        /// How to treat JSON null.
        /// </summary>
        public JsonDeserializationNullValueHandling NullValueHandling = JsonDeserializationNullValueHandling.AsIs;
    }

    /// <summary>
    /// How to treat JSON null.
    /// </summary>
    public enum JsonDeserializationNullValueHandling
    {
        /// <summary>
        /// Keeps the original value.
        /// Deserialize a JSON null to a value type (e.g. int) may lead to an error. 
        /// </summary>
        AsIs,

        /// <summary>
        /// A JSON null can be treated as the default value for value-types.
        /// For a number it is 0; for a date it is 0001-1-1; etc.
        /// </summary>
        AsDefaultValue
    }
}
