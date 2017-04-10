using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Siotrix.Discord.Attributes.Preconditions;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    public class BroadcastModule : ModuleBase<SocketCommandContext>
    {
        [Command("broadcast")]
        [Summary("Broadcasts a message to the default channel of all servers the bot is connected to.")]
        [Remarks("<text> - Whatever important information you need to say goes here.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task Broadcast([Remainder] string broadcast)
        {
            var guilds = (Context.Client as DiscordSocketClient).Guilds;
            var defaultChannels = guilds.Select(g => g.GetChannel(g.Id)).Cast<ITextChannel>();
            await Task.WhenAll(defaultChannels.Select(c => c.SendMessageAsync($"***ANNOUNCEMENT @everyone: " + broadcast + "\n-Thank-You... Bot Staff***")));
        }
    }
}
