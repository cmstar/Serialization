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
using cmstar.RapidReflection.Emit;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// Contains the data for the <see cref="JsonSerializer"/> to serialize or access the value
    /// of a property or field from a CLR object.
    /// </summary>
    public class ContractMemberInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractMemberInfo"/> class
        /// </summary>
        /// <param name="memberInfo">
        /// The instance of <see cref="MemberInfo"/>.
        /// Must be an instance of <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>.
        /// </param>
        /// <param name="jsonPropertyName">The property name in the JSON.</param>
        /// <param name="contract">The contract used to serialize the property/field.</param>
        public ContractMemberInfo(MemberInfo memberInfo, string jsonPropertyName, JsonContract contract)
        {
            ArgAssert.NotNull(memberInfo, "memberInfo");
            ArgAssert.NotNullOrEmptyOrWhitespace(jsonPropertyName, "jsonPropertyName");
            ArgAssert.NotNull(contract, "contract");

            if (!(memberInfo is PropertyInfo) && !(memberInfo is FieldInfo))
                throw new ArgumentException("The member should be a property or a field.", "memberInfo");

            MemberInfo = memberInfo;
            JsonPropertyName = jsonPropertyName;
            Contract = contract;
            IsProperty = memberInfo is PropertyInfo;

            if (IsProperty)
            {
                var propertyInfo = (PropertyInfo)memberInfo;
                Type = propertyInfo.PropertyType;
                ValueGetter = PropertyAccessorGenerator.CreateGetter(propertyInfo, true);
                ValueSetter = PropertyAccessorGenerator.CreateSetter(propertyInfo, true);
            }
            else
            {
                var fieldInfo = (FieldInfo)memberInfo;
                Type = fieldInfo.FieldType;
                ValueGetter = FieldAccessorGenerator.CreateGetter(fieldInfo);
                ValueSetter = FieldAccessorGenerator.CreateSetter(fieldInfo);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="JsonContract"/> used to serialize this property/field.
        /// </summary>
        public JsonContract Contract { get; private set; }

        /// <summary>
        /// Gets the name of the property/field.
        /// </summary>
        public string Name
        {
            get { return MemberInfo.Name; }
        }

        /// <summary>
        /// Gets the property type or field type of the member.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets or sets the property name in JSON of this property/field.
        /// </summary>
        public string JsonPropertyName { get; private set; }

        /// <summary>
        /// Indicates if the class member is a property.
        /// <c>true</c> if the member is a property; otherwise is a field.
        /// </summary>
        public bool IsProperty { get; private set; }

        /// <summary>
        /// Gets the <see cref="MemberInfo"/> of this property/field.
        /// If <see cref="IsProperty"/> is true, <see cref="MemberInfo"/> is 
        /// an instance of <see cref="PropertyInfo"/>; 
        /// otherwise an instance of <see cref="FieldInfo"/>.
        /// </summary>
        public MemberInfo MemberInfo { get; private set; }

        /// <summary>
        /// Gets the method for getting the value of the property or field.
        /// </summary>
        public Func<object, object> ValueGetter { get; private set; }

        /// <summary>
        /// Gets the method for setting the value of the property or field.
        /// </summary>
        public Action<object, object> ValueSetter { get; private set; }
    }
}
