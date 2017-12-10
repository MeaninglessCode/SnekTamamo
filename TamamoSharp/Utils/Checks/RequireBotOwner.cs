using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TamamoSharp
{
    public class RequireBotOwner : PreconditionAttribute
    {
        private static ulong ownerId = (ulong)(JObject.Parse(File.ReadAllText("config.json"))["owner_id"]);

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider svc)
        {
            if (context.User.Id == ownerId)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("User is not bot owner."));
        }
    }
}
