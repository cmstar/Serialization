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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using cmstar.RapidReflection.Emit;
using cmstar.Serialization.Json.Contracts;
using cmstar.Util;

namespace cmstar.Serialization.Json
{
    /// <summary>
    /// The default implementation of <see cref="IJsonContractResolver"/>.
    /// </summary>
    public class JsonContractResolver : IJsonContractResolver
    {
        private static readonly JsonContract NullValueContract = new ObjectContract(typeof(object));

        /// <summary>
        /// An object that can be used to synchronize access the cache of JSON contracts.
        /// </summary>
        public readonly object SyncRoot = new object();

        private readonly Dictionary<Type, JsonContract> _contractCache = new Dictionary<Type, JsonContract>();

        /// <summary>
        /// Initializes a new instance of <see cref="JsonContractResolver"/>.
        /// </summary>
        public JsonContractResolver()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonContractResolver"/>
        /// with a dictionary which contains some ready-made <see cref="JsonContract"/>s.
        /// </summary>
        /// <param name="contracts">
        /// The dictionary which contains some ready-made <see cref="JsonContract"/>s.
        /// </param>
        public JsonContractResolver(IDictionary<Type, JsonContract> contracts)
        {
            foreach (var c in contracts)
            {
                if (c.Value == null)
                {
                    throw new ArgumentException(
                        "The contract in the dictionary should not be null.", "contracts");
                }

                _contractCache.Add(c.Key, c.Value);
            }
        }

        /// <summary>
        /// Resolves the <see cref="JsonContract"/> for the given object.
        /// </summary>
        /// <param name="obj">The object to resolve.</param>
        /// <returns>The instance of <see cref="JsonContract"/> for the type.</returns>
        public JsonContract ResolveContract(object obj)
        {
            return obj == null ? NullValueContract : ResolveContract(obj.GetType());
        }

        /// <summary>
        /// Resolves the <see cref="JsonContract"/> for the given type.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The instance of <see cref="JsonContract"/> for the type.</returns>
        public JsonContract ResolveContract(Type type)
        {
            ArgAssert.NotNull(type, "type");

            JsonContract contract;
            if (_contractCache.TryGetValue(type, out contract))
                return contract;

            lock (SyncRoot)
            {
                try
                {
                    return DoResolve(type);
                }
                catch (Exception ex)
                {
                    var msg = string.Format("Failed on resolving the JsonContract for type {0}.", type);
                    throw new JsonContractException(msg, ex);
                }
            }
        }

        /// <summary>
        /// Performs the contract resolving.
        /// Override this method to customize the resolving.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The instance of <see cref="JsonContract"/> for the type.</returns>
        protected virtual JsonContract DoResolve(Type type)
        {
            var innerResolver = new InnerContractResolver(_contractCache);
            return innerResolver.ResolveContract(type);
        }

        private class InnerContractResolver : IJsonContractResolver
        {
            private Dictionary<Type, JsonContract> _contractCache;
            private Dictionary<Type, JsonContract> _buffer;

            public InnerContractResolver(Dictionary<Type, JsonContract> contractCache)
            {
                _contractCache = contractCache;
            }

            public JsonContract ResolveContract(object obj)
            {
                return obj == null ? NullValueContract : ResolveContract(obj.GetType());
            }

            public JsonContract ResolveContract(Type type)
            {
                JsonContract contract;
                if (_contractCache.TryGetValue(type, out contract))
                    return contract;

                if (_buffer == null)
                    _buffer = new Dictionary<Type, JsonContract>();

                contract = DoResolve(type);
                _contractCache[type] = contract;
                return contract;
            }

            private JsonContract DoResolve(Type type)
            {
                JsonContract contract;
                if (_buffer.TryGetValue(type, out contract))
                    return contract;

                if (type == typeof(string) || type == typeof(char))
                    return new StringContract();

                if (type == typeof(bool))
                    return new BooleanContract();

                //all primitive types except boolean and char are numbers,
                //also consider Decimal as number
                if (type.IsPrimitive || type == typeof(decimal))
                    return new NumberContract(type);

                if (type == typeof(DateTime))
                    return new DateTimeContract();

                if (type == typeof(Guid))
                    return new GuidContract();

                if (type.IsSubclassOf(typeof(Enum)))
                    return new EnumContract(type);

                //the contract for Nullable<>
                if (ReflectionUtils.IsNullableType(type))
                    return ResolveNullableTypeContract(type);

                //the contract for dictionaries, a IDictionary is also a ICollection,
                //so we must resolve this contract before trying to resolve contract for arrays
                DictionaryContract dictionaryContract;
                if (TryResolveDictionaryContract(type, out dictionaryContract))
                    return dictionaryContract;

                //the contract for collections
                ArrayContract arrayContract;
                if (TryResolveArrayContract(type, out arrayContract))
                    return arrayContract;

                //the contract for other types
                return ResolveObjectContract(type);
            }

            private bool TryResolveDictionaryContract(Type type, out DictionaryContract contract)
            {
                contract = null;

                //the contract for IDictionary<>
                var genericArguments = ReflectionUtils.GetGenericArguments(
                    type, DictionaryContract.GenericDictionaryTypeDefinition);
                if (genericArguments != null && genericArguments.Length > 0)
                {
                    var dictionaryContract = new DictionaryContract(type);
                    _buffer[type] = dictionaryContract;

                    if (genericArguments[1] != typeof(object))
                    {
                        dictionaryContract.ValueContract = ResolveContract(genericArguments[1]);
                    }

                    contract = dictionaryContract;
                    return true;
                }

                //the contract for IDictionary
                if (type.GetInterfaces().Contains(DictionaryContract.DictionaryTypeDefinition))
                {
                    contract = new DictionaryContract(type);
                    return true;
                }

                return false;
            }

            private bool TryResolveArrayContract(Type type, out ArrayContract contract)
            {
                contract = null;

                //the contract for object[]
                if (type == typeof(object[]))
                {
                    contract = new ArrayContract(type);
                    return true;
                }

                //the contract for generic collection
                var genericArguments = ReflectionUtils.GetGenericArguments(
                    type, ArrayContract.GenericArrayTypeDefinition);
                if (genericArguments != null && genericArguments.Length > 0)
                {
                    var arrayContract = new ArrayContract(type);
                    _buffer[type] = arrayContract;

                    arrayContract.ElementContract = ResolveContract(genericArguments[0]);
                    contract = arrayContract;
                    return true;
                }

                //the contract for week type collection
                if (type.GetInterfaces().Contains(ArrayContract.ArrayTypeDefinition))
                {
                    contract = new ArrayContract(type);
                    return true;
                }

                return false;
            }

            private NullableTypeContract ResolveNullableTypeContract(Type type)
            {
                var contract = new NullableTypeContract(type);
                _buffer[type] = contract;

                var underlyingType = ReflectionUtils.GetUnderlyingType(type);
                var underlyingTypeContract = ResolveContract(underlyingType);
                contract.UnderlyingTypeContract = underlyingTypeContract;

                return contract;
            }

            private ObjectContract ResolveObjectContract(Type type)
            {
                var contract = new ObjectContract(type);
                _buffer[type] = contract;

                var memberInfos = ResolveMemberInfos(type);
                foreach (var contractMemberInfo in memberInfos)
                {
                    contract.Members.Add(contractMemberInfo);
                }

                return contract;
            }

            private List<ContractMemberInfo> ResolveMemberInfos(Type type)
            {
                var memberInfoDescriptions = new List<MemberInfoDescription>();
                var hasJsonProperty = false;

                var fieldInfos = type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var fieldInfo in fieldInfos)
                {
                    var memberInfoDescription = GetMemberInfoDescription(fieldInfo);
                    memberInfoDescriptions.Add(memberInfoDescription);

                    if (!hasJsonProperty)
                        hasJsonProperty = (memberInfoDescription.JsonPropertyAttribute != null);
                }

                var propertyInfos = type.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var propertyInfo in propertyInfos)
                {
                    var memberInfoDescription = GetMemberInfoDescription(propertyInfo);
                    memberInfoDescriptions.Add(memberInfoDescription);

                    if (!hasJsonProperty)
                        hasJsonProperty = (memberInfoDescription.JsonPropertyAttribute != null);
                }

                return ResolveMemberInfos(memberInfoDescriptions, hasJsonProperty);
            }

            private MemberInfoDescription GetMemberInfoDescription(MemberInfo memberInfo)
            {
                var memberInfoDescription = new MemberInfoDescription();
                memberInfoDescription.MemberInfo = memberInfo;

                var attributes = memberInfo.GetCustomAttributes(true);
                foreach (var attribute in attributes)
                {
                    var jsonPropertyAttribute = attribute as JsonPropertyAttribute;
                    if (jsonPropertyAttribute != null)
                    {
                        memberInfoDescription.JsonPropertyAttribute = jsonPropertyAttribute;
                        continue;
                    }

                    var jsonIgnoreAttribute = attribute as JsonIgnoreAttribute;
                    if (jsonIgnoreAttribute != null)
                    {
                        memberInfoDescription.JsonIgnoreAttribute = jsonIgnoreAttribute;
                    }
                }

                return memberInfoDescription;
            }

            private List<ContractMemberInfo> ResolveMemberInfos(
                List<MemberInfoDescription> memberInfoDescriptions, bool useJsonPropertyAttribute)
            {
                var memberInfos = new List<ContractMemberInfo>(memberInfoDescriptions.Count);

                foreach (var memberInfoDescription in memberInfoDescriptions)
                {
                    var contractMemberInfo = CreateContractMemberInfo(
                        memberInfoDescription, useJsonPropertyAttribute);

                    if (contractMemberInfo == null)
                        continue;

                    _buffer[contractMemberInfo.Type] = contractMemberInfo.Contract;
                    memberInfos.Add(contractMemberInfo);
                }

                return memberInfos;
            }

            //Returns null in any case of:
            //  The member marked with JsonIgnoreAttribute;
            //  Intends to use JsonPropertyAttribute but the member not marked with it;
            //  Neither ValueGetter nor ValueSetter is available;
            private ContractMemberInfo CreateContractMemberInfo(
                MemberInfoDescription memberInfoDescription, bool useJsonPropertyAttribute)
            {
                if (memberInfoDescription.JsonIgnoreAttribute != null)
                    return null;

                if (useJsonPropertyAttribute && memberInfoDescription.JsonPropertyAttribute == null)
                    return null;

                var memberInfo = memberInfoDescription.MemberInfo;
                var info = new ContractMemberInfo();

                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo != null)
                {
                    var getMethod = propertyInfo.GetGetMethod(useJsonPropertyAttribute);
                    var setMethod = propertyInfo.GetSetMethod(useJsonPropertyAttribute);

                    if (getMethod == null && setMethod == null)
                        return null;

                    if (getMethod != null)
                        info.ValueGetter = PropertyAccessorGenerator.CreateGetter(propertyInfo, true);

                    if (setMethod != null)
                        info.ValueSetter = PropertyAccessorGenerator.CreateSetter(propertyInfo, true);

                    info.Type = propertyInfo.PropertyType;
                    info.IsProperty = true;
                }
                else
                {
                    var fieldInfo = (FieldInfo)memberInfo;

                    if (!useJsonPropertyAttribute && !fieldInfo.IsPublic)
                        return null;

                    info.ValueGetter = FieldAccessorGenerator.CreateGetter(fieldInfo);
                    info.ValueSetter = FieldAccessorGenerator.CreateSetter(fieldInfo);
                    info.Type = fieldInfo.FieldType;
                    info.IsProperty = false;
                }

                info.Contract = ResolveContract(info.Type);
                info.MemberInfo = memberInfo;
                info.Name = memberInfo.Name;

                if (useJsonPropertyAttribute)
                {
                    info.JsonPropertyName = memberInfoDescription.JsonPropertyAttribute.PropertyName;
                    if (string.IsNullOrEmpty(info.JsonPropertyName))
                    {
                        info.JsonPropertyName = memberInfo.Name;
                    }
                }
                else
                {
                    info.JsonPropertyName = memberInfo.Name;
                }

                return info;
            }
        }

        private class MemberInfoDescription
        {
            public MemberInfo MemberInfo;
            public JsonPropertyAttribute JsonPropertyAttribute;
            public JsonIgnoreAttribute JsonIgnoreAttribute;
        }
    }
}
