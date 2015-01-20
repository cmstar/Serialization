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
    public class GuidContract : JsonContract
    {
        public GuidContract()
            : base(typeof(Guid))
        {
        }

        protected override void DoWrite(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            //use the default format '00000000-0000-0000-0000-000000000000' for GUIDs
            writer.WriteRawStringValue(((Guid)obj).ToString());
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            reader.Read();

            if (reader.Token != JsonToken.StringValue)
                throw JsonContractErrors.UnexpectedToken(JsonToken.StringValue, reader.Token);

            Guid result;
            if (!TryParseGuid((string)reader.Value, out result))
                throw JsonContractErrors.CannotConverType((string)reader.Value, typeof(Guid), null);

            return result;
        }

        private bool TryParseGuid(string s, out Guid result)
        {
#if NET35
            // there isn't a public Guid.TryParse before .net4, so...
            try
            {
                result = new Guid(s);
                return true;
            }
            catch
            {
                result = Guid.Empty;
                return false;
            }
#else
            return Guid.TryParse(s, out result);
#endif
        }
    }
}
