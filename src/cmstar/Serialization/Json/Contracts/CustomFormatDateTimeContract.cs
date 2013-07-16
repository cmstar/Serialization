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
    /// An extention of <see cref="DateTimeContract"/> that allows to specify
    /// the format for serializing the date.
    /// </summary>
    public class CustomFormatDateTimeContract : DateTimeContract
    {
        private string _format;

        /// <summary>
        /// Gets or sets a value which is used to format the date and time.
        /// The format string will be passed to the <see cref="DateTime.ToString()"/>
        /// method during the serializing.
        /// </summary>
        public string Format
        {
            get
            {
                return _format;
            }
            set
            {
                ArgAssert.NotNull(value, "Format");
                _format = value;
            }
        }

        protected override void DoWrite(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            var datetime = (DateTime)obj;
            writer.WriteStringValue(datetime.ToString(_format));
        }
    }
}
