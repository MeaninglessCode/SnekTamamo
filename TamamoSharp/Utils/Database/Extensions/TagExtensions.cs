using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using TamamoSharp.Database.Extensions;

namespace TamamoSharp.Database
{
    public static class TagExtensions
    {
        public static async Task AddTagDataAsync(this TamamoDb db, TagData data)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "INSERT INTO tags (name, content, owner_id, guild_id, type, " +
                    "created_at, updated_at, uses) VALUES (@1, @2, @3, @4, @5, @6, @7, @8);";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", data.Name);
                    query.Parameters.AddWithValue("@2", data.Content);
                    query.Parameters.AddWithValue("@3", (long)(data.OwnerId - long.MaxValue));
                    query.Parameters.AddWithValue("@4", (long)(data.GuildId - long.MaxValue));
                    query.Parameters.AddWithValue("@5", data.Type);
                    query.Parameters.AddWithValue("@6", data.CreatedAt);
                    query.Parameters.AddWithValue("@7", data.UpdatedAt);
                    query.Parameters.AddWithValue("@8", data.Uses);

                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task AddTagAliasAsync(this TamamoDb db, TagAlias alias)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "INSERT INTO tag_aliases (name, owner_id, tag_id) VALUES " +
                    "(@1, @2, @3);";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", alias.Name);
                    query.Parameters.AddWithValue("@2", (long)(alias.OwnerId - long.MaxValue));
                    query.Parameters.AddWithValue("@3", alias.TagId);

                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task DeleteTagDataAsync(this TamamoDb db, TagData tagData)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "DELETE FROM tags WHERE id=@1;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);
                    query.Parameters.AddWithValue("@1", tagData.Id);
                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task DeleteTagAliasAsync(this TamamoDb db, TagAlias alias)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "DELETE FROM tag_aliases WHERE id=@1;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);
                    query.Parameters.AddWithValue("@1", alias.Id);
                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task DeleteTagAsync(this TamamoDb db, Tag tag)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "DELETE FROM tags WHERE id=@1;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);
                    query.Parameters.AddWithValue("@1", tag.Data.Id);
                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task<TagData> GetTagDataAsync(this TamamoDb db, ulong guildId, string tagName,
            bool searchByAlias = false)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "SELECT * FROM tags WHERE guild_id=@1 AND name=@2 LIMIT 1;";
                if (searchByAlias)
                    sql = "SELECT * FROM tags WHERE guild_id=@1 AND (name=@2 OR id=(SELECT tag_id " +
                        "FROM tag_aliases WHERE tag_aliases.name=@3)) LIMIT 1;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    if (searchByAlias)
                    {
                        query.Parameters.AddWithValue("@1", (long)(guildId - long.MaxValue));
                        query.Parameters.AddWithValue("@2", tagName);
                        query.Parameters.AddWithValue("@3", tagName);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@1", (long)(guildId - long.MaxValue));
                        query.Parameters.AddWithValue("@2", tagName);
                    }

                    TagData data = null;
                    await query.PrepareAsync();
                    using (DbDataReader reader = await query.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                data = new TagData
                                {
                                    Id = await reader.GetGuidAsync("id"),
                                    Name = await reader.GetStringAsync("name"),
                                    Content = await reader.GetStringAsync("content"),
                                    OwnerId = await reader.GetUInt64Async("owner_id"),
                                    GuildId = await reader.GetUInt64Async("guild_id"),
                                    Type = await reader.GetStringAsync("type"),
                                    CreatedAt = await reader.GetDateTimeAsync("created_at"),
                                    UpdatedAt = await reader.GetDateTimeAsync("updated_at"),
                                    Uses = await reader.GetInt32Async("uses")
                                };
                            }
                        }
                    }

                    await tran.CommitAsync();
                    return data;
                }
            }
        }

        // TODO: Fix implementation SQL
        public static async Task<TagAlias> GetTagAliasAsync(this TamamoDb db, ulong guildId, string aliasName)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "SELECT * FROM tag_aliases WHERE " +
                    "(SELECT guild_id FROM tags)=@1 AND name=@2 LIMIT 1;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(guildId - long.MaxValue));
                    query.Parameters.AddWithValue("@2", aliasName);

                    TagAlias alias = null;
                    await query.PrepareAsync();
                    using (DbDataReader reader = await query.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                alias = new TagAlias
                                {
                                    Id = await reader.GetGuidAsync("id"),
                                    Name = await reader.GetStringAsync("name"),
                                    OwnerId = await reader.GetUInt64Async("owner_id"),
                                    TagId = await reader.GetGuidAsync("tag_id")
                                };
                            }
                        }
                    }

                    await tran.CommitAsync();
                    return alias;
                }
            }
        }

        public static async Task<Tag> GetTagAsync(this TamamoDb db, ulong guildId, string tagName,
            bool searchByAlias = false)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "SELECT t.*, a.id AS alias_id, a.name AS alias_name, a.owner_id AS " +
                    "alias_owner_id, a.tag_id FROM tags t LEFT JOIN tag_aliases a ON " +
                    "(t.id=a.tag_id) WHERE (t.guild_id=@1 AND t.name=@2) LIMIT 1;";
                if (searchByAlias)
                    sql = "SELECT t.*, a.id AS alias_id, a.name AS alias_name, a.owner_id AS " +
                        "alias_owner_id, a.tag_id FROM tags t LEFT JOIN tag_aliases a ON " +
                        "(t.id=a.tag_id) WHERE (t.guild_id=@1 AND (t.name=@2 OR t.id=" +
                        "(SELECT tag_id FROM tag_aliases WHERE tag_aliases.name=@3))) LIMIT 1;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    if (searchByAlias)
                    {
                        query.Parameters.AddWithValue("@1", (long)(guildId - long.MaxValue));
                        query.Parameters.AddWithValue("@2", tagName);
                        query.Parameters.AddWithValue("@3", tagName);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@1", (long)(guildId - long.MaxValue));
                        query.Parameters.AddWithValue("@2", tagName);
                    }

                    Tag tag = null;
                    await query.PrepareAsync();
                    using (DbDataReader reader = await query.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            bool tagDataIsSet = false;
                            while (await reader.ReadAsync())
                            {
                                if (!tagDataIsSet)
                                {
                                    tag = new Tag
                                    {
                                        Data = new TagData
                                        {
                                            Id = await reader.GetGuidAsync("id"),
                                            Name = await reader.GetStringAsync("name"),
                                            Content = await reader.GetStringAsync("content"),
                                            OwnerId = await reader.GetUInt64Async("owner_id"),
                                            GuildId = await reader.GetUInt64Async("guild_id"),
                                            Type = await reader.GetStringAsync("type"),
                                            CreatedAt = await reader.GetDateTimeAsync("created_at"),
                                            UpdatedAt = await reader.GetDateTimeAsync("updated_at"),
                                            Uses = await reader.GetInt32Async("uses")
                                        },
                                        Aliases = new List<TagAlias>()
                                    };
                                    tagDataIsSet = true;
                                }
                                if (!(await reader.IsDBNullAsync("alias_id")))
                                {
                                    tag.Aliases.Add(new TagAlias
                                    {
                                        Id = await reader.GetGuidAsync("alias_id"),
                                        Name = await reader.GetStringAsync("alias_name"),
                                        OwnerId = await reader.GetUInt64Async("alias_owner_id"),
                                        TagId = await reader.GetGuidAsync("id")
                                    });
                                }
                            }
                        }
                    }

                    await tran.CommitAsync();
                    return tag;
                }
            }
        }

        public static async Task<List<Tag>> GetAllTagsAsync(this TamamoDb db, ulong guildId)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "SELECT t.*, a.id AS alias_id, a.name AS alias_name, a.owner_id AS " +
                    "alias_owner_id, a.tag_id FROM tags t LEFT JOIN tag_aliases a ON (t.id=a.tag_id) " +
                    "WHERE (t.guild_id=@1) ORDER BY t.name ASC;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(guildId - long.MaxValue));

                    List<Tag> tags = null;
                    await query.PrepareAsync();
                    using (DbDataReader reader = await query.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            tags = new List<Tag>();
                            Guid currentTagId = Guid.Empty;
                            int currentTag = -1;

                            while (await reader.ReadAsync())
                            {
                                Guid tagId = await reader.GetGuidAsync("id");
                                if (currentTagId != tagId)
                                {
                                    currentTagId = tagId;
                                    currentTag++;

                                    tags.Add(new Tag
                                    {
                                        Data = new TagData
                                        {
                                            Id = await reader.GetGuidAsync("id"),
                                            Name = await reader.GetStringAsync("name"),
                                            Content = await reader.GetStringAsync("content"),
                                            OwnerId = await reader.GetUInt64Async("owner_id"),
                                            GuildId = await reader.GetUInt64Async("guild_id"),
                                            Type = await reader.GetStringAsync("type"),
                                            CreatedAt = await reader.GetDateTimeAsync("created_at"),
                                            UpdatedAt = await reader.GetDateTimeAsync("updated_at"),
                                            Uses = await reader.GetInt32Async("uses")
                                        },
                                        Aliases = new List<TagAlias>()
                                    });
                                }
                                if (!(await reader.IsDBNullAsync("alias_id")))
                                {
                                    tags[currentTag].Aliases.Add(new TagAlias
                                    {
                                        Id = await reader.GetGuidAsync("alias_id"),
                                        Name = await reader.GetStringAsync("alias_name"),
                                        OwnerId = await reader.GetUInt64Async("alias_owner_id"),
                                        TagId = await reader.GetGuidAsync("id")
                                    });
                                }
                            }
                        }
                    }

                    await tran.CommitAsync();
                    return tags;
                }
            }
        }

        public static async Task UpdateTagDataAsync(this TamamoDb db, TagData data)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "UPDATE tags SET id=@1, name=@2, content=@3, owner_id=@4, guild_id=@5, type=@6, " +
                    "created_at=@7, updated_at=@8, uses=@9 WHERE id=@9";
                
                // TODO: Figure out proper format here
                //data.UpdatedAt = DateTime.UtcNow;

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", data.Id);
                    query.Parameters.AddWithValue("@2", data.Name);
                    query.Parameters.AddWithValue("@3", data.Content);
                    query.Parameters.AddWithValue("@4", (long)(data.OwnerId - long.MaxValue));
                    query.Parameters.AddWithValue("@5", (long)(data.GuildId - long.MaxValue));
                    query.Parameters.AddWithValue("@6", data.Type);
                    query.Parameters.AddWithValue("@7", data.CreatedAt);
                    query.Parameters.AddWithValue("@8", data.UpdatedAt);
                    query.Parameters.AddWithValue("@9", data.Id);

                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task UpdateTagAliasAsync(TamamoDb db, TagAlias alias)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "UPDATE tag_aliases SET id=@1, name=@2, owner_id=@3, " +
                    "tag_id=@4 WHERE id=@5";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", alias.Id);
                    query.Parameters.AddWithValue("@2", alias.Name);
                    query.Parameters.AddWithValue("@3", (long)(alias.OwnerId - long.MaxValue));
                    query.Parameters.AddWithValue("@4", alias.TagId);
                    query.Parameters.AddWithValue("@5", alias.Id);

                    await query.PrepareAsync();
                    await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                }
            }
        }

        public static async Task AddTagUseAsync(this TamamoDb db, TagData tag)
        {
            tag.Uses += 1;
            await db.UpdateTagDataAsync(tag);
        }

        public static async Task<bool> TagOrAliasNameExists(this TamamoDb db, ulong guildId, string tagName)
        {
            using (NpgsqlConnection conn = db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "SELECT id FROM tags WHERE guild_id=@1 AND (name=@2 OR @3 " +
                    "IN (SELECT name FROM tag_aliases)) LIMIT 1;";

                using (NpgsqlTransaction tran = conn.BeginTransaction())
                {
                    NpgsqlCommand query = new NpgsqlCommand(sql, conn);

                    query.Parameters.AddWithValue("@1", (long)(guildId - long.MaxValue));
                    query.Parameters.AddWithValue("@2", tagName);
                    query.Parameters.AddWithValue("@3", tagName);

                    await query.PrepareAsync();
                    Object result = await query.ExecuteScalarAsync();
                    await tran.CommitAsync();
                    return !(result == null);
                }
            }
        }
    }
}
