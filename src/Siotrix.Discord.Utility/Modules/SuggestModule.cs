using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Utility
{
    [Name("Utility")]
    [MinPermissions(AccessLevel.User)]
    public class SuggestModule : ModuleBase<SocketCommandContext>
    {

        [Command("suggest"), Alias("suggestion", "idea")]
        [Summary("Give a suggestion or idea to the developers of this bot!")]
        [Remarks(" (suggestion text)")]
        [MinPermissions(AccessLevel.User)]
        public async Task SuggestAsync([Remainder]string message)
        {
            var suggestionChannel = Context.Client.GetChannel(328336838844743681) as IMessageChannel;

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
            .AddField("Suggestion", message)
            .AddInlineField(Context.Guild?.Name ?? Context.User.Username, Context.Guild?.Id ?? Context.User.Id)
            .AddInlineField(Context.Channel.Name, Context.Channel.Id);
            await suggestionChannel.SendMessageSafeAsync("", embed: builder.Build());
            await ReplyAsync("Suggestion sent. Thank You for helping!");
        }
    }
}
