using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Siotrix.Discord.Attributes.Preconditions;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("cleanup"), Alias("clean")]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class CleanupModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task CleanAsync()
        {
            var self = Context.Guild.CurrentUser;
            var messages = (await GetMessageAsync(10)).Where(x => x.Author.Id == (ulong)self.Id);

            await DeleteMessagesAsync(messages);
            await ReplyAsync($"Deleted **{messages.Count()}** messages(s).");
        }

        [Command("all")]
        public async Task AllAsync(int history = 25)
        {
            var messages = await GetMessageAsync(history);
            await DeleteMessagesAsync(messages);
            await ReplyAsync($"Deleted **{messages.Count()}** messages(s).");
        }

        [Command("user")]
        public async Task UserAsync(SocketUser user, int history = 25)
        {
            var messages = (await GetMessageAsync(history)).Where(x => x.Author.Id == user.Id);
            await DeleteMessagesAsync(messages);
            await ReplyAsync($"Deleted **{messages.Count()}** messages(s) by **{user}.");
        }

        [Command("bots")]
        public async Task BotsAsync(int history = 25)
        {
            var messages = (await GetMessageAsync(history)).Where(x => x.Author.IsBot);
            await DeleteMessagesAsync(messages);
            await ReplyAsync($"Deleted **{messages.Count()}** messages(s) by bots.");
        }

        [Command("contains")]
        public async Task ContainsAsync(string text, int history = 25)
        {
            var messages = (await GetMessageAsync(history)).Where(x => x.Content.ToLower().Contains(text.ToLower()));
            await DeleteMessagesAsync(messages);
            await ReplyAsync($"Deleted **{messages.Count()}** messages(s) containing '{text}'.");
        }

        [Command("attachments")]
        public async Task AttachmentsAsync(int history = 25)
        {
            var messages = (await GetMessageAsync(history)).Where(x => x.Attachments.Count() != 0);
            await DeleteMessagesAsync(messages);
            await ReplyAsync($"Deleted **{messages.Count()}** messages(s) with attachments.");
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