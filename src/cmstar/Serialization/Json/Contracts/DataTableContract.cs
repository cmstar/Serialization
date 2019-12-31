using System;
using System.Data;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// The contract for <see cref="DataTable"/>.
    /// Treat the whole table as a collection of <see cref="DataRow"/>s,
    /// and each <see cref="DataRow"/> as a POCO.
    /// </summary>
    public class DataTableContract : JsonContract
    {
        public DataTableContract()
            : base(typeof(DataTable))
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

            writer.WriteArrayStart();

            var table = (DataTable)obj;
            var columns = table.Columns;
            var columnCount = columns.Count;
            var columnContracts = new JsonContract[columnCount];

            for (int i = 0; i < columnCount; i++)
            {
                var contract = contractResolver.ResolveContract(columns[i].DataType);
                columnContracts[i] = contract;
            }

            var rowCount = table.Rows.Count;
            for (int i = 0; i < rowCount; i++)
            {
                if (i > 0)
                {
                    writer.WriteComma();
                }

                writer.WriteObjectStart();

                var row = table.Rows[i];
                for (int j = 0; j < columnCount; j++)
                {
                    if (j > 0)
                    {
                        writer.WriteComma();
                    }

                    writer.WritePropertyName(columns[j].ColumnName);

                    var val = row[j];
                    if (val == null || val == DBNull.Value)
                    {
                        writer.WriteNullValue();
                    }
                    else
                    {
                        var contract = columnContracts[j];
                        contract.Write(writer, state, contractResolver, row[j]);
                    }
                }

                writer.WriteObjectEnd();
            }

            writer.WriteArrayEnd();
        }

        protected override object DoRead(JsonReader reader, JsonDeserializingState state)
        {
            throw new NotSupportedException("The deserialization for System.Data.DataTable is not supported.");
        }
    }
}
