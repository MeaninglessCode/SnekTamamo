GUILD_CONFIG_COLUMNS = [
    'guild_id', 'is_ignored', 'starboard_enabled', 'starboard_channel_id', 'starboard_max_age',
    'starboard_threshold', 'join_message_enabled', 'join_message_channel_id', 'join_message',
    'leave_message_enabled', 'leave_message_channel_id', 'leave_message'
]

table_name = 'guild_configs'

guild_config_insert = {
    'guild_id': 834831413,
    'is_ignored': False,
    'starboard_enabled': True,
    'starboard_channel_id': 481523
}

sql = 'SELECT * FROM {0} WHERE {1};'.format(
    table_name,
    ' AND '.join(f'{_} = ${i}' for i, _ in enumerate(guild_config_insert, 1))
)

values = list(guild_config_insert.values())

print(f'values: {values}')

print(f'sql: {sql}')