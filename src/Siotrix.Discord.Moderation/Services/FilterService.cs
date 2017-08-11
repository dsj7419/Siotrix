using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    public class FilterService : IService
    {
        private readonly DiscordSocketClient _client;

        public FilterService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task StartAsync()
        {
            _client.MessageReceived += OnMessageReceivedAsync;
            _client.MessageUpdated += OnMessageUpdatedAsync;
            _client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
            await PrettyConsole.LogAsync("Info", "Filter", "Service started successfully").ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            _client.MessageUpdated -= OnMessageUpdatedAsync;
            _client.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
            await PrettyConsole.LogAsync("Info", "Filter", "Service stopped successfully").ConfigureAwait(false);
        }

        private async Task OnMessageReceivedAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            var argPos = 0;
            string spec = null;
            var context = new SocketCommandContext(_client, message);
            var dictionary = LogChannelExtensions.GetFilterWords(context.Guild.Id.ToLong());
            var words = msg.Content.Split(' ');
            LogChannelExtensions.IsUsableLogChannel(context.Guild.Id.ToLong());
            var channel =
                context.Guild.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
            var badword = LogChannelExtensions.ParseMessages(words, dictionary);
            if (badword != null)
            {
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(msg.Author.GetAvatarUrl())
                        .WithName("Found --" + badword + "-- word from message of " + msg.Author.Username + "#" +
                                  msg.Author.Discriminator))
                    .WithColor(new Color(255, 0, 0));
                await channel.SendMessageAsync("", false, builder.Build());
                await msg.DeleteAsync();
            }

            spec = context.GetGuildPrefix();
            var isFound = IsUsableAutoDeleteCommand(context.Guild.Id.ToLong());
            if (message.HasStringPrefix(spec, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                if (isFound)
                    await message.DeleteAsync();
        }

        private bool IsUsableAutoDeleteCommand(long guildId)
        {
            var isFound = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Gautodeletes.Where(x => x.GuildId == guildId);
                    if (data.Any())
                        isFound = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isFound;
        }

        private Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> cachemsg, SocketMessage msg,
            ISocketMessageChannel channel)
        {
            return Task.CompletedTask;
        }

        private Task OnGuildMemberUpdatedAsync(SocketGuildUser before, SocketGuildUser after)
        {
            return Task.CompletedTask;
        }

        private bool ContainsFilteredWord(string value)
        {
            return false;
        }
    }
}