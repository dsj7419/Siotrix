using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    public class ForgiveModule : ModuleBase<SocketCommandContext>
    {
        [Command("Forgive")]
        [Summary("Forgive a user from ALL of their previous warnings.")]
        [Remarks(" - No additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task ForgiveAsync(SocketGuildUser user)
        {
            var success = DeleteWarningUser(user.Id.ToLong(), Context.Guild.Id.ToLong());
            if (success)
                await ReplyAsync(SiotrixConstants.BotSuccess);
            else
                await ReplyAsync("That person is not found!");
        }
    }
}