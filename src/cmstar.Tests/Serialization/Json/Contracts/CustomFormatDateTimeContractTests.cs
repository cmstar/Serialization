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
using NUnit.Framework;

namespace cmstar.Serialization.Json.Contracts
{
    [TestFixture]
    public class CustomFormatDateTimeContractTests : DateTimeContractTests
    {
        protected override JsonContract GetContract()
        {
            var contract = new CustomFormatDateTimeContract();
            contract.Format = "yyyy@M@dd HH~~mm~~ss ffffff";
            return contract;
        }

        public override void Write()
        {
            var date = new DateTime(2013, 1, 25, 12, 26, 33, 123);
            var json = DoWrite(date);
            Assert.AreEqual("\"2013@1@25 12~~26~~33 123000\"", json);

            date = new DateTime(2012, 3, 15, 6, 25, 35, 152);
            json = DoWrite(date);
            Assert.AreEqual("\"2012@3@15 06~~25~~35 152000\"", json);

            date = new DateTime(1976, 12, 2, 23, 42, 25, 0);
            json = DoWrite(date);
            Assert.AreEqual("\"1976@12@02 23~~42~~28 000000\"", json);
        }
    }
}