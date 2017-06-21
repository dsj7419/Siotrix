using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    public class BroadcastModule : ModuleBase<SocketCommandContext>
    {
        [Command("broadcast")]
        [Summary("Broadcasts a message to the default channel of all servers the bot is connected to.")]
        [Remarks(" <text> - Whatever important information you need to say goes here.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task Broadcast([Remainder] string broadcast)
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Name = Context.User.Username
                },
                Color = new Color(1, 1, 1),
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                    Text = $"{Context.Client.CurrentUser.Username} | ATTENTION"
                },
                Title = "Global Broadcast",
                Description = broadcast,
                Timestamp = DateTime.Now
            };

            var guilds = Context.Client.Guilds;

            await Context.Channel.SendMessageSafeAsync($":white_check_mark: I will notify all my guild owners!");

            foreach (var guild in guilds)
            {
                var dmChannel = (IDMChannel)Context.Client.DMChannels.SingleOrDefault(c => c.Recipient.Id == guild.Owner.Id) ??
                    await guild.Owner.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageSafeAsync($"This announcement was sent to you because you are the owner of the {guild.Name} server, which I am also on.", embed: builder.Build());
            }

            await Task.Delay(20000);
        }
    }
}