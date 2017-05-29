using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord
{

    public enum AccessLevel
    {
        Blocked,
        User,
        GuildMod,
        GuildAdmin,
        GuildOwner,
        BotOwner
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MinPermissionsAttribute : PreconditionAttribute
    {
        private AccessLevel Level;

        public MinPermissionsAttribute(AccessLevel level)
        {
            Level = level;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var access = GetPermission(context);            // Get the acccesslevel for this context

            if (access >= Level)                            // If the user's access level is greater than the required level, return success.
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("Insufficient permissions."));
        }

        public AccessLevel GetPermission(ICommandContext c)
        {
            var context = c as SocketCommandContext;

            if (context.User.IsBot)                                    // Prevent other bots from executing commands.
                return AccessLevel.Blocked;

            if (Configuration.Load().PConfigs.Owners.Contains(context.User.Id)) // Give configured owners special access.
                return AccessLevel.BotOwner;

            // Check if the context is in a guild.
            if (context.User is SocketGuildUser user)
            {
                if (context.Guild.OwnerId == user.Id)                  // Check if the user is the guild owner.
                    return AccessLevel.GuildOwner;

                if (user.GuildPermissions.Administrator)         // Check if the user has the administrator permission.
                    return AccessLevel.GuildAdmin;

                if (user.GuildPermissions.ManageMessages ||      // Check if the user can ban, kick, or manage messages.
                    user.GuildPermissions.BanMembers ||
                    user.GuildPermissions.KickMembers)
                    return AccessLevel.GuildMod;
            }

            return AccessLevel.User;                             // If nothing else, return a default permission.
        }
    }
}