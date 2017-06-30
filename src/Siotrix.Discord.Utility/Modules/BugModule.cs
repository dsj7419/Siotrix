using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Utility
{
    [Name("Utility")]
    [MinPermissions(AccessLevel.User)]
    public class BugModule : ModuleBase<SocketCommandContext>
    {

        [Command("bug"), Alias("bugs")]
        [Summary("Report a bug to the developers of this bot!")]
        [Remarks(" (bug text)")]
        [MinPermissions(AccessLevel.User)]
        public async Task BugAsync([Remainder]string message)
        {
            var suggestionChannel = Context.Client.GetChannel(328338772796112907) as IMessageChannel;

            if (suggestionChannel == null)
            {
                await ReplyAsync("That channel does not exist.");
                return;
            }

            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $"{Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Timestamp = DateTime.Now,
                Color = new Color(1, 1, 1)
            }
            .AddField("Bug Report", message)
            .AddInlineField(Context.Guild?.Name ?? Context.User.Username, Context.Guild?.Id ?? Context.User.Id)
            .AddInlineField(Context.Channel.Name, Context.Channel.Id);
            await suggestionChannel.SendMessageSafeAsync("", embed: builder.Build());
            await ReplyAsync("Bug Reported. Thank You for helping!");
        }
    }
}

