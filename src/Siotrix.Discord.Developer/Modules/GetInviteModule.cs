using Discord.WebSocket;
using Siotrix.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System.Threading.Tasks;

namespace Siotrix.Discord.Developer
{
    class GetInviteModule : ModuleBase<SocketCommandContext>
    {
        [Command("getinvite")]
        [Summary("Makes an invite to the specified guild")]
        [Remarks("getinvite 123456789987654321")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task GetInviteAsync([Summary("Target guild id")]ulong guild)
        {
            var channel = Context.Client.GetChannel((Context.Client.GetGuild(guild)).DefaultChannel.Id);
            var invite = await (channel as SocketGuildChannel).CreateInviteAsync();
            await Context.ReplyAsync(invite.Url);
        }
    }
}
