using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace Siotrix.Discord
{
    public static class UserExtensions
    {
        public static EmbedBuilder FormatUserInfo(IGuild guildInfo, SocketGuild guild, SocketGuildUser user)
        {
            var guildUser = user;
            var roles = guildUser.Roles.OrderBy(x => x.Position).Where(x => !x.IsEveryone);
            var channels = new List<string>();
            guild.TextChannels.OrderBy(x => x.Position).ToList().ForEach(x =>
            {
                if (guildUser.GetPermissions(x).ReadMessages)
                    channels.Add(x.Name);
            });
            guild.VoiceChannels.OrderBy(x => x.Position).ToList().ForEach(x =>
            {
                if (guildUser.GetPermissions(x).Connect)
                    channels.Add(x.Name + " (Voice)");
            });
            var users = guild.Users.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();
            var created = guildUser.CreatedAt.UtcDateTime;
            var joined = guildUser.JoinedAt.Value.UtcDateTime;

            var IDstr = string.Format("**ID:** `{0}`", guildUser.Id);
            var nicknameStr = string.Format("**Nickname:** `{0}`",
                string.IsNullOrWhiteSpace(guildUser.Nickname)
                    ? "NO NICKNAME"
                    : MessageExtensions.EscapeMarkdown(guildUser.Nickname, true));
            var createdStr = string.Format("\n**Created:** `{0}`",
                DateTimeExtensions.FormatDateTime(guildUser.CreatedAt.UtcDateTime));
            var joinedStr = string.Format("**Joined:** `{0}` (`{1}` to join the guild)\n",
                DateTimeExtensions.FormatDateTime(guildUser.JoinedAt.Value.UtcDateTime), users.IndexOf(guildUser) + 1);
            var gameStr = FormatGameStr(guildUser);
            var statusStr = string.Format("**Online status:** `{0}`", guildUser.Status);
            var description = string.Join("\n", IDstr, nicknameStr, createdStr, joinedStr, gameStr, statusStr);

            var color = roles.OrderBy(x => x.Position).LastOrDefault(x => x.Color.RawValue != 0)?.Color;
            var embed = EmbedExtensions.MakeNewEmbed(null, description, color, thumbnailURL: user.GetAvatarUrl());
            if (channels.Count() != 0)
                embed.AddField("Channels", string.Join(", ", channels));
            if (roles.Count() != 0)
                embed.AddField("Roles", string.Join(", ", roles.Select(x => x.Name)));
            if (user.VoiceChannel != null)
            {
                var desc = string.Format(
                    "Server mute: `{0}`\nServer deafen: `{1}`\nSelf mute: `{2}`\nSelf deafen: `{3}`", user.IsMuted,
                    user.IsDeafened, user.IsSelfMuted, user.IsSelfDeafened);
                embed.AddField("Voice Channel: " + user.VoiceChannel.Name, desc);
            }
            embed.WithAuthor(guildUser.FormatUser(), guildUser.GetAvatarUrl(), guildUser.GetAvatarUrl());
            embed.WithFooter("User Info");
            return embed;
        }

        public static EmbedBuilder FormatUserInfo(IGuild guildInfo, SocketGuild guild, SocketUser user)
        {
            var ageStr = string.Format("**Created:** `{0}`\n",
                DateTimeExtensions.FormatDateTime(user.CreatedAt.UtcDateTime));
            var gameStr = FormatGameStr(user);
            var statusStr = string.Format("**Online status:** `{0}`", user.Status);
            var description = string.Join("\n", ageStr, gameStr, statusStr);

            var embed = EmbedExtensions.MakeNewEmbed(null, description, null, thumbnailURL: user.GetAvatarUrl());
            embed.WithAuthor(user.FormatUser(), user.GetAvatarUrl(), user.GetAvatarUrl());
            embed.WithFooter("User Info");
            return embed;
        }

        public static string FormatUser(this IUser user, ulong? userID = 0)
        {
            if (user != null)
                return string.Format("'{0}#{1}' ({2})",
                    MessageExtensions.EscapeMarkdown(user.Username, true)
                        .CaseInsReplace("discord.gg", SiotrixConstants.FAKE_DISCORD_LINK),
                    user.Discriminator,
                    user.Id);
            return string.Format("Irretrievable User ({0})", userID);
        }

        public static string FormatGameStr(IUser user)
        {
            if (user.Game.HasValue)
            {
                var game = user.Game.Value;
                if (game.StreamType == StreamType.Twitch)
                    return string.Format("**Current Stream:** [{0}]({1})",
                        MessageExtensions.EscapeMarkdown(game.Name, true), game.StreamUrl);
                return string.Format("**Current Game:** `{0}`", MessageExtensions.EscapeMarkdown(game.Name, true));
            }
            return "**Current Game:** `N/A`";
        }
    }
}