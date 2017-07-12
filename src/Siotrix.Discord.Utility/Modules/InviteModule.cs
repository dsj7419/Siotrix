using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Utility
{
    [Name("Utility")]
    [MinPermissions(AccessLevel.User)]
    public class InviteModule : ModuleBase<SocketCommandContext>
    {

        [Command("invite"), Alias("join")]
        [Summary("Returns the OAuth2 Invite URL of the bot")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.User)]
        public async Task Invite()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync($"Invite me to your server at:{Environment.NewLine}<{SiotrixConstants.BOT_INVITE}>");
        }
    }
}
