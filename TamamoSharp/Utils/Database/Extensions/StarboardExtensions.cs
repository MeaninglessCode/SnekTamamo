using Npgsql;
using System.Data.Common;
using System.Threading.Tasks;
using TamamoSharp.Database.Constants;
using TamamoSharp.Database.Extensions;

namespace TamamoSharp.Database
{
    public static class StarboardExtensions
    {
        public static async Task AddStarboardEntry(this TamamoDb db, StarboardEntry entry)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = $"INSERT INTO starboard_entries (message_id, bot_message_id, channel_id, " +
                    $"starboard_channel_id, author_id, star_count, guild_id) VALUES (@1, @2, @3, @4, " +
                    $"@5, @6, @7);";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(entry.MessageId - long.MaxValue));
                    query.Parameters.AddWithValue("@2", (long)(entry.BotMessageId - long.MaxValue));
                    query.Parameters.AddWithValue("@3", (long)(entry.ChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@4", (long)(entry.StarboardChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@5", (long)(entry.AuthorId - long.MaxValue));
                    query.Parameters.AddWithValue("@6", entry.StarCount);
                    query.Parameters.AddWithValue("@7", (long)(entry.GuildId - long.MaxValue));

                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task DeleteStarboardEntry(this TamamoDb db, StarboardEntry entry)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "DELETE FROM starboard_entries WHERE " +
                    $"{StarboardEntryFieldNames.Id}=@1";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);
                    query.Parameters.AddWithValue("@1", entry.Id);
                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task UpdateStarboardEntry(this TamamoDb db, StarboardEntry entry)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "UPDATE starboard_entries SET id=@1, message_id=@2, bot_message_id=@3, " +
                    "channel_id=@4, starboard_channel_id=@5, author_id=@6, star_count=@7, guild_id=@8, " +
                    "WHERE id=@9";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", entry.Id);
                    query.Parameters.AddWithValue("@2", (long)(entry.MessageId - long.MaxValue));
                    query.Parameters.AddWithValue("@3", (long)(entry.BotMessageId - long.MaxValue));
                    query.Parameters.AddWithValue("@4", (long)(entry.ChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@5", (long)(entry.StarboardChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@6", (long)(entry.AuthorId - long.MaxValue));
                    query.Parameters.AddWithValue("@7", entry.StarCount);
                    query.Parameters.AddWithValue("@8", (long)(entry.GuildId - long.MaxValue));
                    query.Parameters.AddWithValue("@9", entry.Id);

                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task<StarboardEntry> GetStarboardEntry(this TamamoDb db, ulong messageId)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "SELECT * FROM starboard_entries WHERE " +
                    $"{StarboardEntryFieldNames.MessageId}=@1";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(messageId - long.MaxValue));

                    StarboardEntry entry = null;
                    using (DbDataReader reader = await query.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                entry = new StarboardEntry
                                {
                                    Id = await reader.GetGuidAsync(StarboardEntryFieldNames.Id),
                                    MessageId = await reader.GetUInt64Async(StarboardEntryFieldNames.MessageId),
                                    BotMessageId = await reader.GetUInt64Async(StarboardEntryFieldNames.BotMessageId),
                                    ChannelId = await reader.GetUInt64Async(StarboardEntryFieldNames.ChannelId),
                                    StarboardChannelId = await reader.GetUInt64Async(StarboardEntryFieldNames.StarboardChannelId),
                                    AuthorId = await reader.GetUInt64Async(StarboardEntryFieldNames.AuthorId),
                                    StarCount = await reader.GetInt32Async(StarboardEntryFieldNames.StarCount),
                                    GuildId = await reader.GetUInt64Async(StarboardEntryFieldNames.GuildId)
                                };
                            }
                        }
                    }

                    await query.PrepareAsync();
                    await tran.CommitAsync();
                    return entry;
                }
            }
        }
    }
}
