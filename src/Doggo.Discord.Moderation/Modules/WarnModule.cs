using Discord.WebSocket;
using Doggo.Commands;
using System.Threading.Tasks;

namespace Doggo.Discord.Moderation
{
    [Group("warn")]
    public class WarnModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        [Remarks("Increase a user's warning level by the specified amount")]
        public async Task WarnAsync(SocketUser user, int level, [Remainder]string reason)
        {
            await Task.Delay(1);
        }
    }
}
