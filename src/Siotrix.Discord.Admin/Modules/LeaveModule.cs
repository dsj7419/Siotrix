using Siotrix.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System.Threading.Tasks;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    public class LeaveModule : ModuleBase<SocketCommandContext>
    {

        [Command("leave")]
        [Summary("Instructs the bot to leave this Guild.")]
        [Remarks("leave")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task Leave()
        {
            if (Context.Guild == null) { await Context.ReplyAsync("This command can only be ran in your guild."); return; }
            await Context.ReplyAsync("Leaving~");
            await Context.Guild.LeaveAsync();
        }
    }
}


