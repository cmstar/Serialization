using System;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The contract for <see cref="DBNull"/>.
    /// The <see cref="DBNull.Value"/> is serialized to JSON null.
    /// </summary>
    public class DbNullContract : JsonContract
    {
        public DbNullContract()
            : base(typeof(DBNull))
        {
        }

        protected override void DoWrite(
            JsonWriter writer, JsonSerializingState state, IJsonContractResolver contractResolver, object obj)
        {
            if (obj != null && obj != DBNull.Value)
                throw JsonContractErrors.TypeNotSupported(obj.GetType());

            writer.WriteNullValue();
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            reader.Read();

            switch (reader.Token)
            {
                case JsonToken.NullValue:
                case JsonToken.UndefinedValue:
                    return DBNull.Value;

                default:
                    throw JsonContractErrors.UnexpectedToken(JsonToken.NullValue, reader.Token);
            }
        }
    }
}
