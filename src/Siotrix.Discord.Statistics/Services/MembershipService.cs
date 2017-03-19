using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Siotrix.Discord.Statistics
{
    public class MembershipService : IService
    {
        private DiscordSocketClient _client;
        private LogDatabase _db;

        public MembershipService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task StartAsync()
        {
            _db = new LogDatabase();
            _client.UserJoined += OnUserJoinedAsync;

            _client.UserLeft += OnUserLeftAsync;
            // _client.UserPresenceUpdated += OnUserPresenceUpdatedAsync;

            _client.UserLeft += OnUserLeftAsync;


            await PrettyConsole.LogAsync("Info", "Membership", "Service started successfully");
        }

        public async Task StopAsync()
        {
            _client.UserJoined -= OnUserJoinedAsync;

            _client.UserLeft -= OnUserLeftAsync;
            // _client.UserPresenceUpdated -= OnUserPresenceUpdatedAsync;
            _client.UserLeft -= OnUserLeftAsync;

            _db = null;

            await PrettyConsole.LogAsync("Info", "Membership", "Service stopped successfully").ConfigureAwait(false);
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            var member = EntityHelper.CreateMembership(user, true);

            _db.Memberships.Add(member);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task OnUserLeftAsync(SocketGuildUser user)
        {
            var member = EntityHelper.CreateMembership(user, false);

            _db.Memberships.Add(member);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}