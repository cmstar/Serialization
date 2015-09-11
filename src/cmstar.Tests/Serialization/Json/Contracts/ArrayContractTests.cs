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
using System.Linq;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public class ArrayContractTests
    {
        private interface IIntList : IList<int> { }
        private interface IListD : IList { }

        [Test]
        public void TestUnderlyingTypeValidation()
        {
            //generic
            Assert.DoesNotThrow(() => new ArrayContract(typeof(int[])));
            Assert.DoesNotThrow(() => new ArrayContract(typeof(string[])));
            Assert.DoesNotThrow(() => new ArrayContract(typeof(List<int>)));
            Assert.DoesNotThrow(() => new ArrayContract(typeof(LinkedList<int>)));
            Assert.DoesNotThrow(() => new ArrayContract(typeof(IList<int>)));
            Assert.DoesNotThrow(() => new ArrayContract(typeof(ICollection<string>)));

            Assert.Throws<ArgumentException>(() => new ArrayContract(typeof(IIntList)));

            //non-generic
            Assert.DoesNotThrow(() => new ArrayContract(typeof(ArrayList)));
            Assert.DoesNotThrow(() => new ArrayContract(typeof(CollectionBase)));
            Assert.DoesNotThrow(() => new ArrayContract(typeof(IList)));
            Assert.DoesNotThrow(() => new ArrayContract(typeof(ICollection)));

            Assert.Throws<ArgumentException>(() => new ArrayContract(typeof(IListD)));
            Assert.Throws<ArgumentException>(() => new ArrayContract(typeof(object)));
        }
    }

    [TestFixture]
    public class FloatArrayArrayContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(float[]); }
        }

        [Test]
        public void WriteEmptyArray()
        {
            var result = DoWrite(new float[0]);
            var expected = @"[]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadEmptyArray()
        {
            var result = DoRead("[]");
            var castResult = result as float[];

            Assert.NotNull(castResult);
            Assert.AreEqual(0, castResult.Length);
        }

        [Test]
        public void WriteArray()
        {
            var result = DoWrite(new[] { 12F, 0.33F, -9.2F });
            var expected = "[12,0.33,-9.2]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadArray()
        {
            var json = "[12,0.33,-9.2]";
            var result = DoRead(json);
            var castResult = result as float[];

            Assert.NotNull(castResult);
            Assert.AreEqual(3, castResult.Length);
            Assert.AreEqual(12F, castResult[0]);
            Assert.AreEqual(0.33F, castResult[1]);
            Assert.AreEqual(-9.2F, castResult[2]);
        }
    }

    [TestFixture]
    public class StringListArrayContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(List<string>); }
        }

        [Test]
        public void WriteList()
        {
            var result = DoWrite(new List<string> { "value1", "value2", "" }, true);
            var expected =
@"[
    ""value1"",
    ""value2"",
    """"
]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadList()
        {
            var json =
@"[
    ""value1"",
    ""value2"",
    """"
]";
            var result = DoRead(json);
            var castResult = result as List<string>;

            Assert.NotNull(castResult);
            Assert.AreEqual(3, castResult.Count);
            Assert.AreEqual("value1", castResult[0]);
            Assert.AreEqual("value2", castResult[1]);
            Assert.AreEqual("", castResult[2]);
        }
    }

    [TestFixture]
    public class CustomColletionArrayContractTests : ContractTestBase
    {
        private class CustomCollection : List<int> { }

        protected override Type UnderlyingType
        {
            get { return typeof(CustomCollection); }
        }

        [Test]
        public void WriteEmptyCollection()
        {
            var result = DoWrite(new CustomCollection());
            var expected = @"[]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadEmptyCollection()
        {
            var result = DoRead("[]");
            var castResult = result as CustomCollection;

            Assert.NotNull(castResult);
            Assert.AreEqual(0, castResult.Count);
        }

        [Test]
        public void WriteCollection()
        {
            var result = DoWrite(new CustomCollection { -1, 0, 1 });
            var expected = "[-1,0,1]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadCollection()
        {
            var json = @"[-1,0,1]";
            var result = DoRead(json);
            var castResult = result as CustomCollection;

            Assert.NotNull(castResult);
            Assert.AreEqual(3, castResult.Count);
            Assert.AreEqual(-1, castResult[0]);
            Assert.AreEqual(0, castResult[1]);
            Assert.AreEqual(1, castResult[2]);
        }
    }

    [TestFixture]
    public class NonGenericColletionArrayContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(ArrayList); }
        }

        [Test]
        public void WriteEmptyCollection()
        {
            var result = DoWrite(new ArrayList());
            var expected = @"[]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadEmptyCollection()
        {
            var result = DoRead("[]");
            var castResult = result as ArrayList;

            Assert.NotNull(castResult);
            Assert.AreEqual(0, castResult.Count);
        }

        [Test]
        public void WriteCollection()
        {
            var result = DoWrite(new ArrayList { 123, "str", new object() });
            var expected = "[123,\"str\",{}]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadNonemptyCollectionThrowsExcpetion()
        {
            Assert.Throws<JsonContractException>(() => DoRead("[1]"));
        }
    }

    [TestFixture]
    public class GenericInterfaceArrayContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(ICollection<float>); }
        }

        [Test]
        public void WriteColelction()
        {
            var result = DoWrite(new[] { 12F, 0.33F, -9.2F });
            var expected = "[12,0.33,-9.2]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadCollection()
        {
            var json = "[12,0.33,-9.2]";
            var result = DoRead(json);
            var castResult = result as ICollection<float>;

            Assert.NotNull(castResult);
            Assert.AreEqual(3, castResult.Count);

            int i = 0;
            foreach (var value in castResult)
            {
                switch (i)
                {
                    case 0:
                        Assert.AreEqual(12F, value);
                        break;

                    case 1:
                        Assert.AreEqual(0.33F, value);
                        break;

                    case 2:
                        Assert.AreEqual(-9.2F, value);
                        break;
                }

                i++;
            }
        }
    }

    [TestFixture]
    public class NonGenericInterfaceArrayContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(IList); }
        }

        [Test]
        public void ReadEmptyCollection()
        {
            var result = DoRead("[]");
            var castResult = result as ArrayList;

            Assert.NotNull(castResult);
            Assert.AreEqual(0, castResult.Count);
        }

        [Test]
        public void WriteCollection()
        {
            var result = DoWrite(new ArrayList { 123, "str", new object() });
            var expected = "[123,\"str\",{}]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadNonemptyCollectionThrowsExcpetion()
        {
            Assert.Throws<JsonContractException>(() => DoRead("[1]"));
        }
    }

    [TestFixture]
    public class EnumerableClassContractTests : ContractTestBase
    {
        private class D : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return "1";
                yield return "2";
                yield return "3";
            }
        }

        protected override Type UnderlyingType
        {
            get { return typeof(D); }
        }

        [Test]
        public void ReadEmptyCollection()
        {
            var result = DoRead("[]");
            var castResult = result as D;

            Assert.NotNull(castResult);
        }

        [Test]
        public void WriteCollection()
        {
            var result = DoWrite(new D());
            var expected = "[\"1\",\"2\",\"3\"]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadNonemptyCollectionThrowsExcpetion()
        {
            Assert.Throws<JsonContractException>(() => DoRead("[1]"));
        }
    }

    [TestFixture]
    public class GenericEnumerableClassContractTests : ContractTestBase
    {
        private class D : IEnumerable<string>
        {
            public IEnumerator<string> GetEnumerator()
            {
                yield return "1";
                yield return "2";
                yield return "3";
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        protected override Type UnderlyingType
        {
            get { return typeof(D); }
        }

        [Test]
        public void ReadEmptyCollection()
        {
            var result = DoRead("[]");
            var castResult = result as D;

            Assert.NotNull(castResult);
        }

        [Test]
        public void WriteCollection()
        {
            var result = DoWrite(new D());
            var expected = "[\"1\",\"2\",\"3\"]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadNonemptyCollectionThrowsExcpetion()
        {
            Assert.Throws<JsonContractException>(() => DoRead("[1]"));
        }
    }

    [TestFixture]
    public class EnumerableInterfaceContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(IEnumerable); }
        }

        [Test]
        public void ReadEmptyCollection()
        {
            var result = DoRead("[]");
            var castResult = result as IEnumerable;

            Assert.NotNull(castResult);
            Assert.AreEqual(0, castResult.Cast<object>().Count());
        }

        [Test]
        public void WriteCollection()
        {
            var result = DoWrite(new ArrayList { 123, "str", new object() });
            var expected = "[123,\"str\",{}]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void WriteEnumerable()
        {
            var result = DoWrite("123x".Select(x => x));
            var expected = "[\"1\",\"2\",\"3\",\"x\"]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadNonemptyCollectionThrowsExcpetion()
        {
            Assert.Throws<JsonContractException>(() => DoRead("[1]"));
        }
    }

    [TestFixture]
    public class GenericEnumerableInterfaceContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(IEnumerable<string>); }
        }

        [Test]
        public void ReadEmptyCollection()
        {
            var result = DoRead("[]");
            var castResult = result as IEnumerable<string>;

            Assert.NotNull(castResult);
            Assert.AreEqual(0, castResult.Count());
        }

        [Test]
        public void WriteCollection()
        {
            var result = DoWrite(new[] { "1", "abc", "gg" });
            var expected = "[\"1\",\"abc\",\"gg\"]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void WriteEnumerable()
        {
            var result = DoWrite("123x".Select(x => x));
            var expected = "[\"1\",\"2\",\"3\",\"x\"]";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReadCollection()
        {
            var json = "[\"1\",\"abc\",\"gg\"]";
            var result = DoRead(json);
            var castResult = result as IEnumerable<string>;

            Assert.NotNull(castResult);
            Assert.AreEqual(3, castResult.Count());

            int i = 0;
            foreach (var value in castResult)
            {
                switch (i)
                {
                    case 0:
                        Assert.AreEqual("1", value);
                        break;

                    case 1:
                        Assert.AreEqual("abc", value);
                        break;

                    case 2:
                        Assert.AreEqual("gg", value);
                        break;
                }

                i++;
            }
        }
    }
}