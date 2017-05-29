using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequireBotPermissionAttribute : PreconditionAttribute
    {
        public GuildPermission? GuildPermission { get; }
        public ChannelPermission? ChannelPermission { get; }
        
        public RequireBotPermissionAttribute(GuildPermission permission)
        {
            GuildPermission = permission;
            ChannelPermission = null;
        }

        public RequireBotPermissionAttribute(ChannelPermission permission)
        {
            ChannelPermission = permission;
            GuildPermission = null;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext icontext, CommandInfo command, IServiceProvider map)
        {
            var context = icontext as SocketCommandContext;
            var guildUser = context.Guild.CurrentUser;

            if (GuildPermission.HasValue)
            {
                if (guildUser == null)
                    return Task.FromResult(PreconditionResult.FromError("Command must be used in a guild channel"));
                if (!guildUser.GuildPermissions.Has(GuildPermission.Value))
                    return Task.FromResult(PreconditionResult.FromError($"Command requires guild permission {GuildPermission.Value}"));
            }

            if (ChannelPermission.HasValue)
            {
                var guildChannel = context.Channel as IGuildChannel;

                ChannelPermissions perms;
                if (guildChannel != null)
                    perms = guildUser.GetPermissions(guildChannel);
                else
                    perms = ChannelPermissions.All(guildChannel);

                if (!perms.Has(ChannelPermission.Value))
                    return Task.FromResult(PreconditionResult.FromError($"Command requires channel permission {ChannelPermission.Value}"));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
