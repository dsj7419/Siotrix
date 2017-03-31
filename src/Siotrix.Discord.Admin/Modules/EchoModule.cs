using Discord;
using Siotrix.Commands;
using Discord.WebSocket;
using Siotrix.Discord.Attributes.Preconditions;
using System.Threading.Tasks;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    public class EchoModule : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        [Summary("Echo's input into a specified channel.")]
        [Remarks("say #general I am alive!")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task EchoAsync([Summary("Target channel")] IGuildChannel channel, [Remainder, Summary("Text to echo")] string text)
        {
            await (Context.Client.GetChannel(channel.Id) as SocketTextChannel)?.SendMessageAsync(text);
        }
    }
}
