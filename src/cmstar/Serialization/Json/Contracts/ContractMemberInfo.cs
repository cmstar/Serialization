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
using System.Reflection;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// Contains the data for the <see cref="JsonSerializer"/> to serialize or access the value
    /// of a property or field from a CLR object.
    /// </summary>
    public class ContractMemberInfo
    {
        /// <summary>
        /// Gets or sets the <see cref="JsonContract"/> used to serialize this property/field.
        /// </summary>
        public JsonContract Contract { get; set; }

        /// <summary>
        /// Gets the name of the property/field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the property type or field type of the member.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the property name in JSON of this property/field.
        /// </summary>
        public string JsonPropertyName { get; set; }

        /// <summary>
        /// Indicates if the class member is a property.
        /// <c>true</c> if the member is a property; otherwise is a field.
        /// </summary>
        public bool IsProperty { get; set; }

        /// <summary>
        /// Gets the <see cref="MemberInfo"/> of this property/field.
        /// If <see cref="IsProperty"/> is true, <see cref="MemberInfo"/> is 
        /// an instance of <see cref="PropertyInfo"/>; 
        /// otherwise an instance of <see cref="FieldInfo"/>.
        /// </summary>
        public MemberInfo MemberInfo { get; set; }

        /// <summary>
        /// Gets or sets the method for getting the value of the property or field.
        /// <c>null</c> if the getter is not available.
        /// </summary>
        public Func<object, object> ValueGetter { get; set; }

        /// <summary>
        /// Gets or sets the method for setting the value of the property or field.
        /// <c>null</c> if the setter is not available.
        /// </summary>
        public Action<object, object> ValueSetter { get; set; }
    }
}
