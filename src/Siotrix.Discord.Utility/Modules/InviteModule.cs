using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Siotrix.Discord.Utility
{
    [Name("Basic")]
    public class InviteModule : ModuleBase<SocketCommandContext>
    {

        [Command("invite"), Alias("join")]
        [Summary("Returns the OAuth2 Invite URL of the bot")]
        [MinPermissions(AccessLevel.User)]
        public async Task Invite()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync($"Invite me to your server at:{Environment.NewLine}<https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot>");
        }
    }
}
