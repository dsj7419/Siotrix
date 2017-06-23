using Discord.Commands;
using System.Threading.Tasks;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    public class PowerDownModule : ModuleBase<SocketCommandContext>
    {
        [Command("powerdown"), Alias("pd")]
        [Summary("Shuts down Siotrix.")]
        [Remarks(" - no additional arguments needed")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task PowerdownAsync()
        {
            //TODO: add a proper powerdown for bot!
            await ReplyAsync("Powering down!").ConfigureAwait(false);
            await Context.Client.StopAsync().ConfigureAwait(false);
            await Task.Delay(1500).ConfigureAwait(false);
        }
    }
}