using Discord.Commands;
using Discord.WebSocket;
using GynBot.Common.Enums;
using GynBot.Common.Types;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GynBot.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MinPermissionsAttribute : PreconditionAttribute
    {
        private AccessLevelEnum Level;

        public MinPermissionsAttribute(AccessLevelEnum level)
        {
            Level = level;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var access = GetPermission(context);            // Get the acccesslevel for this context

            if (access >= Level)                            // If the user's access level is greater than the required level, return success.
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("Insufficient permissions."));
        }

        public AccessLevelEnum GetPermission(ICommandContext c)
        {
            if (c.User.IsBot)                                    // Prevent other bots from executing commands.
                return AccessLevelEnum.Blocked;

            if (Configuration.Load().Owners.Contains(c.User.Id)) // Give configured owners special access.
                return AccessLevelEnum.BotOwner;

            // Check if the context is in a guild.
            if (c.User is SocketGuildUser user)
            {
                if (c.Guild.OwnerId == user.Id)                  // Check if the user is the guild owner.
                    return AccessLevelEnum.ServerOwner;

                if (user.GuildPermissions.Administrator)         // Check if the user has the administrator permission.
                    return AccessLevelEnum.ServerAdmin;

                if (user.GuildPermissions.ManageMessages ||      // Check if the user can ban, kick, or manage messages.
                    user.GuildPermissions.BanMembers ||
                    user.GuildPermissions.KickMembers)
                    return AccessLevelEnum.ServerMod;
            }

            return AccessLevelEnum.User;                             // If nothing else, return a default permission.
        }
    }
}