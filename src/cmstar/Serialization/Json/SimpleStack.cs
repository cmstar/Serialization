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

namespace cmstar.Serialization.Json
{
    internal class SimpleStack<T>
    {
        private const int DefaultCapacity = 8;
        private T[] _stack;
        private int _len = -1;
        private int _tail = -1;
        private T _noneValue;

        public SimpleStack()
            : this(DefaultCapacity, default(T))
        {
        }

        public SimpleStack(int capacity)
            : this(capacity, default(T))
        {
        }

        public SimpleStack(T noneValue)
            : this(DefaultCapacity, noneValue)
        {
        }

        public SimpleStack(int capacity, T noneValue)
        {
            _stack = new T[capacity];
            _len = _stack.Length;
            _noneValue = noneValue;
        }

        public int Count
        {
            get { return _tail + 1; }
        }

        public void Clear()
        {
            _tail = -1;
        }

        public void Push(T value)
        {
            var index = _tail + 1;

            //enlarge the array (size * 2) when the old one is full
            if (index >= _len)
            {
                var newStack = new T[_len * 2];
                Array.Copy(_stack, newStack, _len);

                _stack = newStack;
                _len = newStack.Length;
            }

            _stack[index] = value;
            _tail = index;
        }

        public T Top
        {
            get { return _tail < 0 ? _noneValue : _stack[_tail]; }
        }

        public T Peek(int count)
        {
            var index = _tail - count + 1;
            return index < 0 ? _noneValue : _stack[index];
        }

        public T Pop()
        {
            return _tail < 0 ? _noneValue : _stack[_tail--];
        }

        public bool Contains(T value)
        {
            foreach (T e in _stack)
            {
                if (Equals(e, value))
                    return true;
            }

            return false;
        }
    }
}