using System;
using System.Data;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The contract for <see cref="IDataRecord"/>.
    /// Serialize IDataRecords like POCOs.
    /// </summary>
    public class DataRecordContract : JsonContract
    {
        public DataRecordContract(Type type)
            : base(type)
        {
        }

        protected override void DoWrite(
            JsonWriter writer, JsonSerializingState state, IJsonContractResolver contractResolver, object obj)
        {
            if (obj == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteObjectStart();

            var record = (IDataRecord)obj;
            for (int i = 0; i < record.FieldCount; i++)
            {
                if (i > 0)
                {
                    writer.WriteComma();
                }

                writer.WritePropertyName(record.GetName(i));
                var val = record.GetValue(i);
                var contract = contractResolver.ResolveContract(val);
                contract.Write(writer, state, contractResolver, val);
            }

            writer.WriteObjectEnd();
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            throw new NotSupportedException("The deserialization for System.Data.DataRow is not supported.");
        }
    }
}
