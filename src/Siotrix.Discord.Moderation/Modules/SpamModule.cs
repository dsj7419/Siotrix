using Discord.Commands;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("spam")]
    [Summary("Special guild-specific spam moderation commands.")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class SpamModule : ModuleBase<SocketCommandContext>
    {
        [Name("no-help")]
        [Command]
        [Remarks("Receive a private message with information about this channel's spam filter.")]
        public async Task SpamAsync()
        {
            await Task.Delay(1);
        }
    }
}
