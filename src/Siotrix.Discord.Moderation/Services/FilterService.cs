using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    public class FilterService : IService
    {
        private DiscordSocketClient _client;

        public FilterService(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task StartAsync()
        {
            _client.MessageReceived += OnMessageReceivedAsync;
            _client.MessageUpdated += OnMessageUpdatedAsync;
            _client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            _client.MessageUpdated -= OnMessageUpdatedAsync;
            _client.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
            return Task.CompletedTask;
        }
        
        private Task OnMessageReceivedAsync(SocketMessage msg)
        {
            return Task.CompletedTask;
        }

        private Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> cachemsg, SocketMessage msg, ISocketMessageChannel channel)
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
