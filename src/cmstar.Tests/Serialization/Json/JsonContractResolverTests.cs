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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using NUnit.Framework;
using cmstar.Serialization.Json.Contracts;

// ReSharper disable UnusedMember.Local
namespace cmstar.Serialization.Json
{
    [TestFixture]
    public class JsonContractResolverTests
    {
        [Test]
        public void ResolveScalarContract()
        {
            var resolver = new JsonContractResolver();

            AssertTypeContract<NumberContract>(resolver, typeof(int));
            AssertTypeContract<NumberContract>(resolver, typeof(float));
            AssertTypeContract<NumberContract>(resolver, typeof(decimal));
            AssertTypeContract<NumberContract>(resolver, typeof(ushort));
            AssertTypeContract<StringContract>(resolver, typeof(string));
            AssertTypeContract<BooleanContract>(resolver, typeof(bool));
            AssertTypeContract<DateTimeContract>(resolver, typeof(DateTime));
            AssertTypeContract<EnumContract>(resolver, typeof(SaleOrderType));
            AssertTypeContract<GuidContract>(resolver, typeof(Guid));
            AssertTypeContract<DataRowContract>(resolver, typeof(DataRow));
            AssertTypeContract<DataTableContract>(resolver, typeof(DataTable));
            AssertTypeContract<DataRecordContract>(resolver, typeof(DataRowRecord));
            AssertTypeContract<DbNullContract>(resolver, typeof(DBNull));

            var contract = AssertTypeContract<NullableTypeContract>(resolver, typeof(SaleOrderPoint?));
            Assert.IsInstanceOf<ObjectContract>(contract.UnderlyingTypeContract);
        }

        [Test]
        public void ResolveNullableTypeContract()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<NullableTypeContract>(resolver, typeof(int?));
            Assert.IsInstanceOf<NumberContract>(contract.UnderlyingTypeContract);

            contract = AssertTypeContract<NullableTypeContract>(resolver, typeof(DateTime?));
            Assert.IsInstanceOf<DateTimeContract>(contract.UnderlyingTypeContract);

            contract = AssertTypeContract<NullableTypeContract>(resolver, typeof(bool?));
            Assert.IsInstanceOf<BooleanContract>(contract.UnderlyingTypeContract);

            contract = AssertTypeContract<NullableTypeContract>(resolver, typeof(SaleOrderType?));
            Assert.IsInstanceOf<EnumContract>(contract.UnderlyingTypeContract);
        }

        [Test]
        public void ResolveGenericCollectionContract()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<ArrayContract>(resolver, typeof(int[]));
            Assert.IsInstanceOf<NumberContract>(contract.ElementContract);

            contract = AssertTypeContract<ArrayContract>(resolver, typeof(List<string>));
            Assert.IsInstanceOf<StringContract>(contract.ElementContract);

            contract = AssertTypeContract<ArrayContract>(resolver, typeof(List<SaleOrderType?>));
            Assert.IsInstanceOf<NullableTypeContract>(contract.ElementContract);
            Assert.IsInstanceOf<EnumContract>(((NullableTypeContract)contract.ElementContract).UnderlyingTypeContract);

            contract = AssertTypeContract<ArrayContract>(resolver, typeof(string[][]));
            Assert.IsInstanceOf<ArrayContract>(contract.ElementContract);
            Assert.IsInstanceOf<StringContract>(((ArrayContract)contract.ElementContract).ElementContract);
        }

        [Test]
        public void ResolveObjectCollectionContract()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<ArrayContract>(resolver, typeof(Collection));
            Assert.IsNull(contract.ElementContract);

            contract = AssertTypeContract<ArrayContract>(resolver, typeof(ArrayList));
            Assert.IsNull(contract.ElementContract);

            contract = AssertTypeContract<ArrayContract>(resolver, typeof(object[]));
            Assert.IsNull(contract.ElementContract);
        }

        [Test]
        public void ResolveGenericDictionaryContract()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<DictionaryContract>(resolver, typeof(Dictionary<string, int>));
            Assert.IsInstanceOf<NumberContract>(contract.ValueContract);

            contract = AssertTypeContract<DictionaryContract>(resolver, typeof(Dictionary<int, Dictionary<string, double>>));
            Assert.IsInstanceOf<DictionaryContract>(contract.ValueContract);
            Assert.IsInstanceOf<NumberContract>(((DictionaryContract)contract.ValueContract).ValueContract);
        }

        [Test]
        public void ResolveObjectDictionaryContract()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<DictionaryContract>(resolver, typeof(Hashtable));
            Assert.IsNull(contract.ValueContract);
        }

        [Test]
        public void ResolveSimpleObjectContract()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<ObjectContract>(resolver, typeof(SaleOrder));
            Assert.NotNull(contract.Members);

            var memberContractTypes = new Dictionary<string, Type>();
            memberContractTypes["OrderId"] = typeof(NumberContract);
            memberContractTypes["Name"] = typeof(StringContract);
            memberContractTypes["OrderDate"] = typeof(DateTimeContract);
            memberContractTypes["OrderType"] = typeof(EnumContract);
            memberContractTypes["Mobile"] = typeof(StringContract);
            memberContractTypes["Remark"] = typeof(StringContract);
            memberContractTypes["Attributes"] = typeof(StringContract);
            memberContractTypes["ClassLevel"] = typeof(NullableTypeContract);
            memberContractTypes["Amount"] = typeof(NumberContract);
            memberContractTypes["Rate"] = typeof(NumberContract);
            memberContractTypes["OrderId"] = typeof(NumberContract);
            memberContractTypes["OrderPoint"] = typeof(ObjectContract);
            memberContractTypes["Items"] = typeof(ArrayContract);
            memberContractTypes["Flags"] = typeof(ArrayContract);

            Assert.AreEqual(memberContractTypes.Count, contract.Members.Count);
            foreach (var m in contract.Members)
            {
                Assert.IsTrue(m.IsProperty);

                var propInfo = m.MemberInfo as PropertyInfo;
                Assert.NotNull(propInfo);

                Type contractType;
                Assert.IsTrue(memberContractTypes.TryGetValue(propInfo.Name, out contractType));
                Assert.IsInstanceOf(contractType, m.Contract);
                Assert.AreEqual(propInfo.Name, m.JsonPropertyName);
                Assert.AreEqual(propInfo.PropertyType, m.Type);
            }
        }

        [Test]
        public void ResoveObjectWithJsonPropertyAttributes()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<ObjectContract>(resolver, typeof(SaleOrderWithJsonAttr));
            Assert.NotNull(contract.Members);

            //value: contract type, json property name, is property
            var memberContractTypes = new Dictionary<string, TypeAndNameAndIsProperty>();
            memberContractTypes["OrderId"] = new TypeAndNameAndIsProperty(typeof(NumberContract), "order_id", true);
            memberContractTypes["_name"] = new TypeAndNameAndIsProperty(typeof(StringContract), "name", false);
            memberContractTypes["OrderType"] = new TypeAndNameAndIsProperty(typeof(EnumContract), "order_type", true);
            memberContractTypes["OrderDate"] = new TypeAndNameAndIsProperty(typeof(DateTimeContract), "order_date", true);
            memberContractTypes["ClassLevel"] = new TypeAndNameAndIsProperty(typeof(NullableTypeContract), "class_level", false);
            memberContractTypes["OrderPoint"] = new TypeAndNameAndIsProperty(typeof(ObjectContract), "order_point", true);
            memberContractTypes["Items"] = new TypeAndNameAndIsProperty(typeof(ArrayContract), "items", true);

            Assert.AreEqual(memberContractTypes.Count, contract.Members.Count);
            foreach (var m in contract.Members)
            {
                Console.WriteLine(m.Name);

                TypeAndNameAndIsProperty resultAssertion;
                Assert.IsTrue(memberContractTypes.TryGetValue(m.Name, out resultAssertion));
                Assert.IsInstanceOf(resultAssertion.Type, m.Contract);
                Assert.AreEqual(resultAssertion.Name, m.JsonPropertyName);
                Assert.AreEqual(resultAssertion.IsProperty, m.IsProperty);

                MemberInfo memberInfo = typeof(SaleOrderWithJsonAttr).GetProperty(
                    m.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                memberInfo = memberInfo ?? typeof(SaleOrderWithJsonAttr).GetField(
                    m.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Assert.AreEqual(memberInfo, m.MemberInfo);
            }
        }

        [Test]
        public void ResolveObjectWithJsonIgnoreAttribute()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<ObjectContract>(resolver, typeof(ClassWithJsonIgnoreAttr));
            Assert.NotNull(contract.Members);
            Assert.AreEqual(2, contract.Members.Count);

            ContractMemberInfo contractMemberInfo;
            Assert.IsTrue(contract.Members.TryGetValue("PublicProperty", out contractMemberInfo));
            Assert.IsInstanceOf<NumberContract>(contractMemberInfo.Contract);

            Assert.IsTrue(contract.Members.TryGetValue("PublicField", out contractMemberInfo));
            Assert.IsInstanceOf<NumberContract>(contractMemberInfo.Contract);
        }

        [Test]
        public void ResolveObjectWithMixedJsonAttributes()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<ObjectContract>(resolver, typeof(ClassWithMixedAttr));
            Assert.NotNull(contract.Members);
            Assert.AreEqual(2, contract.Members.Count);

            ContractMemberInfo contractMemberInfo;
            Assert.IsTrue(contract.Members.TryGetValue("pub_field", out contractMemberInfo));
            Assert.IsInstanceOf<NumberContract>(contractMemberInfo.Contract);

            Assert.IsTrue(contract.Members.TryGetValue("NoExplicicName", out contractMemberInfo));
            Assert.IsInstanceOf<GuidContract>(contractMemberInfo.Contract);
        }

        [Test]
        public void ResolveObjectWithReadOnlyWriteOnlyProperty()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<ObjectContract>(
                resolver, typeof(ClassWithReadOnlyWriteOnlyProperty));

            Assert.AreEqual(4, contract.Members.Count);

            ContractMemberInfo contractMemberInfo;
            Assert.IsTrue(contract.Members.TryGetValue("NoSetter", out contractMemberInfo));
            Assert.IsNotNull(contractMemberInfo.ValueGetter);
            Assert.IsNull(contractMemberInfo.ValueSetter);

            Assert.IsTrue(contract.Members.TryGetValue("NoGetter", out contractMemberInfo));
            Assert.IsNull(contractMemberInfo.ValueGetter);
            Assert.IsNotNull(contractMemberInfo.ValueSetter);

            Assert.IsTrue(contract.Members.TryGetValue("PrivateSetter", out contractMemberInfo));
            Assert.IsNotNull(contractMemberInfo.ValueGetter);
            Assert.IsNull(contractMemberInfo.ValueSetter);

            Assert.IsTrue(contract.Members.TryGetValue("ProtectedGetter", out contractMemberInfo));
            Assert.IsNull(contractMemberInfo.ValueGetter);
            Assert.IsNotNull(contractMemberInfo.ValueSetter);
        }

        [Test]
        public void ResolveObjectWithReadOnlyWriteOnlyPropertyAndJsonAttr()
        {
            var resolver = new JsonContractResolver();
            var contract = AssertTypeContract<ObjectContract>(
                resolver, typeof(ClassWithReadOnlyWriteOnlyPropertyAndJsonAttr));

            Assert.AreEqual(4, contract.Members.Count);

            ContractMemberInfo contractMemberInfo;
            Assert.IsTrue(contract.Members.TryGetValue("no_setter", out contractMemberInfo));
            Assert.IsNotNull(contractMemberInfo.ValueGetter);
            Assert.IsNull(contractMemberInfo.ValueSetter);

            Assert.IsTrue(contract.Members.TryGetValue("no_getter", out contractMemberInfo));
            Assert.IsNull(contractMemberInfo.ValueGetter);
            Assert.IsNotNull(contractMemberInfo.ValueSetter);

            Assert.IsTrue(contract.Members.TryGetValue("private_setter", out contractMemberInfo));
            Assert.IsNotNull(contractMemberInfo.ValueGetter);
            Assert.IsNotNull(contractMemberInfo.ValueSetter);

            Assert.IsTrue(contract.Members.TryGetValue("internal_getter", out contractMemberInfo));
            Assert.IsNotNull(contractMemberInfo.ValueGetter);
            Assert.IsNotNull(contractMemberInfo.ValueSetter);
        }

        [Test]
        public void ResolveCycleTypeReferenceStartFromA()
        {
            var resolver = new JsonContractResolver();
            var contractA = AssertTypeContract<ArrayContract>(resolver, typeof(A));
            var contractB = contractA.ElementContract as ObjectContract;
            Assert.NotNull(contractB);
            Assert.NotNull(contractB.Members);
            AssertContractDEFromB(contractB);

            ContractMemberInfo member;
            Assert.IsTrue(contractB.Members.TryGetValue("RefA", out member));
            Assert.AreEqual(contractA, member.Contract);

            Assert.IsTrue(contractB.Members.TryGetValue("RefC", out member));
            var contractC = member.Contract as ArrayContract;
            Assert.NotNull(contractC);
            Assert.AreEqual(contractA, contractC.ElementContract);
        }

        [Test]
        public void ResolveCycleTypeReferenceStartFromB()
        {
            var resolver = new JsonContractResolver();
            var contractB = AssertTypeContract<ObjectContract>(resolver, typeof(B));
            Assert.NotNull(contractB.Members);
            AssertContractDEFromB(contractB);

            ContractMemberInfo member;
            Assert.IsTrue(contractB.Members.TryGetValue("RefA", out member));
            var contractA = member.Contract as ArrayContract;
            Assert.NotNull(contractA);
            Assert.AreEqual(contractB, contractA.ElementContract);

            Assert.IsTrue(contractB.Members.TryGetValue("RefC", out member));
            var contractC = member.Contract as ArrayContract;
            Assert.NotNull(contractC);
            Assert.AreEqual(contractA, contractC.ElementContract);
        }

        [Test]
        public void ResolveCycleTypeReferenceStartFromC()
        {
            var resolver = new JsonContractResolver();
            var contractC = AssertTypeContract<ArrayContract>(resolver, typeof(C));
            Assert.NotNull(contractC);

            var contractA = contractC.ElementContract as ArrayContract;
            Assert.NotNull(contractA);

            var contractB = contractA.ElementContract as ObjectContract;
            Assert.NotNull(contractB);
            Assert.NotNull(contractB.Members);
            AssertContractDEFromB(contractB);

            ContractMemberInfo member;
            Assert.IsTrue(contractB.Members.TryGetValue("RefA", out member));
            Assert.AreEqual(contractA, member.Contract);

            Assert.IsTrue(contractB.Members.TryGetValue("RefC", out member));
            Assert.AreEqual(contractC, member.Contract);
        }

        private void AssertContractDEFromB(ObjectContract contractB)
        {
            ContractMemberInfo member;
            Assert.IsTrue(contractB.Members.TryGetValue("RefE", out member));
            var contractE = member.Contract as ObjectContract;
            Assert.NotNull(contractE);

            Assert.IsTrue(contractB.Members.TryGetValue("RefD", out member));
            var contractD = member.Contract as ObjectContract;
            Assert.NotNull(contractD);

            Assert.IsTrue(contractD.Members.TryGetValue("RefE", out member));
            Assert.AreEqual(contractE, member.Contract);

            Assert.IsTrue(contractE.Members.TryGetValue("RefD", out member));
            Assert.AreEqual(contractD, member.Contract);
        }

        private TContract AssertTypeContract<TContract>(JsonContractResolver resolver, Type type)
            where TContract : JsonContract
        {
            var contract = resolver.ResolveContract(type);
            Assert.IsInstanceOf<TContract>(contract);
            return (TContract)contract;
        }

        private class TypeAndNameAndIsProperty
        {
            public Type Type { get; private set; }
            public string Name { get; private set; }
            public bool IsProperty { get; private set; }

            public TypeAndNameAndIsProperty(Type type, string name, bool isProperty)
            {
                Type = type;
                Name = name;
                IsProperty = isProperty;
            }
        }

        private class A : List<B>
        {
        }

        private class B
        {
            public A RefA { get; set; }
            public C RefC { get; set; }
            public D RefD { get; set; }
            public E RefE { get; set; }
        }

        private class C : List<A>
        {
        }

        private class D
        {
            public E RefE { get; set; }
        }

        private class E
        {
            public D RefD { get; set; }
        }

        private class Collection : CollectionBase
        {
        }

        private class ClassWithJsonIgnoreAttr
        {
            public int PublicProperty { get; set; }
            public int PublicField { get; set; }

            [JsonIgnore]
            public int IngoredProperty { get; set; }

            [JsonIgnore]
            public int IngoredPropertyWithJsonProperty { get; set; }
        }

        private class ClassWithMixedAttr
        {
            public int PublicProperty { get; set; }

            [JsonProperty("pub_field")]
            public int PublicField { get; set; }

            [JsonIgnore]
            public int IngoredProperty { get; set; }

            [JsonProperty("ignored"), JsonIgnore]
            public int IngoredPropertyWithJsonProperty { get; set; }

            [JsonProperty]
            public Guid NoExplicicName { get; set; }
        }

        private class ClassWithReadOnlyWriteOnlyProperty
        {
            public int NoSetter { get { return 1; } }
            public int NoGetter { set { } }
            public int PrivateSetter { get; private set; }
            public int ProtectedGetter { protected get; set; }
        }

        private class ClassWithReadOnlyWriteOnlyPropertyAndJsonAttr
        {
            [JsonProperty("no_setter")]
            public int NoSetter { get { return 1; } }

            [JsonProperty("no_getter")]
            public int NoGetter { set { } }

            [JsonProperty("private_setter")]
            public int PrivateSetter { get; private set; }

            [JsonProperty("internal_getter")]
            public int InternalGetter { internal get; set; }
        }
    }
}
