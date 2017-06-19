using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildAdmin)]
    public class AnnouncementModule : ModuleBase<SocketCommandContext>
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