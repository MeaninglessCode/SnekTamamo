from discord.ext import commands

async def check_permissions(ctx, perms, *, check=all):
    if await ctx.bot.is_owner(ctx.author):
        return True

    resolved = ctx.channel.permissions_for(ctx.author)
    return check(getattr(resolved, name, None) == value for name, value in perms.items())

def has_permissions(*, check=all, **perms):
    async def pred(ctx):
        return await check_permissions(ctx, perms, check=check)
    return commands.check(pred)