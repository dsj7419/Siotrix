using Discord;
using Discord.WebSocket;
using System.Text;

namespace Siotrix.Discord
{
    public static class EntityHelper
    {
        public static DiscordMessage CreateMessage(SocketMessage message)
        {
            var guild = (message.Channel as SocketGuildChannel)?.Guild;
            var user = message.Author as SocketGuildUser;

            var formattedContent = string.IsNullOrWhiteSpace(message.Content)
                ? null
                : Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(message.Content));

            return new DiscordMessage()
            {
                AuthorId = (long)message.Author.Id,
                MessageId = (long)message.Id,
                Content = formattedContent,
                ChannelId = (long)message.Channel.Id,
                GuildId = (long?)guild?.Id,
                IsBot = message.Author.IsBot,
                GuildName = guild?.Name,
                ChannelName = message.Channel.Name,
                Name = Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(user?.Nickname ?? user.Username))
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
          //      EmojiId =  (long?)reaction.Emote.Id,
                EmojiName = reaction.Emote.Name,
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
