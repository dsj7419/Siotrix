using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using Siotrix.Discord.Attributes.Preconditions;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    public class BanModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [Summary("Ban a user")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task BanAsync(SocketUser user, int prunedays = -1)
        {
            int prune = prunedays == -1 ? 0 : prunedays;
            await Context.Guild.AddBanAsync(user, prune);
            await ReplyAsync("👍");
        }

        [Command("tempban")]
        [Summary("Ban a user for X amount of time/days")]
        [Remarks("<time> - eg. 1h or 2d - can use weeks days hours minutes.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task TempBanAsync(SocketUser user, [Remainder]TimeSpan duration)
        {
            await Context.Guild.AddBanAsync(user);
            await ReplyAsync("👍");
        }
    }
}
