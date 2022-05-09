using System;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// A <see cref="DateTimeOffsetContract"/> that formats time in the Microsoft JSON format,
    /// such as '/Date(1xxxxxxxxxxxx+yyyy)/'.
    /// </summary>
    public class MicrosoftJsonDateContract : DateTimeOffsetContract
    {
        protected override string ToStringValue(DateTimeOffset value)
        {
            return JsonConvert.ToMicrosoftJsonDate(value, true);
        }
    }
}
