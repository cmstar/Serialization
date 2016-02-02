using System;
using System.Data;
using System.Data.SqlTypes;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// Adapts from <see cref="DataRow"/> to <see cref="IDataRecord"/>.
    /// </summary>
    internal class DataRowRecord : IDataRecord
    {
        private readonly DataTable _table;
        private readonly DataRow _currentRow;

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="dataRow">作为数据源的<see cref="DataRow"/>。</param>
        public DataRowRecord(DataRow dataRow)
        {
            ArgAssert.NotNull(dataRow, "dataRow");

            _currentRow = dataRow;
            _table = dataRow.Table;
        }

        public int FieldCount
        {
            get { return _table.Columns.Count; }
        }

        public string GetDataTypeName(int i)
        {
            return GetFieldType(i).Name;
        }

        public Type GetFieldType(int i)
        {
            return _table.Columns[i].DataType;
        }

        public object GetValue(int i)
        {
            return _currentRow[i];
        }

        public string GetName(int i)
        {
            return _table.Columns[i].ColumnName;
        }

        public int GetOrdinal(string name)
        {
            return _table.Columns[name].Ordinal;
        }

        public int GetValues(object[] values)
        {
            ArgAssert.NotNull(values, "values");

            var num = Math.Min(values.Length, FieldCount);
            for (int i = 0; i <= num; i++)
            {
                values[i] = GetValue(i);
            }

            return num;
        }

        public object this[string name]
        {
            get
            {
                var index = GetOrdinal(name);
                return GetValue(index);
            }
        }

        public object this[int i]
        {
            get { return GetValue(i); }
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new InvalidOperationException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new InvalidOperationException();
        }

        public IDataReader GetData(int i)
        {
            throw new InvalidOperationException();
        }

        public bool GetBoolean(int i)
        {
            return Convert.ToBoolean(GetValue(i));
        }

        public byte GetByte(int i)
        {
            return Convert.ToByte(GetValue(i));
        }

        public char GetChar(int i)
        {
            return Convert.ToChar(GetValue(i));
        }

        public Guid GetGuid(int i)
        {
            var value = GetValue(i);
            if (value == null)
                throw new InvalidCastException();

            if (value is SqlGuid)
                return ((SqlGuid)value).Value;

            if (value is Guid)
                return (Guid)value;

            return new Guid(value.ToString());
        }

        public short GetInt16(int i)
        {
            return Convert.ToInt16(GetValue(i));
        }

        public int GetInt32(int i)
        {
            return Convert.ToInt32(GetValue(i));
        }

        public long GetInt64(int i)
        {
            return Convert.ToInt64(GetValue(i));
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(GetValue(i));
        }

        public double GetDouble(int i)
        {
            return Convert.ToDouble(GetValue(i));
        }

        public string GetString(int i)
        {
            return GetValue(i).ToString();
        }

        public decimal GetDecimal(int i)
        {
            return Convert.ToDecimal(GetValue(i));
        }

        public DateTime GetDateTime(int i)
        {
            return Convert.ToDateTime(GetValue(i));
        }

        public bool IsDBNull(int i)
        {
            return GetValue(i) is DBNull;
        }
    }
}