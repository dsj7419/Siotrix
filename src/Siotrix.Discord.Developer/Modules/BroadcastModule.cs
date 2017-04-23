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
        [Remarks("<text> - Whatever important information you need to say goes here.")]
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
                    Text = $"{Context.Client.CurrentUser.Username} | Announcement"
                },
                Title = "Global Announcement",
                Description = broadcast,
                Timestamp = DateTime.Now
            };

            var guilds = Context.Client.Guilds;
            new Task(async () =>
            {
                foreach (var guild in guilds)
                {
                    if (guild.DefaultChannel.UserHasPermission(Context.Guild.GetUser(Context.Client.CurrentUser.Id), ChannelPermission.SendMessages))
                        await guild.DefaultChannel.SendMessageSafeAsync(guild.Owner.Mention, embed: builder.Build());
                    else
                    {
                        var dmChannel = (IDMChannel)Context.Client.DMChannels.SingleOrDefault(c => c.Recipient.Id == guild.Owner.Id) ??
                            await guild.Owner.CreateDMChannelAsync();
                        await dmChannel.SendMessageSafeAsync($"I was unable to send this message in the default channel of {guild.Name} (#{guild.DefaultChannel.Name})", embed: builder.Build());
                    }
                }
            }).Start();

            await Context.Channel.SendMessageAsync("Consider it done!");

            /*  var guilds = (Context.Client as DiscordSocketClient).Guilds;
              var defaultChannels = guilds.Select(g => g.GetChannel(g.Id)).Cast<ITextChannel>();
              await Task.WhenAll(defaultChannels.Select(c => c.SendMessageAsync($"***ANNOUNCEMENT @everyone: " + broadcast + "\n-Thank-You... Bot Staff***"))); */
        }
    }
}
