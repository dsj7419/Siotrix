using Doggo.Commands;
using System.Threading.Tasks;

namespace Doggo.Discord.Moderation
{
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
