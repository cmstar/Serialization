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

using System.Collections.ObjectModel;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// A key-value map which takes the property names as the keys and instances of
    /// the <see cref="ContractMemberInfo"/> as the values.
    /// </summary>
    public class ContractMemberCollection : KeyedCollection<string, ContractMemberInfo>
    {
        /// <summary>
        /// Gets the instance of <see cref="ContractMemberInfo"/> with the specified property name.
        /// </summary>
        /// <param name="propertyName">
        /// The property name used to lookup the <see cref="ContractMemberInfo"/>.
        /// </param>
        /// <param name="member">
        /// The result of the lookup.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="ContractMemberInfo"/> is retrieved successfully;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetContractMember(string propertyName, out ContractMemberInfo member)
        {
            return Dictionary.TryGetValue(propertyName, out member);
        }

        protected override string GetKeyForItem(ContractMemberInfo item)
        {
            return item.JsonPropertyName;
        }
    }
}
