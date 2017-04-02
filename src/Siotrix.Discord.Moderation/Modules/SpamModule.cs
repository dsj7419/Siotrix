using Discord.Commands;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("spam")]
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
