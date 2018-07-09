using System.Collections.Generic;

namespace TamamoSharp.Database
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public bool IsIgnored { get; set; } = false;
        public bool DeleteInvokingMessage { get; set; } = false;
        public bool DeleteCommandReplies { get; set; } = false;
        public bool DeleteErrorMessages { get; set; } = false;
        public bool LeaveMessageEnabled { get; set; } = false;
        public string LeaveMessage { get; set; } = "%user% has left the guild. :(";
        public ulong LeaveMessageChannelId { get; set; } = 0;
        public bool JoinMessageEnabled { get; set; } = false;
        public string JoinMessage { get; set; } = "Welcome, %user%!";
        public ulong JoinMessageChannelId { get; set; } = 0;
        public bool StarboardEnabled { get; set; } = false;
        public ulong StarboardChannelId { get; set; } = 0;
        public int StarboardMaxAge { get; set; } = 7;
        public int StarboardThreshold { get; set; } = 1;
        public bool StarboardLocked { get; set; } = false;
        public bool StarboardAutoClear { get; set; } = false;
        public int MaxTagCountPerUser { get; set; } = 0;
        public int MaxTagCount { get; set; } = 0;
        public bool AutoAssignRole { get; set; } = false;
        public ulong AutoAssignRoleId { get; set; } = 0;
        public ulong MuteRoleId { get; set; } = 0;
    }
}
