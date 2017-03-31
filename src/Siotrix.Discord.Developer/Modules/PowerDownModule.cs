using Siotrix.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System.Threading.Tasks;
namespace Siotrix.Discord.Developer
{
    [Name("Bot Owner")]
    public class PowerDownModule : ModuleBase<SocketCommandContext>
    {
        [Command("powerdown"), Alias("pd")]
        [Summary("Terminates the bot application")]
        [Remarks("powerdown")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task PowerdownAsync()
        {
            await Context.ReplyAsync("Powering down!").ConfigureAwait(false);
            await Context.Client.StopAsync().ConfigureAwait(false);
            await Task.Delay(1500).ConfigureAwait(false);
        }
    }
}