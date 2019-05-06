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
using cmstar.Serialization.Json;

namespace cmstar.Serialization
{
    public class SaleOrderWithJsonAttr : IEquatable<SaleOrderWithJsonAttr>
    {
        public SaleOrderWithJsonAttr()
        {
        }

        public SaleOrderWithJsonAttr(string name, SaleOrderPoint saleOrderPoint)
        {
            _name = name;
            OrderPoint = saleOrderPoint;
        }

        [JsonProperty("order_id")]
        public long OrderId { get; set; }

        [JsonProperty("name")]
        private string _name;

        [JsonProperty("order_date")]
        public DateTime OrderDate { get; set; }

        [JsonProperty("order_type")]
        public SaleOrderType OrderType { get; set; }

        [JsonProperty("class_level")]
        public int? ClassLevel;

        [JsonProperty("order_point")]
        private SaleOrderPoint OrderPoint { get; set; }

        [JsonProperty("items")]
        public List<SaleOrderItem> Items { get; set; }

        public string Mobile { get; set; }
        public string Remark { get; set; }
        public string Attributes { get; set; }
        public decimal Amount { get; set; }
        public float Rate { get; set; }

        public bool Equals(SaleOrderWithJsonAttr other)
        {
            if (other == null)
                return false;

            if (OrderId != other.OrderId
                || _name != other._name
                || OrderType != other.OrderType
                || ClassLevel != other.ClassLevel)
            {
                return false;
            }

            if (TimeZone.CurrentTimeZone.ToUniversalTime(other.OrderDate)
                != TimeZone.CurrentTimeZone.ToUniversalTime(OrderDate))
            {
                return false;
            }

            if (other.OrderPoint.Level != OrderPoint.Level
                || other.OrderPoint.Quantity != OrderPoint.Quantity)
            {
                return false;
            }

            if (Items != null && other.Items != null)
            {
                if (Items.Count != other.Items.Count)
                    return false;

                for (int i = 0; i < Items.Count; i++)
                {
                    if (!other.Items[i].Equals(Items[i]))
                        return false;
                }
            }
            else if (Items != null || other.Items != null)
            {
                return false;
            }

            return true;
        }
    }

    public class SaleOrder : IEquatable<SaleOrder>
    {
        public static SaleOrder CreateExample()
        {
            var order = new SaleOrder
            {
                Amount = 1024.567M,
                Mobile = "1234567890",
                Name = "Someone",
                OrderId = 1234567890123456,
                OrderDate = new DateTime(2012, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc),
                OrderType = SaleOrderType.WaitNotifying,
                TypeFlag = 'D',
                Rate = 0.56F,
                Remark = "\"Hello\r\nWorld!\"",
                OrderPoint = new SaleOrderPoint { Level = 2, Quantity = 335 },
                Items = new List<SaleOrderItem>
                {
                    new SaleOrderItem {ProductNo = "ProductNo1", Quantity = 3, Price = 23.3M},
                    new SaleOrderItem {ProductNo = "ProductNo2", Quantity = 1, Price = 26M},
                    new SaleOrderItem {ProductNo = "ProductNo3", Quantity = 2, Price = 788.45M},
                },
                Flags = new[] { 1, 3, 4, 11 }
            };
            return order;
        }

        public long OrderId { get; set; }
        public string Name { get; set; }
        public DateTime OrderDate { get; set; }
        public SaleOrderType OrderType { get; set; }
        public char TypeFlag { get; set; }
        public string Mobile { get; set; }
        public string Remark { get; set; }
        public string Attributes { get; set; }
        public int? ClassLevel { get; set; }
        public decimal Amount { get; set; }
        public float Rate { get; set; }
        public SaleOrderPoint OrderPoint { get; set; }
        public List<SaleOrderItem> Items { get; set; }
        public IEnumerable<int> Flags { get; set; }

        public bool Equals(SaleOrder other)
        {
            if (other == null)
                return false;

            if (other.OrderId != OrderId
                || other.Name != Name
                || other.OrderType != OrderType
                || other.TypeFlag != TypeFlag
                || other.Mobile != Mobile
                || other.Remark != Remark
                || other.Attributes != Attributes
                || other.ClassLevel != ClassLevel
                || other.Amount != Amount
                || Math.Abs(other.Rate - Rate) > float.Epsilon)
            {
                return false;
            }

            if (TimeZone.CurrentTimeZone.ToUniversalTime(other.OrderDate)
                != TimeZone.CurrentTimeZone.ToUniversalTime(OrderDate))
            {
                return false;
            }

            if (other.OrderPoint.Level != OrderPoint.Level
                || other.OrderPoint.Quantity != OrderPoint.Quantity)
            {
                return false;
            }

            if (Items != null && other.Items != null)
            {
                if (Items.Count != other.Items.Count)
                    return false;

                for (int i = 0; i < Items.Count; i++)
                {
                    if (!other.Items[i].Equals(Items[i]))
                        return false;
                }
            }
            else if (Items != null || other.Items != null)
            {
                return false;
            }

            if (Flags != null && other.Flags != null)
            {
                using (var itor = Flags.GetEnumerator())
                using (var otherItor = other.Flags.GetEnumerator())
                {
                    while (true)
                    {
                        bool moveNext;
                        if ((moveNext = itor.MoveNext()) != otherItor.MoveNext())
                            return false;

                        if (!moveNext)
                            break;

                        if (itor.Current != otherItor.Current)
                            return false;
                    }
                }
            }
            else if (Items != null || other.Items != null)
            {
                return false;
            }

            return true;
        }
    }

    public enum SaleOrderType
    {
        Normal = 0,
        Emergency,
        WaitNotifying
    }

    public class SaleOrderItem : IEquatable<SaleOrderItem>
    {
        public string ProductNo { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public bool Equals(SaleOrderItem other)
        {
            if (other == null)
                return false;

            if (other.ProductNo != ProductNo
                || other.Quantity != Quantity
                || other.Price != Price)
            {
                return false;
            }

            return true;
        }
    }

    public struct SaleOrderPoint
    {
        public int Level { get; set; }
        public int Quantity { get; set; }
    }
}
