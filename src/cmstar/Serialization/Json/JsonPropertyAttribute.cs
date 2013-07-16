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
    /// <summary>
    /// Instructs the <see cref="JsonSerializer"/> to always serialize the member with the specified name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class JsonPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyAttribute"/> class.
        /// </summary>
        public JsonPropertyAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyAttribute"/> class 
        /// with the specified name.
        /// </summary>
        /// <param name="propertyName">Name of the property/field.</param>
        public JsonPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Gets or sets the name of the class member.
        /// </summary>
        /// <value>The name of the class member.</value>
        public string PropertyName { get; set; }
    }
}
