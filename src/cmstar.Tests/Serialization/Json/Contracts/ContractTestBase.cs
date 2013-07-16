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
using System.IO;
using System.Text;
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public abstract class ContractTestBase
    {
        protected abstract Type UnderlyingType { get; }

        protected virtual bool SupportsNullValue
        {
            get { return true; }
        }

        [Test]
        public void WriteNull()
        {
            if (SupportsNullValue)
            {
                var result = DoWrite(null);
                Assert.AreEqual("null", result);
            }
            else
            {
                Assert.Pass();
            }
        }

        [Test]
        public void ReadNull()
        {
            if (SupportsNullValue)
            {
                var result = DoRead("null");
                Assert.IsNull(result);
            }
            else
            {
                Assert.Pass();
            }
        }

        protected virtual JsonContract GetContarct()
        {
            var contractResolver = new JsonContractResolver();
            var contract = contractResolver.ResolveContract(UnderlyingType);
            return contract;
        }

        protected string DoWrite(object obj, bool formatResult = false)
        {
            return DoWrite(obj, new JsonSerializingState(), formatResult);
        }

        protected string DoWrite(object obj, JsonSerializingState state, bool formatResult = false)
        {
            var sb = new StringBuilder();

            using (var textWriter = new IndentedTextWriter(new StringWriter(sb)))
            {
                if (!formatResult)
                {
                    textWriter.IndentMark = string.Empty;
                    textWriter.NewLine = string.Empty;
                }

                using (var jsonWriter = new JsonWriterImproved(textWriter))
                {
                    var resolver = new JsonContractResolver();
                    GetContarct().Write(jsonWriter, state, resolver, obj);
                }
            }

            return sb.ToString();
        }

        protected object DoRead(string json)
        {
            return DoRead(json, new JsonDeserializingState());
        }

        protected object DoRead(string json, JsonDeserializingState state)
        {
            using (var reader = new JsonReader(new StringReader(json)))
            {
                var result = GetContarct().Read(reader, state);
                return result;
            }
        }
    }
}
