using System;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// A <see cref="DateTimeContract"/> that formats time in the Microsoft JSON format,
    /// such as '/Date(1xxxxxxxxxxxx+yyyy)/'.
    /// </summary>
    public class MicrosoftJsonDateContract : DateTimeContract
    {
        protected override string ToStringValue(DateTime dateTime)
        {
            return JsonConvert.ToMicrosoftJsonDate(dateTime, true);
        }
    }
}
