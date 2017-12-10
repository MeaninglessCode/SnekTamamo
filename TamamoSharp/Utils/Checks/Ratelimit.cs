using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TamamoSharp
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class Ratelimit : PreconditionAttribute
    {
        private readonly Dictionary<ulong, CommandTimeout> _userInvokes = new Dictionary<ulong, CommandTimeout>();
        private readonly bool _dmLimited;
        private readonly bool _adminLimited;
        private readonly uint _lim;
        private readonly TimeSpan _invokePeriod;

        public Ratelimit(uint lim, double period, TimeMeasure measure, bool dmLimited = false,
            bool adminLimited = false)
        {
            _lim = lim;
            _dmLimited = dmLimited;
            _adminLimited = adminLimited;

            switch (measure)
            {
                case TimeMeasure.Minutes:
                    _invokePeriod = TimeSpan.FromMinutes(period);
                    break;
                case TimeMeasure.Hours:
                    _invokePeriod = TimeSpan.FromHours(period);
                    break;
                case TimeMeasure.Days:
                    _invokePeriod = TimeSpan.FromDays(period);
                    break;
            }
        }

        public Ratelimit(uint lim, TimeSpan period, bool dmLimited = true, bool adminLimited = true)
        {
            _lim = lim;
            _dmLimited = dmLimited;
            _adminLimited = adminLimited;
            _invokePeriod = period;
        }


        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext ctx, CommandInfo cmd,
            IServiceProvider svc)
        {
            if ((ctx.Channel is ISocketPrivateChannel) && !_dmLimited)
                return Task.FromResult(PreconditionResult.FromSuccess());
            if ((ctx.User is IGuildUser u) && u.GuildPermissions.Administrator && !_adminLimited)
                return Task.FromResult(PreconditionResult.FromSuccess());


            CommandTimeout timeout = (_userInvokes.TryGetValue(ctx.User.Id, out var t) &&
                (DateTime.Now - t.FirstInvoke) < _invokePeriod)
                ? t
                : new CommandTimeout(DateTime.Now);
            timeout.InvokeCount++;

            if (timeout.InvokeCount <= _lim)
            {
                _userInvokes[ctx.User.Id] = timeout;
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            return Task.FromResult(PreconditionResult.FromError(""));

            throw new NotImplementedException();
        }

        private class CommandTimeout
        {
            public uint InvokeCount { get; set; }
            public DateTime FirstInvoke { get; }
            public CommandTimeout(DateTime start) { FirstInvoke = start; }
        }

        public enum TimeMeasure { Days, Hours, Minutes }
    }
}
