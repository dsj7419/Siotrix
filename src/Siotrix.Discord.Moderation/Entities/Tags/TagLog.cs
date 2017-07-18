using Discord.Commands;
using System;

namespace Siotrix
{
    public class TagLog : Entity
    {
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
        public ulong GuildId { get; private set; }
        public ulong ChannelId { get; private set; }
        public ulong UserId { get; private set; }
        public ulong TagId { get; private set; }

        // Foreign Keys
        public Tag Tag { get; private set; }

        public TagLog() { }
        public TagLog(ulong tagId, SocketCommandContext context)
        {
            TagId = tagId;
            GuildId = context.Guild.Id;
            ChannelId = context.Channel.Id;
            UserId = context.User.Id;
        }
    }
}