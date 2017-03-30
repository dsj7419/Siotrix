using Discord;
using Siotrix.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord.Developer
{
    class BroadcastModule : ModuleBase<SocketCommandContext>
    {
        [Command("broadcast")]
        [Summary("Broadcasts a message to the default channel of all servers the bot is connected to.")]
        [Remarks("broadcast IMPORTANT MESSAGE")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task Broadcast([Remainder] string broadcast)
        {
            var guilds = Context.Client.Guilds;
            var defaultChannels = guilds.Select(g => g.GetChannel(g.Id)).Cast<ITextChannel>();
            await Task.WhenAll(defaultChannels.Select(c => c.SendMessageAsync($"***ANNOUNCEMENT @everyone: " + broadcast + "\n-Thank-You... Bot Staff***")));
        }
    }
}
