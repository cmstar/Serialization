using System;
using System.Data;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The contract for <see cref="DataRow"/>.
    /// Serialize DataRows like POCOs.
    /// </summary>
    public class DataRowContract : JsonContract
    {
        public DataRowContract()
            : base(typeof(DataRow))
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

            var dataRow = (DataRow)obj;
            var cols = dataRow.Table.Columns;
            for (int i = 0; i < cols.Count; i++)
            {
                if (i > 0)
                {
                    writer.WriteComma();
                }

                writer.WritePropertyName(cols[i].ColumnName);

                var val = dataRow[i];
                if (val == null || val == DBNull.Value)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    var contract = contractResolver.ResolveContract(val);
                    contract.Write(writer, state, contractResolver, val);
                }
            }

            writer.WriteObjectEnd();
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            throw new NotSupportedException("The deserialization for System.Data.DataRow is not supported.");
        }
    }
}
