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
using System.Collections.ObjectModel;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// A key-value map which takes the property names as the keys and instances of
    /// the <see cref="ContractMemberInfo"/> as the values.
    /// </summary>
    public class ContractMemberCollection : IndexedKeyedCollection<string, ContractMemberInfo>
    {
        protected override string GetKeyForItem(ContractMemberInfo item)
        {
            return item.JsonPropertyName;
        }
    }

    public abstract class IndexedKeyedCollection<TKey, TItem> : Collection<TItem>
    {
        protected readonly Dictionary<TKey, int> Dictionary = new Dictionary<TKey, int>();

        public bool TryGetValue(TKey key, out TItem member)
        {
            int index;
            return TryGetValueAndIndex(key, out member, out index);
        }

        public bool TryGetValueAndIndex(TKey key, out TItem member, out int index)
        {
            if (!Dictionary.TryGetValue(key, out index))
            {
                member = default(TItem);
                return false;
            }

            member = base[index];
            return true;
        }

        protected abstract TKey GetKeyForItem(TItem item);

        protected override void ClearItems()
        {
            base.ClearItems();
            Dictionary.Clear();
        }

        protected override void InsertItem(int index, TItem item)
        {
            var key = GetKeyForItem(item);
            Dictionary.Add(key, index);

            base.InsertItem(index, item);
            ResetIndexAfter(index + 1);
        }

        protected override void RemoveItem(int index)
        {
            var key = GetKeyForItem(base[index]);
            Dictionary.Remove(key);

            base.RemoveItem(index);
            ResetIndexAfter(index);
        }

        protected override void SetItem(int index, TItem item)
        {
            var key = GetKeyForItem(item);

            int existItemIndex;
            if (!Dictionary.TryGetValue(key, out existItemIndex) || index != existItemIndex)
            {
                Dictionary.Add(key, index);

                var oldKey = GetKeyForItem(base[index]);
                Dictionary.Remove(oldKey);
            }

            base.SetItem(index, item);
        }

        private void ResetIndexAfter(int startIndex)
        {
            for (int i = startIndex; i < Count; i++)
            {
                var key = GetKeyForItem(base[i]);
                Dictionary[key] = i;
            }
        }
    }
}
