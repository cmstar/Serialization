using System;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// A <see cref="DateTimeContract"/> that formats time in the Javascript Date function expression,
    /// like '/Date(1xxxxxxxxxxxx+yyyy)/'.
    /// </summary>
    public class JavascriptDateTimeContract : DateTimeContract
    {
        protected override string ToStringValue(DateTime dateTime)
        {
            return JsonConvert.ToJavascriptDate(dateTime, true);
        }
    }
}
