using Discord.Commands;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("filter")]
    [Summary("A special guild-specific word and phrase filter.")]
    public class FilterModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        [Remarks("Receive a private message listing words filtered from this channel.")]
        public async Task FilterAsync()
        {
            await Task.Delay(1);
        }

        [Command("add")]
        [Remarks("Add a new word to this channel's filter.")]
        public async Task AddAsync(string word, int warning = 0)
        {
            await Task.Delay(1);
        }

        [Command("remove")]
        [Remarks("Remove an existing word from this channel's filter.")]
        public async Task RemoveAsync(string word, int warning = 0)
        {
            await Task.Delay(1);
        }
    }
}
