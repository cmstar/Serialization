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
    /// Represents a resolves that resolves <see cref="JsonContract"/>s for types.
    /// </summary>
    public interface IJsonContractResolver
    {
        /// <summary>
        /// Resolves the <see cref="JsonContract"/> for the given object.
        /// </summary>
        /// <param name="obj">The object to resolve.</param>
        /// <returns>The instance of <see cref="JsonContract"/> for the type.</returns>
        JsonContract ResolveContract(object obj);

        /// <summary>
        /// Resolves the <see cref="JsonContract"/> for the given type.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The instance of <see cref="JsonContract"/> for the type.</returns>
        JsonContract ResolveContract(Type type);

        /// <summary>
        /// Specify the <see cref="JsonContract"/> for the given type.
        /// If there's already a contract for the type, it will be replaced.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="contract">The instance of <see cref="JsonContract"/>.</param>
        void RegisterContract(Type type, JsonContract contract);
    }
}
