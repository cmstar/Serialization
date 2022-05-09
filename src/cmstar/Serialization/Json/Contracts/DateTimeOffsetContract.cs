using System;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The default contract for <see cref="DateTimeOffset"/>.
    /// By default it formats a value in the ISO-8601 format.
    /// Override the <see cref="ToStringValue"/> method to customize the format.
    /// </summary>
    public class DateTimeOffsetContract : JsonContract
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DateTimeOffsetContract"/>.
        /// </summary>
        public DateTimeOffsetContract()
            : base(typeof(DateTimeOffset))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DateTimeOffsetContract"/>, and change the underlying type.
        /// </summary>
        public DateTimeOffsetContract(Type type)
            : base(type)
        {
        }

        protected override void DoWrite(
            JsonWriter writer,
            JsonSerializingState state,
            IJsonContractResolver contractResolver,
            object obj)
        {
            if (obj == null)
                throw JsonContractErrors.NullValueNotSupported();

            var dateTimeValue = ToStringValue((DateTimeOffset)obj);
            writer.WriteStringValue(dateTimeValue);
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            reader.Read();

            if (reader.Token == JsonToken.NullValue
                && state.NullValueHandling == JsonDeserializationNullValueHandling.AsDefaultValue)
            {
                return new DateTimeOffset();
            }

            if (reader.Token != JsonToken.StringValue)
                throw JsonContractErrors.UnexpectedToken(JsonToken.StringValue, reader.Token);

            if (!TryParseDateTime((string)reader.Value, out var d))
            {
                var msg = string.Format("Cannot convert the value \"{0}\" to a DateTimeOffset.", reader.Value);
                throw new JsonContractException(msg);
            }
            return d;
        }

        /// <summary>
        /// Converts the specified string to <see cref="DateTimeOffset"/> and returns a value 
        /// that indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">The string represents a <see cref="DateTime"/>.</param>
        /// <param name="dateTimeOffset">The result.</param>
        /// <returns>
        /// true if the string was converted successfully; otherwise false.
        /// </returns>
        protected virtual bool TryParseDateTime(string value, out DateTimeOffset dateTimeOffset)
        {
            if (DateTime.TryParse(value, out var d))
            {
                dateTimeOffset = d;
                return true;
            }

            return JsonConvert.TryParseMicrosoftJsonDate(value, out dateTimeOffset);
        }

        /// <summary>
        /// Converts the specified <see cref="DateTimeOffset"/> to it's corresponding string representation.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/>.</param>
        /// <returns>The string value represents the <see cref="DateTimeOffset"/>.</returns>
        protected virtual string ToStringValue(DateTimeOffset value)
        {
            // For a UTC time whose offset is zero, the 'O' format of DateTimeOffset will output '+0000',
            // not 'Z', which does not fit ISO-8601 well.
            return value.Offset.Ticks == 0
                ? value.UtcDateTime.ToString("O")
                : value.ToString("O");
        }
    }
}
