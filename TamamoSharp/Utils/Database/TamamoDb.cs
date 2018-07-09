using Npgsql;
using System.Threading.Tasks;

namespace TamamoSharp.Database
{
    public class TamamoDb
    {
        private readonly string _connectionString;

        public TamamoDb(string connectionString)
        {
            _connectionString = connectionString;
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task EnsureDbCreated()
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();

                string sql = @"
CREATE TABLE IF NOT EXISTS guild_configs (
    ""guild_id"" BIGINT NOT NULL PRIMARY KEY,
    ""is_ignored"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""delete_invoking_message"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""delete_command_replies"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""delete_error_messages"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""leave_message_enabled"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""leave_message"" TEXT NOT NULL DEFAULT('%user% has left the guild. :('),
    ""leave_message_channel_id"" BIGINT NOT NULL DEFAULT(0),
    ""join_message_enabled"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""join_message"" TEXT NOT NULL DEFAULT('Welcome, %user%!'),
    ""join_message_channel_id"" BIGINT NOT NULL DEFAULT(0),
    ""starboard_enabled"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""starboard_channel_id"" BIGINT NOT NULL DEFAULT(0),
    ""starboard_max_age"" INT NOT NULL DEFAULT(7),
    ""starboard_threshold"" INT NOT NULL DEFAULT(1),
    ""starboard_locked"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""starboard_auto_clear"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""max_tag_count_per_user"" INT NOT NULL DEFAULT(10),
    ""max_tag_count"" INT NOT NULL DEFAULT(0),
    ""auto_assign_role"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""auto_assign_role_id"" BIGINT NOT NULL DEFAULT(0),
    ""mute_role_id"" BIGINT NOT NULL DEFAULT(0)
);

CREATE TABLE IF NOT EXISTS channel_configs (
    ""channel_id"" BIGINT NOT NULL PRIMARY KEY,
    ""is_ignored"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""guild_id"" BIGINT NOT NULL REFERENCES guild_configs(guild_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS user_configs (
    ""id"" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    ""user_id"" BIGINT NOT NULL,
    ""is_ignored"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""created_tag_count"" INT NOT NULL DEFAULT(0),
    ""guild_id"" BIGINT NOT NULL REFERENCES guild_configs(guild_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS starboard_entries (
    ""id"" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    ""message_id"" BIGINT NOT NULL,
    ""bot_message_id"" BIGINT NOT NULL,
    ""channel_id"" BIGINT NOT NULL,
    ""starboard_channel_id"" BIGINT NOT NULL,
    ""author_id"" BIGINT NOT NULL,
    ""star_count"" INT NOT NULL,
    ""guild_id"" BIGINT NOT NULL REFERENCES guild_configs(guild_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS tags (
    ""id"" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    ""name"" TEXT NOT NULL,
    ""content"" TEXT NOT NULL,
    ""owner_id"" BIGINT NOT NULL,
    ""guild_id"" BIGINT NOT NULL REFERENCES guild_configs(guild_id) ON DELETE CASCADE,
    ""type"" TEXT NOT NULL DEFAULT('guild'),
    ""created_at"" TIMESTAMP NOT NULL DEFAULT now(),
    ""updated_at"" TIMESTAMP NOT NULL DEFAULT now(),
    ""uses"" INT NOT NULL DEFAULT(0),
    CONSTRAINT chk_type CHECK (type IN ('guild', 'stored'))
);

CREATE TABLE IF NOT EXISTS tag_aliases (
    ""id"" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    ""name"" TEXT NOT NULL,
    ""owner_id"" BIGINT NOT NULL,
    ""tag_id"" UUID NOT NULL REFERENCES tags(id) ON DELETE CASCADE
);";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);
                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }
    }
}
