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
            var memberContractTypes = new Dictionary<string, Tuple<Type, string, bool>>();
            memberContractTypes["OrderId"] = new Tuple<Type, string, bool>(typeof(NumberContract), "order_id", true);
            memberContractTypes["_name"] = new Tuple<Type, string, bool>(typeof(StringContract), "name", false);
            memberContractTypes["OrderType"] = new Tuple<Type, string, bool>(typeof(EnumContract), "order_type", true);
            memberContractTypes["OrderDate"] = new Tuple<Type, string, bool>(typeof(DateTimeContract), "order_date", true);
            memberContractTypes["ClassLevel"] = new Tuple<Type, string, bool>(typeof(NullableTypeContract), "class_level", false);
            memberContractTypes["OrderPoint"] = new Tuple<Type, string, bool>(typeof(ObjectContract), "order_point", true);
            memberContractTypes["Items"] = new Tuple<Type, string, bool>(typeof(ArrayContract), "items", true);

            Assert.AreEqual(memberContractTypes.Count, contract.Members.Count);
            foreach (var m in contract.Members)
            {
                Console.WriteLine(m.Name);

                Tuple<Type, string, bool> resultAssertion;
                Assert.IsTrue(memberContractTypes.TryGetValue(m.Name, out resultAssertion));
                Assert.IsInstanceOf(resultAssertion.Item1, m.Contract);
                Assert.AreEqual(resultAssertion.Item2, m.JsonPropertyName);
                Assert.AreEqual(resultAssertion.Item3, m.IsProperty);

                MemberInfo memberInfo = typeof(SaleOrderWithJsonAttr).GetProperty(
                    m.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                memberInfo = memberInfo ?? typeof(SaleOrderWithJsonAttr).GetField(
                    m.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Assert.AreEqual(memberInfo, m.MemberInfo);
            }
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
            Assert.IsTrue(contractB.Members.TryGetContractMember("RefA", out member));
            Assert.AreEqual(contractA, member.Contract);

            Assert.IsTrue(contractB.Members.TryGetContractMember("RefC", out member));
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
            Assert.IsTrue(contractB.Members.TryGetContractMember("RefA", out member));
            var contractA = member.Contract as ArrayContract;
            Assert.NotNull(contractA);
            Assert.AreEqual(contractB, contractA.ElementContract);

            Assert.IsTrue(contractB.Members.TryGetContractMember("RefC", out member));
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
            Assert.IsTrue(contractB.Members.TryGetContractMember("RefA", out member));
            Assert.AreEqual(contractA, member.Contract);

            Assert.IsTrue(contractB.Members.TryGetContractMember("RefC", out member));
            Assert.AreEqual(contractC, member.Contract);
        }

        private void AssertContractDEFromB(ObjectContract contractB)
        {
            ContractMemberInfo member;
            Assert.IsTrue(contractB.Members.TryGetContractMember("RefE", out member));
            var contractE = member.Contract as ObjectContract;
            Assert.NotNull(contractE);

            Assert.IsTrue(contractB.Members.TryGetContractMember("RefD", out member));
            var contractD = member.Contract as ObjectContract;
            Assert.NotNull(contractD);

            Assert.IsTrue(contractD.Members.TryGetContractMember("RefE", out member));
            Assert.AreEqual(contractE, member.Contract);

            Assert.IsTrue(contractE.Members.TryGetContractMember("RefD", out member));
            Assert.AreEqual(contractD, member.Contract);
        }

        private TContract AssertTypeContract<TContract>(JsonContractResolver resolver, Type type)
            where TContract : JsonContract
        {
            var contract = resolver.ResolveContract(type);
            Assert.IsInstanceOf<TContract>(contract);
            return (TContract)contract;
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
    }
}
