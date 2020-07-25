from discord.ext import commands
import asyncio
import discord
import io

class TamamoContext(commands.Context):
    def __init__(self, **kwargs):
        super().__init__(**kwargs)