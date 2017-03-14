using Discord.WebSocket;
using Siotrix.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    public class BanModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        public async Task BanAsync(SocketUser user, int prunedays = -1)
        {
            int prune = prunedays == -1 ? 0 : prunedays;
            await Context.Guild.AddBanAsync(user, prune);
            await Context.ReplyAsync("👍");
        }

        [Command("tempban")]
        public async Task TempBanAsync(SocketUser user, [Remainder]TimeSpan duration)
        {
            await Context.Guild.AddBanAsync(user);
            await Context.ReplyAsync("👍");
        }
    }
}
