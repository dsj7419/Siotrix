using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("warn")]
    [Summary("Warn a misbehaving user.")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class WarnModule : ModuleBase<SocketCommandContext>
    {
        [Command("warn")]
        [Remarks("Increase a user's warning level by the specified amount")]
        public async Task WarnAsync(SocketUser user, int level, [Remainder]string reason)
        {
            await Task.Delay(1);
        }
    }
}