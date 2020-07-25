import discord
import random as rng
import cogs.utils.checks as checks
from discord.ext import commands

class Fun(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.command()
    @commands.is_owner()
    async def say(self, ctx, *, text: str):
        await ctx.send(text)

    @commands.command()
    async def choice(self, ctx, *, text: str):
        await ctx.send(text)

    @commands.command()
    async def test_command(self, ctx):
        await ctx.send(await self.bot.db.get_rows('guild_configs', **{'guild_id': 403752523325767690}))

    @commands.command()
    @checks.has_permissions(manage_webhooks=True)
    async def mock(self, ctx, member: discord.Member, *, text: str):
        webhook = None

        for wh in await ctx.channel.webhooks():
            if wh.name == 'Tamamo Webhook':
                webhook = wh
                break

        if webhook is None:
            webhook = await ctx.channel.create_webhook(
                name='Tamamo Webhook',
                avatar=await self.bot.user.avatar_url_as(format='png', size=128).read(),
                reason='Webhook for Tamamo '
            )

        await webhook.send(text, username=member.display_name, avatar_url=member.avatar_url)

def setup(bot):
    bot.add_cog(Fun(bot))