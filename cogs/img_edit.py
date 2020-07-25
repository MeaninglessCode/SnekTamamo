from discord.ext import commands
import random as rng
import cv2
import numpy as np

class ImgEdit(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.group(pass_context=True)
    async def img(self, ctx):
        """Commands for applying various effects and edits to images."""
        if ctx.invoked_subcommand is None:
            await ctx.send('No subcommand invoked')

    @img.command()
    async def remove_background(self, ctx):

        print('test')

    @img.command()
    async def resize(self, ctx):
        if len(ctx.message.attachments) < 1:
            await ctx.send('No image specified. Please attach an image to use this command.')
        else:
            # resize thing

            await ctx.send(ctx.message.attachments)

def setup(bot):
    bot.add_cog(ImgEdit(bot))