using System;
using Discord.WebSocket;

namespace Siotrix.Discord
{
    public static class EntityHelper
    {
        public static DiscordMessage CreateMessage(SocketMessage message)
        {
            var guild = (message.Channel as SocketGuildChannel)?.Guild;
            var user = message.Author as SocketGuildUser;

            return new DiscordMessage()
            {
                AuthorId = (long)message.Author.Id,
                MessageId = (long)message.Id,
                Content = message.Content,
                ChannelId = (long)message.Channel.Id,
                GuildId = (long?)guild?.Id,
                Name = user?.Nickname ?? user.Username
            };
        }

        public static DiscordMembership CreateMembership(SocketGuildUser user, bool isjoin)
        {
            return new DiscordMembership()
            {
                GuildId = (long)user.Guild.Id,
                UserId = (long)user.Id,
                IsJoining = isjoin
            };
        }

        public static DiscordReaction CreateReaction(SocketReaction reaction)
        {
            return new DiscordReaction()
            {
                AuthorId = (long)reaction.UserId,
                EmojiId = (long?)reaction.Emoji.Id,
                EmojiName = reaction.Emoji.Name,
                MessageId = (long)reaction.MessageId
            };
        }
        
        public static DiscordStatus CreateStatus(SocketPresence after, SocketUser user, SocketGuild guild)
        {
            return new DiscordStatus()
            {
                GuildId = (long?)guild?.Id,
                Status = (DiscordStatusType)after.Status
            };
        }
    }
}
