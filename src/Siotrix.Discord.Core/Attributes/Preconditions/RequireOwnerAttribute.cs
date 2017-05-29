using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireOwnerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext icontext, CommandInfo command, IServiceProvider map)
        {
            var context = icontext as SocketCommandContext;
            var appinfo = await context.Client.GetApplicationInfoAsync();

            if (context.User.Id == appinfo.Owner.Id)
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("You are not my owner");
        }
    }
}
