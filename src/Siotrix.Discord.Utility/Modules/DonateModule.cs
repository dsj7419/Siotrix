using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Utility
{
    [Name("Utility")]
    [MinPermissions(AccessLevel.User)]
    public class DonateModule : ModuleBase<SocketCommandContext>
    {
        [Command("donate")]
        [Alias("don")]
        [Summary("Give likes to Patreon page to help support this community bot!")]
        [Remarks("- no additional arguments needed.")]
        [MinPermissions(AccessLevel.User)]
        public async Task Donate()
        {
            await ReplyAsync(
                $"Siotrix is a community bot, and needs your help to support it. All proceeds go back into the bot 100% <{SiotrixConstants.BotDonate}> **Thank You!!!**");
        }
    }
}