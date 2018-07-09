using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace TamamoSharp.Database.Extensions
{
    public static class DbDataReaderExtensions
    {
        public static async Task<string> GetStringAsync(this DbDataReader reader, string columnName)
            => await reader.GetFieldValueAsync<string>(reader.GetOrdinal(columnName));

        public static async Task<bool> GetBooleanAsync(this DbDataReader reader, string columnName)
            => await reader.GetFieldValueAsync<bool>(reader.GetOrdinal(columnName));

        public static async Task<Int32> GetInt32Async(this DbDataReader reader, string columnName)
            => await reader.GetFieldValueAsync<Int32>(reader.GetOrdinal(columnName));

        public static async Task<ulong> GetUInt64Async(this DbDataReader reader, string columnName)
            => (((ulong)await reader.GetFieldValueAsync<long>(reader.GetOrdinal(columnName))) + long.MaxValue);

        public static async Task<Guid> GetGuidAsync(this DbDataReader reader, string columnName)
            => await reader.GetFieldValueAsync<Guid>(reader.GetOrdinal(columnName));

        public static async Task<DateTime> GetDateTimeAsync(this DbDataReader reader, string columnName)
            => await reader.GetFieldValueAsync<DateTime>(reader.GetOrdinal(columnName));

        public static async Task<bool> IsDBNullAsync(this DbDataReader reader, string columnName)
            => await reader.IsDBNullAsync(reader.GetOrdinal(columnName));
    }
}
