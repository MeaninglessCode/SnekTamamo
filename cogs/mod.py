import discord
import cogs.utils.checks as checks
from discord.ext import commands
from collections import Counter

class Mod(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.group()
    @checks.has_permissions(manage_messages=True)
    async def purge(self, ctx):
        if ctx.invoked_subcommand is None:
            await ctx.send_help(ctx.command)
    
    async def do_purge(self, ctx, limit, check, *, before=None, after=None):
        if limit > 2000:
            return await ctx.send(f'test')

        before = ctx.message if before is None else discord.Object(id=before)

        try:
            deleted = await ctx.channel.purge(limit=limit, before=before, after=after, check=check)
        except discord.HTTPException as e:
            return await ctx.send(f'Error: {e} (undefined behavior)')

        authors = Counter(msg.author.display_name for msg in deleted)
        deleted = len(deleted)
        desc = [f'{deleted} message{" was" if deleted == 1 else "s were"} removed.']

        if deleted:
            desc.append('')
            authors = sorted(authors.items(), key=lambda t: t[1], reverse=True)
            desc.extend(f'**{name}**: {count}' for name, count in authors)

        desc = '\n'.join(desc)

        if len(desc) > 2000:
            await ctx.send(f'Successfully purged {deleted} message{"" if deleted == 1 else "s"}', delete_after=5)
        else:
            await ctx.send(embed=discord.Embed(title='Prune Result', description=desc, color=0x273847), delete_after=5)
    
    @purge.command(name='all')
    async def _all(self, ctx, search=100):
        await self.do_purge(ctx, search, lambda e: True)

    @purge.command(name='user')
    async def _user(self, ctx, member: discord.Member, search=100):
        await self.do_purge(ctx, search, lambda e: e.author == member)

def setup(bot):
    bot.add_cog(Mod(bot))