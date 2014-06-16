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
using System.Globalization;
using System.Text;

namespace cmstar.Serialization.Json
{
    /// <summary>
    /// Provides methods for converting between CLR types and JSON types.
    /// </summary>
    public static class JsonConvert
    {
        private static readonly long JavaScriptMinDateTicks
            = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        /// <summary>
        /// Converts an object to its JSON string representation.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The JSON string representation of the object.</returns>
        public static string ToString(object obj)
        {
            if (obj == null)
                return string.Empty;

            string s = obj as string;
            if (s != null)
                return s;

            if (obj is IConvertible)
                return Convert.ToString(obj, CultureInfo.InvariantCulture);

            return obj.ToString();
        }

        /// <summary>
        /// Convert a value of <see cref="DateTime"/> to a string representation 
        /// of a datetime in a JSON. 
        /// </summary>
        /// <param name="dateTime">
        /// The datetime value.
        /// If the kind of the datetime is not <see cref="DateTimeKind.Utc"/>,
        /// the result would contains a timezone offset.
        /// </param>
        /// <param name="wrappedInSlashes">
        /// Indicates whether to wrap the datetime value in a pair of slashes.
        /// If <c>true</c>, the datetime value will be in this format:'/Date(1xxxxxxxxxxxx+yyyy)/';
        /// otherwise, no prefix and surfix:'Date(1xxxxxxxxxxxx+yyyy)'.
        /// </param>
        /// <returns>A string represents a datetime value in a JSON.</returns>
        public static string ToJsonDateTimeValue(DateTime dateTime, bool wrappedInSlashes)
        {
            var stringBuilder = new StringBuilder(28); //capacity=len(/Date(1xxxxxxxxxxxx+yyyy)/)
            if (wrappedInSlashes)
            {
                stringBuilder.Append("/");
            }
            stringBuilder.Append("Date(");

            //write javascript ticks
            var jsTicks = ClrTicksToJavascriptTicks(dateTime.ToUniversalTime().Ticks);
            stringBuilder.Append(jsTicks);

            //write the timezone
            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
            if (offset.Ticks != 0)
            {
                stringBuilder.Append((offset.Ticks > 0) ? '+' : '-');

                var hours = Math.Abs(offset.Hours);
                if (hours < 10)
                {
                    stringBuilder.Append(0);
                }
                stringBuilder.Append(hours);

                var minutes = Math.Abs(offset.Minutes);
                if (minutes < 10)
                {
                    stringBuilder.Append(0);
                }
                stringBuilder.Append(minutes);
            }

            stringBuilder.Append(")");
            if (wrappedInSlashes)
            {
                stringBuilder.Append("/");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Convert a number of ticks in the CLR to a number of ticks in Javascript.
        /// </summary>
        /// <param name="clrTicks">The number of ticks in the CLR.</param>
        /// <returns>The number of ticks in Javascript.</returns>
        public static long ClrTicksToJavascriptTicks(long clrTicks)
        {
            var javascriptTicks = (clrTicks - JavaScriptMinDateTicks) / 10000;
            return javascriptTicks;
        }

        /// <summary>
        /// Convert a number of ticks in Javascript to a number of ticks in the CLR.
        /// </summary>
        /// <param name="javascriptTicks">The number of ticks in Javascript.</param>
        /// <returns>The number of ticks in the CLR.</returns>
        public static long JavascriptTicksToClrTicks(long javascriptTicks)
        {
            var clrTicks = (javascriptTicks * 10000) + JavaScriptMinDateTicks;
            return clrTicks;
        }

        /// <summary>
        /// Convert the string representation of a Javascript datetime value 
        /// to a <see cref="DateTime"/>.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="dateTime">
        /// The datetime result.
        /// The kind of datetime is <see cref="DateTimeKind.Local"/>.
        /// This is a output parameter.
        /// </param>
        /// <returns><c>true</c> if the convertion succeeded; otherwise <c>false</c>.</returns>
        public static bool TryParseJsonDateTimeValue(string value, out DateTime dateTime)
        {
            dateTime = DateTime.MinValue;
            if (value == null || value.Length < 7) //7=len(Date(0))
                return false;

            int start, end;
            if (!TryGetDateDataIndexes(value, out start, out end) || start >= end)
                return false;

            bool hasTimeZone = false;
            int timeZoneStart = start + 1;
            while (timeZoneStart <= end)
            {
                char c = value[timeZoneStart];
                if (c == '+' || c == '-')
                {
                    hasTimeZone = true;
                    break;
                }

                if (c < '0' || c > '9')
                    return false;

                timeZoneStart++;
            }

            long javascriptTicks;
            if (!long.TryParse(value.Substring(start, timeZoneStart - start), out javascriptTicks))
                return false;

            long clrTicks = JavascriptTicksToClrTicks(javascriptTicks);
            if (hasTimeZone)
            {
                long offset;
                if (!TryParseTimeZoneOffset(value, timeZoneStart, end, out offset))
                    return false;
            }

            dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(clrTicks, DateTimeKind.Utc));
            return true;
        }

        private static bool TryParseTimeZoneOffset(string value, int startIndex, int endIndex, out long ticks)
        {
            ticks = 0;
            if (endIndex - startIndex != 4)
                return false;

            int v;
            if (!int.TryParse(value.Substring(startIndex + 1, 4), out v))
                return false;

            ticks = TimeSpan.FromHours(v / 100).Ticks;
            ticks += TimeSpan.FromMinutes(v % 100).Ticks;
            if (value[startIndex] == '-')
            {
                ticks = -ticks;
            }

            return true;
        }

        //Ignores the prefix an suffix in the string:
        ///\Date(1xxxxxxxxxxxx+yyyy)\
        //      |start          |end
        //or:
        //Date(1xxxxxxxxxxxx+yyyy)
        //     |start           |end
        private static bool TryGetDateDataIndexes(string value, out int startIndex, out int endIndex)
        {
            startIndex = 0;
            endIndex = value.Length - 1;

            //parse the prefix
            bool hasSlash = false;
            if (value[startIndex] == '/')
            {
                hasSlash = true;
                startIndex++;
            }

            if (value[startIndex] == 'D'
                && value[++startIndex] == 'a'
                && value[++startIndex] == 't'
                && value[++startIndex] == 'e'
                && value[++startIndex] == '(')
            {
                startIndex++;
            }
            else
            {
                return false;
            }

            //parse the suffix
            if (hasSlash)
            {
                if (value[endIndex] != '/')
                    return false;

                endIndex--;
            }

            if (value[endIndex] == ')')
            {
                endIndex--;
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
