using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    public class LeaveModule : ModuleBase<SocketCommandContext>
    {
        [Command("leave")]
        [Summary("Instructs the bot to leave this Guild.")]
        [Remarks(" - no additional arguments needed")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task Leave()
        {
            await ReplyAsync("Leaving~");
            await Context.Guild.LeaveAsync();
        }
    }
}