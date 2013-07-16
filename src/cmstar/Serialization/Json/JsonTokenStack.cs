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
    internal class JsonTokenStack
    {
        private readonly List<JsonToken> _stack = new List<JsonToken>();
        private int _max = -1;
        private int _tail = -1;

        public int Count
        {
            get { return _tail + 1; }
        }

        public void Clear()
        {
            _tail = -1;
        }

        public void Push(JsonToken token)
        {
            var index = _tail + 1;
            if (index > _max)
            {
                _stack.Add(token);
                _max = index;
            }
            else
            {
                _stack[index] = token;
            }
            _tail = index;
        }

        public JsonToken Top
        {
            get { return _tail < 0 ? JsonToken.None : _stack[_tail]; }
        }

        public JsonToken Peek(int count)
        {
            var index = _tail - count + 1;
            return index < 0 ? JsonToken.None : _stack[index];
        }

        public JsonToken Pop()
        {
            return _tail < 0 ? JsonToken.None : _stack[_tail--];
        }
    }
}
