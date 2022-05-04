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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace cmstar.Serialization.Json
{
    [TestFixture]
    public class JsonSerializerTests
    {
        [Test]
        public void Format()
        {
            var input =
@"{A:'value', 'B':123
, ""C"":  null, 'D':
undefined
}";

            // Indented
            var expected =
@"{
    ""A"":""value"",
    ""B"":123,
    ""C"":null,
    ""D"":undefined
}";
            var output = JsonSerializer.Default.Format(input, Formatting.Indented);
            Assert.AreEqual(expected, output);

            // Multiple
            expected =
@"{
""A"":""value"",
""B"":123,
""C"":null,
""D"":undefined
}";
            output = JsonSerializer.Default.Format(input, Formatting.Multiple);
            Assert.AreEqual(expected, output);

            // None
            expected = @"{""A"":""value"",""B"":123,""C"":null,""D"":undefined}";
            output = JsonSerializer.Default.Format(input, Formatting.None);
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void FormatCustom()
        {
            var input =
@"{A:'value', 'B':123
, ""C"":  null, 'D':
undefined
}";

            var expected = "{\t  \"A\":\"value\",\t  \"B\":123,\t  \"C\":null,\t  \"D\":undefined\t}";
            var output = JsonSerializer.Default.Format(input, "  ", "\t");
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void SerializeDictionaryWithObjectValues()
        {
            var order = SaleOrder.CreateExample();
            var dic = new Dictionary<string, object>();
            dic.Add("k1", order);
            dic.Add("k2", 2.22);

            var innerDic = new Dictionary<string, object>();
            innerDic.Add("k1", 1);
            innerDic.Add("k2", order);
            innerDic.Add("k4", new[] { "a1", "a2", "a3" });

            dic.Add("k3", innerDic);
            dic.Add("k4", new[] { "a1", "a2", "a3" });

            var json = DoSerialize(dic);
            var expected =
@"{
    ""k1"":{
        ""OrderId"":1234567890123456,
        ""Name"":""Someone"",
        ""OrderDate"":""2012-05-20T00:00:00.0000000Z"",
        ""OrderType"":2,
        ""TypeFlag"":""D"",
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
        ],
        ""Flags"":[
            1,
            3,
            4,
            11
        ]
    },
    ""k2"":2.22,
    ""k3"":{
        ""k1"":1,
        ""k2"":{
            ""OrderId"":1234567890123456,
            ""Name"":""Someone"",
            ""OrderDate"":""2012-05-20T00:00:00.0000000Z"",
            ""OrderType"":2,
            ""TypeFlag"":""D"",
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
            ],
            ""Flags"":[
                1,
                3,
                4,
                11
            ]
        },
        ""k4"":[
            ""a1"",
            ""a2"",
            ""a3""
        ]
    },
    ""k4"":[
        ""a1"",
        ""a2"",
        ""a3""
    ]
}";
            Assert.AreEqual(expected, json);
        }

        [Test]
        public void DeserializeAnonymousObject()
        {
            var template = new { Foo = "foooo", Bar = 3.1415926 };
            var json = "{\"Foo\":\"foooo\",\"Bar\":3.1415926}";
            var s = new JsonSerializer();
            var result = s.DeserializeByTemplate(json, template);
            Assert.AreEqual(template, result);
        }

        [Test]
        public void SerializeWithCustomState()
        {
            var json = "{ Level: null, Quantity: 3 }";
            var state = new JsonDeserializingState
            {
                NullValueHandling = JsonDeserializationNullValueHandling.AsDefaultValue
            };
            var result = JsonSerializer.Default.Deserialize<SaleOrderPoint>(json, state);
            var expected = new SaleOrderPoint { Level = 0, Quantity = 3 };
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CheckCycleReference()
        {
            var c = new C();
            Assert.DoesNotThrow(() => DoSerialize(c, true));
            c.Other = c;
            Assert.Throws<JsonContractException>(() => DoSerialize(c, true));

            var c2 = new C { Other = c };
            c.Other = c2;
            Assert.Throws<JsonContractException>(() => DoSerialize(c, true));

            var arrayList = new ArrayList();
            arrayList.Add(1);
            arrayList.Add(arrayList);
            Assert.Throws<JsonContractException>(() => DoSerialize(arrayList, true));

            var dic = new Dictionary<string, object>();
            dic.Add("1", null);
            Assert.DoesNotThrow(() => DoSerialize(dic, true));
            dic.Add("2", dic);
            Assert.Throws<JsonContractException>(() => DoSerialize(dic, true));
        }

        private class C
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public C Other { get; set; }
        }

        private string DoSerialize(object obj, bool checkCycleReference = false)
        {
            var s = new JsonSerializer { CheckCycleReference = checkCycleReference };
            var sb = new StringBuilder();
            var writer = new JsonWriterImproved(new IndentedTextWriter(new StringWriter(sb)));
            s.Serialize(obj, writer);
            return sb.ToString();
        }
    }
}