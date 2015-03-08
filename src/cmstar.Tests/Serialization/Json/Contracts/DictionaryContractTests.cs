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
using System.ComponentModel;
using System.Globalization;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public class SimpleDictionaryContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(Dictionary<string, string>); }
        }

        string _expected =
@"{
    ""k1"":""v1"",
    ""k2"":""v2"",
    ""k3"":""v3"",
    ""k4"":""v4""
}";

        [Test]
        public void Write()
        {
            var dic = new Dictionary<string, string>();
            dic.Add("k1", "v1");
            dic.Add("k2", "v2");
            dic.Add("k3", "v3");
            dic.Add("k4", "v4");

            var json = DoWrite(dic, true);
            Assert.AreEqual(_expected, json);
        }

        [Test]
        public void Read()
        {
            var result = DoRead(_expected);
            var deserializedDic = result as Dictionary<string, string>;
            Assert.NotNull(deserializedDic);
            Assert.AreEqual(4, deserializedDic.Count);

            for (int i = 1; i <= 4; i++)
            {
                string value;
                Assert.IsTrue(deserializedDic.TryGetValue("k" + i, out value));
                Assert.AreEqual("v" + i, value);
            }
        }
    }

    [TestFixture]
    public class InterfaceDictionaryContractTests : SimpleDictionaryContractTests
    {
        protected override Type UnderlyingType
        {
            get { return typeof(IDictionary<string, string>); }
        }
    }

    [TestFixture]
    public class NonGenericDictionaryContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(Hashtable); }
        }

        [Test]
        public void Write()
        {
            var hashtable = new Hashtable();
            hashtable.Add(1, "v1");
            var json = DoWrite(hashtable);
            var expected = @"{""1"":""v1""}";
            Assert.AreEqual(expected, json);

            hashtable.Clear();
            hashtable.Add("k2", -2);
            json = DoWrite(hashtable);
            expected = @"{""k2"":-2}";
            Assert.AreEqual(expected, json);

            hashtable.Clear();
            hashtable.Add(3.14, new object());
            json = DoWrite(hashtable);
            expected = @"{""3.14"":{}}";
            Assert.AreEqual(expected, json);
        }

        [Test]
        public void Read()
        {
            Assert.Throws<JsonContractException>(() => DoRead("{}"));
        }
    }

    [TestFixture]
    public class KeyConvertiableDictionaryContractTests : ContractTestBase
    {
        private class CustomKey
        {
            private readonly int _x;
            private readonly int _y;

            public CustomKey(int x, int y)
            {
                _x = x;
                _y = y;
            }

            public override string ToString()
            {
                return string.Format("{0}_{1}", _x, _y);
            }

            public override int GetHashCode()
            {
                return _x ^ _y;
            }

            public override bool Equals(object obj)
            {
                var other = obj as CustomKey;
                if (other == null)
                    return false;

                return other._x == _x && other._y == _y;
            }
        }

        private class CustomKeyConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(string);
            }

            public override object ConvertFrom(
                ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                var s = value as string;
                if (s == null)
                    return null;

                var xy = s.Split(new[] { '_' });
                if (xy.Length != 2)
                    return null;

                var x = Convert.ToInt32(xy[0]);
                var y = Convert.ToInt32(xy[1]);
                return new CustomKey(x, y);
            }

            public override object ConvertTo(
                ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var customKey = value as CustomKey;
                return customKey == null ? string.Empty : customKey.ToString();
            }
        }

        private class CustomDic : Dictionary<CustomKey, int> { }

        protected override Type UnderlyingType
        {
            get { return typeof(CustomDic); }
        }

        protected override JsonContract GetContarct()
        {
            var contract = (DictionaryContract)base.GetContarct();
            contract.KeyConverter = new CustomKeyConverter();
            return contract;
        }

        string _expected =
@"{
    ""1_2"":12,
    ""3_4"":34,
    ""5_6"":56
}";

        [Test]
        public void Write()
        {
            var dic = new CustomDic();
            dic.Add(new CustomKey(1, 2), 12);
            dic.Add(new CustomKey(3, 4), 34);
            dic.Add(new CustomKey(5, 6), 56);

            var json = DoWrite(dic, true);
            Assert.AreEqual(_expected, json);
        }

        [Test]
        public void Read()
        {
            var result = DoRead(_expected);
            var dic = result as CustomDic;
            Assert.NotNull(dic);
            Assert.AreEqual(12, dic[new CustomKey(1, 2)]);
            Assert.AreEqual(34, dic[new CustomKey(3, 4)]);
            Assert.AreEqual(56, dic[new CustomKey(5, 6)]);
        }
    }

    public class ObjectValueGenericDictionaryContractTest : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(Dictionary<string, object>); }
        }

        [Test]
        public void Write()
        {
            var dic = new Dictionary<string, object>();
            dic.Add("k1", 35);
            dic.Add("k2", "s");
            dic.Add("k3", new { x = 3, y = "v" });

            var expected =
@"{
    ""k1"":35,
    ""k2"":""s"",
    ""k3"":{
        ""x"":3,
        ""y"":""v""
    }
}";
            var json = DoWrite(dic, true);
            Assert.AreEqual(expected, json);
        }
    }
}