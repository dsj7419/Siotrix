using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Siotrix.Discord
{
    public static class RoleExtensions
    {
        public static EmbedBuilder FormatRoleInfo(IGuild guildInfo, SocketGuild guild, SocketRole role)
        {
            var ageStr = string.Format("**Created:** `{0}` (`{1}` days ago)",
                DateTimeExtensions.FormatDateTime(role.CreatedAt.UtcDateTime),
                DateTime.UtcNow.Subtract(role.CreatedAt.UtcDateTime).Days);
            var positionStr = string.Format("**Position:** `{0}`", role.Position);
            var usersStr = string.Format("**User Count:** `{0}`",
                guild.Users.Where(x => x.Roles.Any(y => y.Id == role.Id)).Count());
            var description = string.Join("\n", ageStr, positionStr, usersStr);

            var color = role.Color;
            var embed = EmbedExtensions.MakeNewEmbed(null, description, color);
            embed.WithAuthor(role.FormatRole());
            embed.WithFooter("Role Info");
            return embed;
        }

        public static string FormatRole(this IRole role)
        {
            if (role != null)
                return string.Format("'{0}' ({1})", MessageExtensions.EscapeMarkdown(role.Name, true), role.Id);
            return "Irretrievable Role";
        }

        public static IGuildUser GetBot(IGuild guild)
        {
            return (guild as SocketGuild).CurrentUser;
        }

        public static int GetUserPosition(IUser user)
        {
            //Make sure they're a SocketGuildUser
            var tempUser = user as SocketGuildUser;
            if (user == null)
                return -1;

            return tempUser.Hierarchy;
        }

        public static async Task<int> ModifyRolePosition(IRole role, int position)
        {
            if (role == null)
                return -1;

            var roles = role.Guild.Roles.Where(x => x.Id != role.Id && x.Position < GetUserPosition(GetBot(role.Guild)))
                .OrderBy(x => x.Position).ToArray();
            position = Math.Max(1, Math.Min(position, roles.Length));

            var reorderProperties = new ReorderRoleProperties[roles.Length + 1];
            for (var i = 0; i < reorderProperties.Length; i++)
                if (i == position)
                    reorderProperties[i] = new ReorderRoleProperties(role.Id, i);
                else if (i > position)
                    reorderProperties[i] = new ReorderRoleProperties(roles[i - 1].Id, i);
                else if (i < position)
                    reorderProperties[i] = new ReorderRoleProperties(roles[i].Id, i);

            await role.Guild.ReorderRolesAsync(reorderProperties);
            return reorderProperties.FirstOrDefault(x => x.Id == role.Id)?.Position ?? -1;
        }

        public static async Task GiveRole(IGuildUser user, IRole role)
        {
            if (role == null)
                return;
            if (user.RoleIds.Contains(role.Id))
                return;
            await user.AddRoleAsync(role).ConfigureAwait(false);
        }

        public static async Task GiveRoles(IGuildUser user, IEnumerable<IRole> roles)
        {
            if (!roles.Any())
                return;

            await user.AddRolesAsync(roles);
        }

        public static async Task TakeRole(IGuildUser user, IRole role)
        {
            if (role == null)
                return;
            if (!user.RoleIds.Contains(role.Id))
                return;
            await user.RemoveRoleAsync(role);
        }

        public static async Task TakeRoles(IGuildUser user, IEnumerable<IRole> roles)
        {
            if (!roles.Any())
                return;

            await user.RemoveRolesAsync(roles);
        }
    }
}