using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("cleanup"), Alias("clean")]
    [Summary("Delete various types of messages.")]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class CleanupModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        [Summary("Instantly cleanup a lot of Siotrx's recent messages..")]
        [Remarks("- no additional arguments needed.")]
        public async Task CleanAsync()
        {
            var self = Context.Guild.CurrentUser;
            var messages = (await GetMessageAsync(100)).Where(x => x.Author.Id == self.Id);

            if (self.GetPermissions(Context.Channel as SocketGuildChannel).ManageMessages)
                await DeleteMessagesAsync(messages);
            else
                foreach (var msg in messages)
                    await msg.DeleteAsync();

            var reply = await ReplyAsync($"Deleted **{messages.Count()}** message(s)");
            await DelayDeleteMessageAsync(reply);
        }

        [Command("all")]
        [Summary("Clean up past X amount of messages in channel.")]
        [Remarks(" <number> - cleanup past X amount of messages **note** if you do not put a number, default is 25.")]
        public async Task AllAsync(int history = 25)
        {
            var messages = await GetMessageAsync(history);
            await DeleteMessagesAsync(messages);

            await MessageExtensions.NumberOfCleanupMessages(messages.Count(), Context.User.Id);
            var reply = await ReplyAsync($"Deleted **{messages.Count()}** message(s)");
            await DelayDeleteMessageAsync(reply);
        }

        [Command("user")]
        [Summary("Clean up past X amount of messages in channel from a specific user.")]
        [Remarks( "<@username> <number> - cleanup past X amount of messages specific user wrote. Default is 25")]
        public async Task UserAsync(SocketUser user, int history = 25)
        {
            var messages = (await GetMessageAsync(history)).Where(x => x.Author.Id == user.Id);
            await DeleteMessagesAsync(messages);

            var reply = await ReplyAsync($"Deleted **{messages.Count()}** message(s) by **{user}**");
            await DelayDeleteMessageAsync(reply);
        }

        [Command("bots")]
        [Summary("Clean up past X amount of messages in channel from any channel bot.")]
        [Remarks(" <number> - cleanup past X amount of messages done by bots. Default is 25")]
        public async Task BotsAsync(int history = 25)
        {
            var messages = (await GetMessageAsync(history)).Where(x => x.Author.IsBot);
            await DeleteMessagesAsync(messages);

            var reply = await ReplyAsync($"Deleted **{messages.Count()}** message(s) by bots");
            await DelayDeleteMessageAsync(reply);
        }

        [Command("contains")]
        [Summary("Clean up past X amount of messages in channel that contains a keyword or words you provide.")]
        [Remarks(" <keyword(s)> <number> - cleanup past X amount of messages containing those parameters. Default is 25")]
        public async Task ContainsAsync(string text, int history = 25)
        {
            var messages = (await GetMessageAsync(history)).Where(x => x.Content.ToLower().Contains(text.ToLower()));
            await DeleteMessagesAsync(messages);

            var reply = await ReplyAsync($"Deleted **{messages.Count()}** message(s) containing `{text}`.");
            await DelayDeleteMessageAsync(reply);
        }

        [Command("attachments")]
        [Summary("Clean up past X amount of messages in channel with attachments.")]
        [Remarks(" <number> - cleanup past X amount of message attachments. Default is 25")]
        public async Task AttachmentsAsync(int history = 25)
        {
            var messages = (await GetMessageAsync(history)).Where(x => x.Attachments.Count() != 0);
            await DeleteMessagesAsync(messages);

            var reply = await ReplyAsync($"Deleted **{messages.Count()}** message(s) with attachments.");
            await DelayDeleteMessageAsync(reply);
        }

        private Task<IEnumerable<IMessage>> GetMessageAsync(int count)
            => Context.Channel.GetMessagesAsync(count).Flatten();

        private Task DeleteMessagesAsync(IEnumerable<IMessage> messages)
            => Context.Channel.DeleteMessagesAsync(messages);

        private async Task DelayDeleteMessageAsync(IMessage message, int ms = 5000)
        {
            await Task.Delay(ms);
            await message.DeleteAsync();
        }
    }
}