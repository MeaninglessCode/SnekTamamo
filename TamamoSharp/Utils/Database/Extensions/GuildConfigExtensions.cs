using Npgsql;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using TamamoSharp.Database.Constants;
using TamamoSharp.Database.Extensions;

namespace TamamoSharp.Database
{
    public static class GuildConfigExtensions
    {
        public static async Task AddGuildConfigAsync(this TamamoDb db, GuildConfig config)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "INSERT INTO guild_configs (guild_id, is_ignored, delete_invoking_message, " +
                    "delete_command_replies, delete_error_messages, leave_message_enabled, leave_message, " +
                    "leave_message_channel_id, join_message_enabled, join_message, join_message_channel_id, " +
                    "starboard_enabled, starboard_channel_id, starboard_max_age, starboard_threshold, " +
                    "starboard_locked, starboard_auto_clear, max_tag_count_per_user, max_tag_count, auto_assign_role, " +
                    "auto_assign_role_id, mute_role_id) VALUES (@1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, " +
                    "@13, @14, @15, @16, @17, @18, @19, @20, @21, @22);";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(config.GuildId - long.MaxValue));
                    query.Parameters.AddWithValue("@2", config.IsIgnored);
                    query.Parameters.AddWithValue("@3", config.DeleteInvokingMessage);
                    query.Parameters.AddWithValue("@4", config.DeleteCommandReplies);
                    query.Parameters.AddWithValue("@5", config.DeleteErrorMessages);
                    query.Parameters.AddWithValue("@6", config.LeaveMessageEnabled);
                    query.Parameters.AddWithValue("@7", config.LeaveMessage);
                    query.Parameters.AddWithValue("@8", (long)(config.LeaveMessageChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@9", config.JoinMessageEnabled);
                    query.Parameters.AddWithValue("@10", config.JoinMessage);
                    query.Parameters.AddWithValue("@11", (long)(config.JoinMessageChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@12", config.StarboardEnabled);
                    query.Parameters.AddWithValue("@13", (long)(config.StarboardChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@14", config.StarboardMaxAge);
                    query.Parameters.AddWithValue("@15", config.StarboardThreshold);
                    query.Parameters.AddWithValue("@16", config.StarboardLocked);
                    query.Parameters.AddWithValue("@17", config.StarboardAutoClear);
                    query.Parameters.AddWithValue("@18", config.MaxTagCountPerUser);
                    query.Parameters.AddWithValue("@19", config.MaxTagCount);
                    query.Parameters.AddWithValue("@20", config.AutoAssignRole);
                    query.Parameters.AddWithValue("@21", (long)(config.AutoAssignRoleId - long.MaxValue));
                    query.Parameters.AddWithValue("@22", (long)(config.MuteRoleId - long.MaxValue));

                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task AddChannelConfigAsync(this TamamoDb db, ChannelConfig config)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "INSERT INTO channel_configs (channel_id, is_ignored, guild_id) " +
                    "VALUES (@1, @2, @3);";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(config.ChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@2", config.IsIgnored);
                    query.Parameters.AddWithValue("@3", (long)(config.GuildId - long.MaxValue));

                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task<GuildConfig> GetGuildConfigAsync(this TamamoDb db, ulong guildId)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = $"SELECT * FROM guild_configs WHERE " +
                    $"{GuildConfigFieldNames.GuildId}=@1;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(guildId - long.MaxValue));

                    GuildConfig config = null;
                    await query.PrepareAsync();
                    using (DbDataReader reader = await query.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                config = new GuildConfig
                                {
                                    GuildId = guildId,
                                    IsIgnored = await reader.GetBooleanAsync(GuildConfigFieldNames.IsIgnored),
                                    DeleteInvokingMessage = await reader.GetBooleanAsync(GuildConfigFieldNames.DeleteInvokingMessage),
                                    DeleteCommandReplies = await reader.GetBooleanAsync(GuildConfigFieldNames.DeleteCommandReplies),
                                    DeleteErrorMessages = await reader.GetBooleanAsync(GuildConfigFieldNames.DeleteErrorMessages),
                                    LeaveMessageEnabled = await reader.GetBooleanAsync(GuildConfigFieldNames.LeaveMessageEnabled),
                                    LeaveMessage = await reader.GetStringAsync(GuildConfigFieldNames.LeaveMessage),
                                    LeaveMessageChannelId = await reader.GetUInt64Async(GuildConfigFieldNames.LeaveMessageChannelId),
                                    JoinMessageEnabled = await reader.GetBooleanAsync(GuildConfigFieldNames.JoinMessageEnabled),
                                    JoinMessage = await reader.GetStringAsync(GuildConfigFieldNames.JoinMessage),
                                    JoinMessageChannelId = await reader.GetUInt64Async(GuildConfigFieldNames.JoinMessageChannelId),
                                    StarboardEnabled = await reader.GetBooleanAsync(GuildConfigFieldNames.StarboardEnabled),
                                    StarboardChannelId = await reader.GetUInt64Async(GuildConfigFieldNames.StarboardChannelId),
                                    StarboardMaxAge = await reader.GetInt32Async(GuildConfigFieldNames.StarboardMaxAge),
                                    StarboardThreshold = await reader.GetInt32Async(GuildConfigFieldNames.StarboardThreshold),
                                    StarboardLocked = await reader.GetBooleanAsync(GuildConfigFieldNames.StarboardLocked),
                                    StarboardAutoClear = await reader.GetBooleanAsync(GuildConfigFieldNames.StarboardAutoClear),
                                    MaxTagCountPerUser = await reader.GetInt32Async(GuildConfigFieldNames.MaxTagCountPerUser),
                                    MaxTagCount = await reader.GetInt32Async(GuildConfigFieldNames.MaxTagCount),
                                    AutoAssignRole = await reader.GetBooleanAsync(GuildConfigFieldNames.AutoAssignRole),
                                    AutoAssignRoleId = await reader.GetUInt64Async(GuildConfigFieldNames.AutoAssignRoleId),
                                    MuteRoleId = await reader.GetUInt64Async(GuildConfigFieldNames.MuteRoleId)
                                };
                            }
                        }
                    }
                    
                    await tran.CommitAsync();
                    return config;
                }
            }
        }

        public static async Task<GuildIgnoreData> GetGuildIgnoreDataAsync(this TamamoDb db, ulong guildId)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "SELECT is_ignored AS guild_is_ignored, guild_id, is_ignored " +
                    "AS channel_is_ignored, channel_id FROM guild_configs NATURAL JOIN " +
                    "channel_configs WHERE guild_id=@1;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(guildId - long.MaxValue));

                    GuildIgnoreData guildData = null;
                    await query.PrepareAsync();
                    using (DbDataReader reader = await query.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            guildData = new GuildIgnoreData
                            {
                                ChannelData = new Dictionary<ulong, bool>()
                            };

                            bool guildIdSet = false;
                            bool guildIgnoredSet = false;

                            while (await reader.ReadAsync())
                            {
                                if (!guildIdSet && !guildIgnoredSet)
                                {
                                    guildData.GuildId = await reader.GetUInt64Async("guild_id");
                                    guildData.IsGuildIgnored = await reader.GetBooleanAsync("guild_is_ignored");

                                    guildIdSet = true;
                                    guildIgnoredSet = true;
                                }

                                guildData.ChannelData.Add(
                                    await reader.GetUInt64Async("channel_id"),
                                    await reader.GetBooleanAsync("channel_is_ignored")
                                );
                            }
                        }
                    }

                    await tran.CommitAsync();
                    return guildData;
                }
            }
        }

        public static async Task<ChannelConfig> GetChannelConfigAsync(this TamamoDb db, ulong channelId)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "SELECT * FROM channel_configs WHERE channel_id=@1;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(channelId - long.MaxValue));

                    ChannelConfig config = null;
                    await query.PrepareAsync();
                    using (DbDataReader reader = await query.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                config = new ChannelConfig
                                {
                                    ChannelId = channelId,
                                    IsIgnored = await reader.GetBooleanAsync(ChannelConfigEntryNames.IsIgnored),
                                    GuildId = await reader.GetUInt64Async(ChannelConfigEntryNames.GuildId)
                                };
                            }
                        }
                    }

                    await tran.CommitAsync();
                    return config;
                }
            }
        }

        public static async Task UpdateGuildConfigAsync(this TamamoDb db, GuildConfig config)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "UPDATE guild_configs SET guild_id=@1, is_ignored=@2, delete_invoking_message=@3, " +
                    "delete_command_replies=@4, delete_error_messages=@5, leave_message_enabled=@6, leave_message=@7, " +
                    "leave_message_channel_id=@8, join_message_enabled=@9, join_message=@10, join_message_channel_id=@11, " +
                    "starboard_enabled=@12, starboard_channel_id=@13, starboard_max_age=@14, starboard_threshold=@15, " +
                    "starboard_locked=@16, starboard_auto_clear=@17, max_tag_count_per_user=@18, max_tag_count=@19, " +
                    "auto_assign_role=@20, auto_assign_role_id=@21, mute_role_id=@22 WHERE guild_id=@23;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(config.GuildId - long.MaxValue));
                    query.Parameters.AddWithValue("@2", config.IsIgnored);
                    query.Parameters.AddWithValue("@3", config.DeleteInvokingMessage);
                    query.Parameters.AddWithValue("@4", config.DeleteCommandReplies);
                    query.Parameters.AddWithValue("@5", config.DeleteErrorMessages);
                    query.Parameters.AddWithValue("@6", config.LeaveMessageEnabled);
                    query.Parameters.AddWithValue("@7", config.LeaveMessage);
                    query.Parameters.AddWithValue("@8", (long)(config.LeaveMessageChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@9", config.JoinMessageEnabled);
                    query.Parameters.AddWithValue("@10", config.JoinMessage);
                    query.Parameters.AddWithValue("@11", (long)(config.JoinMessageChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@12", config.StarboardEnabled);
                    query.Parameters.AddWithValue("@13", (long)(config.StarboardChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@14", config.StarboardMaxAge);
                    query.Parameters.AddWithValue("@15", config.StarboardThreshold);
                    query.Parameters.AddWithValue("@16", config.StarboardLocked);
                    query.Parameters.AddWithValue("@17", config.StarboardAutoClear);
                    query.Parameters.AddWithValue("@18", config.MaxTagCountPerUser);
                    query.Parameters.AddWithValue("@19", config.MaxTagCount);
                    query.Parameters.AddWithValue("@20", config.AutoAssignRole);
                    query.Parameters.AddWithValue("@21", (long)(config.AutoAssignRoleId - long.MaxValue));
                    query.Parameters.AddWithValue("@22", (long)(config.MuteRoleId - long.MaxValue));
                    query.Parameters.AddWithValue("@23", (long)(config.GuildId - long.MaxValue));

                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task UpdateChannelConfigAsync(this TamamoDb db, ChannelConfig config)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "UPDATE channel_configs SET channel_id=@1, is_ignored=@2, guild_id=@3 WHERE" +
                    "channel_id=@4;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(config.ChannelId - long.MaxValue));
                    query.Parameters.AddWithValue("@2", config.IsIgnored);
                    query.Parameters.AddWithValue("@3", (long)(config.GuildId - long.MaxValue));
                    query.Parameters.AddWithValue("@4", (long)(config.ChannelId - long.MaxValue));

                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task<bool> IsGuildIgnoredAsync(this TamamoDb db, ulong guildId)
        {
            GuildConfig config = await db.GetGuildConfigAsync(guildId);
            return (config == null) ? true : config.IsIgnored;
        }

        public static async Task<bool> IsChannelIgnoredAsync(this TamamoDb db, ulong channelId)
        {
            ChannelConfig config = await db.GetChannelConfigAsync(channelId);
            return (config == null) ? true : config.IsIgnored;
        }

        public static async Task IgnoreGuildAsync(this TamamoDb db, ulong guildId)
        {
            GuildConfig config = await db.GetGuildConfigAsync(guildId);
            config.IsIgnored = true;
            await db.UpdateGuildConfigAsync(config);
        }

        public static async Task IgnoreChannelAsync(this TamamoDb db, ulong channelId)
        {
            ChannelConfig config = await db.GetChannelConfigAsync(channelId);
            config.IsIgnored = true;
            await db.UpdateChannelConfigAsync(config);
        }

        public static async Task IgnoreChannelAsync(this TamamoDb db, ChannelConfig config)
        {
            config.IsIgnored = true;
            await db.UpdateChannelConfigAsync(config);
        }


        public static async Task UnIgnoreGuildAsync(this TamamoDb db, ulong guildId)
        {
            GuildConfig config = await db.GetGuildConfigAsync(guildId);
            config.IsIgnored = false;
            await db.UpdateGuildConfigAsync(config);
        }

        public static async Task UnIgnoreChannelAsync(this TamamoDb db, ulong channelId)
        {
            ChannelConfig config = await db.GetChannelConfigAsync(channelId);
            config.IsIgnored = false;
            await db.UpdateChannelConfigAsync(config);
        }

        public static async Task UnIgnoreChannelAsync(this TamamoDb db, ChannelConfig config)
        {
            config.IsIgnored = false;
            await db.UpdateChannelConfigAsync(config);
        }
    }
}
