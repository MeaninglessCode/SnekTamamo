import discord
import datetime, re
import json, asyncio
import copy
import logging
import traceback
import aiohttp
import sys
import asyncpg
import config

from discord.ext import commands
from cogs.utils.context import TamamoContext

cogs = (
    'cogs.admin',
    'cogs.fun',
    'cogs.mod'
)

def prefixes(bot, msg):
    user_id = bot.user.id
    base = [f"<@!{user_id}> ", f"<@{user_id}> "]
    return base

class SnekTamamo(commands.AutoShardedBot):
    def __init__(self, db):
        super().__init__(
            command_prefix=prefixes,
            description="big tama"
        )

        self.client_id = config.client_id
        self.db = db

        for cog in cogs:
            try:
                self.load_extension(cog)
            except Exception as e:
                print(f'Failed to load {e}.')

    async def on_command_error(self, ctx, error):
        if isinstance(error, commands.NoPrivateMessage):
            await ctx.author.send('This command cannot be used in private messages.')
        elif isinstance(error, commands.DisabledCommand):
            await ctx.author.send('Sorry. This command is disabled and cannot be used.')
        elif isinstance(error, commands.CommandInvokeError):
            original = error.original
            if not isinstance(original, discord.HTTPException):
                print(f'In {ctx.command.qualified_name}:', file=sys.stderr)
                traceback.print_tb(original.__traceback__)
                print(f'{original.__class__.__name__}: {original}', file=sys.stderr)
        elif isinstance(error, commands.ArgumentParsingError):
            await ctx.send(error)

    async def process_commands(self, msg):
        ctx = await self.get_context(msg, cls=TamamoContext)

        if ctx.command is None:
            return
        elif ctx.author.id == "0":
            return

        await self.invoke(ctx)

    async def on_message(self, msg):
        if msg.author.bot:
            return
        await self.process_commands(msg)

    async def on_ready(self):
        if not hasattr(self, 'uptime'):
            self.uptime = datetime.datetime.now()
        print(f'Ready: {self.user} ({self.user.id})')

        for guild in self.guilds:
            guild_data = {'guild_id': guild.id}

            await self.db.insert(table='guild_configs', **guild_data)

    @property
    def config(self):
        return __import__('config')

    def run(self):
        super().run(config.bot_token, reconnect=True)