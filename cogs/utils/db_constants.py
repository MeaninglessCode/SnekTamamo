from cogs.utils.db import Column, Table

GUILD_CONFIG_COLS = [
    Column(is_index=True, is_pkey=True, is_nullable=False, is_unique=True, name='guild_id'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, default=False, name='is_ignored'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, name='starboard_enabled'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, default=0, name='starboard_channel_id'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, default=7, name='starboard_max_age'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, default=3, name='starboard_threshold'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, default=False, name='join_message_enabled'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, default=0, name='join_message_channel_id'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, default='Welcome, {}!', name='join_message'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, name='leave_message_enabled'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, default=0, name='leave_message_channel_id'),
    Column(is_index=False, is_pkey=True, is_nullable=False, is_unique=True, default='{} has left the server. Farewell!', name='leave_message')
]