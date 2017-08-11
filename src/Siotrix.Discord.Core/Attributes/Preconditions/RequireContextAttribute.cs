using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Siotrix.Discord
{
    [Flags]
    public enum ContextType
    {
        Guild = 0x01,
        Dm = 0x02,
        Group = 0x04
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireContextAttribute : PreconditionAttribute
    {
        public RequireContextAttribute(ContextType contexts)
        {
            Contexts = contexts;
        }

        public ContextType Contexts { get; }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext icontext, CommandInfo command,
            IServiceProvider map)
        {
            var context = icontext as SocketCommandContext;
            var isValid = false;

            if ((Contexts & ContextType.Guild) != 0)
                isValid = isValid || context.Channel is IGuildChannel;
            if ((Contexts & ContextType.Dm) != 0)
                isValid = isValid || context.Channel is IDMChannel;
            if ((Contexts & ContextType.Group) != 0)
                isValid = isValid || context.Channel is IGroupChannel;

            if (isValid)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(
                PreconditionResult.FromError($"Invalid context for command; accepted contexts: {Contexts}"));
        }
    }
}