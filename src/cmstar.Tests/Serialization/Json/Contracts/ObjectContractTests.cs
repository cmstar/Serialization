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
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public class PocoObjectContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(SaleOrder); }
        }

        private SaleOrder _example = SaleOrder.CreateExample();
        private string _expected =
@"{
    ""OrderId"":1234567890123456,
    ""Name"":""Someone"",
    ""OrderDate"":""\/Date(1337472000000)\/"",
    ""OrderType"":2,
    ""Mobile"":""1234567890"",
    ""Remark"":""\""Hello\r\nWorld!\"""",
    ""Attributes"":null,
    ""ClassLevel"":null,
    ""Amount"":1024.567,
    ""Rate"":0.56,
    ""OrderPoint"":{
        ""Level"":2,
        ""Quantity"":335
    },
    ""Items"":[
        {
            ""ProductNo"":""ProductNo1"",
            ""Quantity"":3,
            ""Price"":23.3
        },{
            ""ProductNo"":""ProductNo2"",
            ""Quantity"":1,
            ""Price"":26
        },{
            ""ProductNo"":""ProductNo3"",
            ""Quantity"":2,
            ""Price"":788.45
        }
    ]
}";

        [Test]
        public void WriteObject()
        {
            var json = DoWrite(_example, true);
            Assert.AreEqual(_expected, json);
        }

        [Test]
        public void ReadObject()
        {
            var result = DoRead(_expected);
            Assert.IsInstanceOf<SaleOrder>(result);
            Assert.IsTrue(_example.Equals((SaleOrder)result));
        }

        [Test]
        public void ReadEmptyObject()
        {
            var result = DoRead("{}");
            Assert.IsInstanceOf<SaleOrder>(result);
        }

        [Test]
        public void ReadMissingProperty()
        {
            var result = DoRead("{\"Mobile\":\"1234567890\"}");
            Assert.IsInstanceOf<SaleOrder>(result);
        }

        [Test]
        public void ReadUnknownProperty()
        {
            var result = DoRead("{\"XXXX\":\"xxxx\"}");
            Assert.IsInstanceOf<SaleOrder>(result);
        }
    }

    [TestFixture]
    public class JsonPropertyAttributeObjectContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(SaleOrderWithJsonAttr); }
        }

        [Test]
        public void WriteAndReadObject()
        {
            //since the property order in the JSON is not absolute,
            //we just serialize the object then deserialize and comapare
            //the original object with the deserialized result
            var example = CreateExample();
            var json = DoWrite(example, true);

            var result = DoRead(json);
            Assert.IsInstanceOf<SaleOrderWithJsonAttr>(result);
            Assert.IsTrue(example.Equals((SaleOrderWithJsonAttr)result));
        }

        private SaleOrderWithJsonAttr CreateExample()
        {
            var order = new SaleOrderWithJsonAttr("the name", new SaleOrderPoint { Level = 3, Quantity = 5 });
            order.Mobile = "123456";
            order.OrderDate = new DateTime(2012, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc);
            order.OrderId = 9941248795;
            order.OrderType = SaleOrderType.Emergency;
            order.Rate = 1.5F;
            order.Remark = "\"Hello\r\nWorld!\"";
            order.Items = new List<SaleOrderItem>
            {
                new SaleOrderItem {ProductNo = "the product_no", Quantity = 3, Price = 123.4M},
            };

            return order;
        }
    }

    [TestFixture]
    public class AnonymousObjectContractTests : ContractTestBase
    {
        private object _anonymousObject;
        private string _expected =
@"{
    ""Int"":123,
    ""String"":""s"",
    ""Array"":[
        1,
        2,
        3
    ]
}";

        protected override Type UnderlyingType
        {
            get { return _anonymousObject.GetType(); }
        }

        [SetUp]
        public void Setup()
        {
            _anonymousObject = new
            {
                Int = 123,
                String = "s",
                Array = new[] { 1, 2, 3 }
            };
        }

        [Test]
        public void WriteObject()
        {
            var json = DoWrite(_anonymousObject, true);
            Assert.AreEqual(_expected, json);
        }

        [Test]
        public void ReadObject()
        {
            var result = DoRead(_expected);
            Assert.IsInstanceOf(_anonymousObject.GetType(), result);

            var type = result.GetType();
            var intValue = type.GetProperty("Int").GetValue(result, null);
            Assert.AreEqual(123, intValue);

            var stringValue = type.GetProperty("String").GetValue(result, null);
            Assert.AreEqual("s", stringValue);

            var arrayValue = (int[])type.GetProperty("Array").GetValue(result, null);
            Assert.AreEqual(3, arrayValue.Length);
        }

        [Test]
        public void ReadMissingProperty()
        {
            //miss a value type
            var jsonWithoutInt = "{\"String\":\"s\"}";
            var result = DoRead(jsonWithoutInt);

            var type = result.GetType();
            var intValue = type.GetProperty("Int").GetValue(result, null);
            Assert.AreEqual(0, intValue);

            var stringValue = type.GetProperty("String").GetValue(result, null);
            Assert.AreEqual("s", stringValue);

            //miss a reference type
            var jsonWithoutString = "{\"Int\":123}";
            result = DoRead(jsonWithoutString);

            intValue = type.GetProperty("Int").GetValue(result, null);
            Assert.AreEqual(123, intValue);

            stringValue = type.GetProperty("String").GetValue(result, null);
            Assert.AreEqual(null, stringValue);
        }
    }

    [TestFixture]
    public class StructObjectContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof(CustomStruct); }
        }

        private CustomStruct _example = new CustomStruct { S = "s", I = 123 };
        private string _expected =
@"{
    ""S"":""s"",
    ""I"":123
}";

        [Test]
        public void WriteStruct()
        {
            var json = DoWrite(_example, true);
            Assert.AreEqual(_expected, json);
        }

        [Test]
        public void ReadStruct()
        {
            var result = DoRead(_expected);
            Assert.IsInstanceOf<CustomStruct>(result);
            Assert.IsTrue(_example.Equals(result));
        }

        private struct CustomStruct
        {
            public string S;
            public int I;
        }
    }

    [TestFixture]
    public class DerivedObjectContractTests : ContractTestBase
    {
        protected override Type UnderlyingType
        {
            get { return typeof (object); }
        }

        private string _expected =
@"{
    ""S"":""s"",
    ""I"":123
}";

        [Test]
        public void WriteObject()
        {
            var json = DoWrite(new { S = "s", I = 123 }, true);
            Assert.AreEqual(_expected, json);
        }

        [Test]
        public void ReadObject()
        {
            var result = DoRead(_expected);
            Assert.IsInstanceOf(UnderlyingType, result);
        }
    }
}