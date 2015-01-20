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

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The contract for numbers.
    /// </summary>
    public class NumberContract : JsonContract
    {
        private readonly TypeCode _typeCode;

        public NumberContract(Type type)
            : base(type)
        {
            //a enum has the typecode Int32 but is not supported in this contract
            if (typeof(Enum).IsAssignableFrom(type))
                throw new ArgumentException(
                    string.Format("The enumeration type {0} is not supported in this contract.", type), "type");

            _typeCode = Type.GetTypeCode(type);

            switch (_typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.DateTime:
                case TypeCode.String:
                    throw new ArgumentException(
                        string.Format("The type {0} is not supported in this contract.", type), "type");
            }
        }

        protected override void DoWrite(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            if (obj == null)
                throw JsonContractErrors.NullValueNotSupported();

            var typeCode = Type.GetTypeCode(obj.GetType());
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    writer.WriteNumberValue((bool)obj ? 1 : 0);
                    break;

                case TypeCode.Byte:
                    writer.WriteNumberValue((byte)obj);
                    break;

                case TypeCode.SByte:
                case TypeCode.Int16:
                    writer.WriteNumberValue((short)obj);
                    break;

                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.Char:
                    writer.WriteNumberValue((int)obj);
                    break;

                case TypeCode.UInt32:
                    writer.WriteNumberValue((uint)obj);
                    break;

                case TypeCode.Int64:
                    writer.WriteNumberValue((long)obj);
                    break;

                case TypeCode.UInt64:
                    writer.WriteNumberValue((ulong)obj);
                    break;

                case TypeCode.Single:
                    writer.WriteNumberValue((float)obj);
                    break;

                case TypeCode.Double:
                    writer.WriteNumberValue((double)obj);
                    break;

                case TypeCode.Decimal:
                    writer.WriteNumberValue((decimal)obj);
                    break;

                default:
                    throw JsonContractErrors.TypeNotSupported(obj.GetType());
            }
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            reader.Read();

            double value;
            switch (reader.Token)
            {
                case JsonToken.NumberValue:
                    value = (double)reader.Value;
                    break;

                case JsonToken.StringValue:
                    var s = (string)reader.Value;
                    if (!double.TryParse(s, out value))
                        throw JsonContractErrors.CannotConverType(s, typeof(double), null);

                    break;

                default:
                    throw JsonContractErrors.UnexpectedToken(JsonToken.NumberValue, reader.Token);
            }

            return _typeCode == TypeCode.Double ? value : Convert.ChangeType(value, UnderlyingType);
        }
    }
}
