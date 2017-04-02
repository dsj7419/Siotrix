using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("warning")]
    public class WarningModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        [Remarks("Check your current warning level")]
        public async Task WarningAsync()
        {
            await Task.Delay(1);
        }

        [Command]
        [Remarks("Check warnings you received between two dates")]
        public async Task WarningAsync(DateTime from, DateTime to)
        {
            await Task.Delay(1);
        }

        [Command]
        [Remarks("View information about a specific warning")]
        public async Task WarningAsync(int id)
        {
            await Task.Delay(1);
        }
    }
}
