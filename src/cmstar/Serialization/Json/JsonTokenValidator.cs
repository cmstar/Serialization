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

using System.Collections.Generic;

namespace cmstar.Serialization.Json
{
    internal static class JsonTokenValidator
    {
        private static readonly HashSet<int> AcceptableTokenStates;

        static JsonTokenValidator()
        {
            AcceptableTokenStates = new HashSet<int>();
            AddAcceptableTokenStates();
        }

        public static bool Validate(JsonToken current, JsonToken last, JsonToken container)
        {
            var code = GetTokenStateCode(current, last, container);
            return AcceptableTokenStates.Contains(code);
        }

        private static void AddAcceptableTokenStates()
        {
            AddAcceptableTokenState(JsonToken.ObjectStart, JsonToken.ArrayStart, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ObjectStart, JsonToken.Comma, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ObjectStart, JsonToken.None, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ObjectStart, JsonToken.PropertyName, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ObjectStart, JsonToken.None, JsonToken.None);
            AddAcceptableTokenState(JsonToken.ObjectStart, JsonToken.PropertyName, JsonToken.None);

            AddAcceptableTokenState(JsonToken.ObjectEnd, JsonToken.ObjectStart, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ObjectEnd, JsonToken.ObjectEnd, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ObjectEnd, JsonToken.ArrayEnd, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ObjectEnd, JsonToken.BooleanValue, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ObjectEnd, JsonToken.NullValue, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ObjectEnd, JsonToken.NumberValue, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ObjectEnd, JsonToken.StringValue, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ObjectEnd, JsonToken.UndefinedValue, JsonToken.ObjectStart);

            AddAcceptableTokenState(JsonToken.ArrayStart, JsonToken.ArrayStart, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ArrayStart, JsonToken.Comma, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ArrayStart, JsonToken.None, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ArrayStart, JsonToken.PropertyName, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.ArrayStart, JsonToken.None, JsonToken.None);
            AddAcceptableTokenState(JsonToken.ArrayStart, JsonToken.PropertyName, JsonToken.None);

            AddAcceptableTokenState(JsonToken.ArrayEnd, JsonToken.ArrayStart, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ArrayEnd, JsonToken.ArrayEnd, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ArrayEnd, JsonToken.ObjectEnd, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ArrayEnd, JsonToken.BooleanValue, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ArrayEnd, JsonToken.NullValue, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ArrayEnd, JsonToken.NumberValue, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ArrayEnd, JsonToken.StringValue, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.ArrayEnd, JsonToken.UndefinedValue, JsonToken.ArrayStart);

            AddAcceptableTokenState(JsonToken.PropertyName, JsonToken.ObjectStart, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.PropertyName, JsonToken.Comma, JsonToken.ObjectStart);

            AddAcceptableTokenState(JsonToken.Comma, JsonToken.ObjectEnd, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.ArrayEnd, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.BooleanValue, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.NullValue, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.NumberValue, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.StringValue, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.UndefinedValue, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.ObjectEnd, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.ArrayEnd, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.BooleanValue, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.NullValue, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.NumberValue, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.StringValue, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.Comma, JsonToken.UndefinedValue, JsonToken.ArrayStart);

            AddAcceptableTokenState(JsonToken.BooleanValue, JsonToken.None, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.BooleanValue, JsonToken.None, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.BooleanValue, JsonToken.None, JsonToken.None);
            AddAcceptableTokenState(JsonToken.BooleanValue, JsonToken.PropertyName, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.BooleanValue, JsonToken.Comma, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.BooleanValue, JsonToken.ArrayStart, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.BooleanValue, JsonToken.Comma, JsonToken.ArrayStart);

            AddAcceptableTokenState(JsonToken.NullValue, JsonToken.None, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.NullValue, JsonToken.None, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.NullValue, JsonToken.None, JsonToken.None);
            AddAcceptableTokenState(JsonToken.NullValue, JsonToken.PropertyName, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.NullValue, JsonToken.Comma, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.NullValue, JsonToken.ArrayStart, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.NullValue, JsonToken.Comma, JsonToken.ArrayStart);

            AddAcceptableTokenState(JsonToken.NumberValue, JsonToken.None, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.NumberValue, JsonToken.None, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.NumberValue, JsonToken.None, JsonToken.None);
            AddAcceptableTokenState(JsonToken.NumberValue, JsonToken.PropertyName, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.NumberValue, JsonToken.Comma, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.NumberValue, JsonToken.ArrayStart, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.NumberValue, JsonToken.Comma, JsonToken.ArrayStart);

            AddAcceptableTokenState(JsonToken.StringValue, JsonToken.None, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.StringValue, JsonToken.None, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.StringValue, JsonToken.None, JsonToken.None);
            AddAcceptableTokenState(JsonToken.StringValue, JsonToken.PropertyName, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.StringValue, JsonToken.Comma, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.StringValue, JsonToken.ArrayStart, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.StringValue, JsonToken.Comma, JsonToken.ArrayStart);

            AddAcceptableTokenState(JsonToken.UndefinedValue, JsonToken.None, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.UndefinedValue, JsonToken.None, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.UndefinedValue, JsonToken.None, JsonToken.None);
            AddAcceptableTokenState(JsonToken.UndefinedValue, JsonToken.PropertyName, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.UndefinedValue, JsonToken.Comma, JsonToken.ObjectStart);
            AddAcceptableTokenState(JsonToken.UndefinedValue, JsonToken.ArrayStart, JsonToken.ArrayStart);
            AddAcceptableTokenState(JsonToken.UndefinedValue, JsonToken.Comma, JsonToken.ArrayStart);
        }

        private static void AddAcceptableTokenState(JsonToken current, JsonToken last, JsonToken container)
        {
            var code = GetTokenStateCode(current, last, container);
            AcceptableTokenStates.Add(code);
        }

        private static int GetTokenStateCode(JsonToken current, JsonToken last, JsonToken container)
        {
            var code = ((int)current << 16) | ((int)last << 8) | ((int)container);
            return code;
        }
    }
}
