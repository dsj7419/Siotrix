using Discord.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("spam")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class SpamModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        [Remarks("Receive a private message with information about this channel's spam filter.")]
        public async Task SpamAsync()
        {
            await Task.Delay(1);
        }
    }
}
