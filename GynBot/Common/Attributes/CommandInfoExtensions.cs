namespace GynBot.Common.Attributes
{
    using Discord;
    using Discord.Commands;
    using System.Linq;
    using System.Threading.Tasks;

    public static class CommandInfoExtensions
    {
        public static bool CanExecute(this CommandInfo cmd, SocketCommandContext ctx)
        {
            return (cmd.CheckPreconditionsAsync(ctx)).GetAwaiter().GetResult().IsSuccess;
        }

        public static bool CanExecute(this ModuleInfo mod, SocketCommandContext ctx)
        {
            return mod.Commands.Any(c => c.CanExecute(ctx));
        }
    }
}