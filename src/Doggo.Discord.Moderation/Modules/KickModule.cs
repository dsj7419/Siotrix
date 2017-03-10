using Discord.WebSocket;
using Doggo.Commands;
using System.Threading.Tasks;

namespace Doggo.Discord.Moderation
{
    public class KickModule : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        public async Task KickAsync(SocketGuildUser user)
        {
            await user.KickAsync();
            await Context.ReplyAsync("👍");
        }

        [Command("kick")]
        public async Task KickAsync(SocketUser user, int prunedays = -1)
        {
            int prune = prunedays == -1 ? 0 : prunedays;
            await Context.Guild.AddBanAsync(user, prune);
            await Context.Guild.RemoveBanAsync(user);
            await Context.ReplyAsync("👍");
        }
    }
}
