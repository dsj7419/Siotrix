using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Doggo.Discord.Statistics
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
            _client.UserPresenceUpdated += OnUserPresenceUpdatedAsync;

            await PrettyConsole.LogAsync("Info", "Membership", "Service started successfully");
        }

        public async Task StopAsync()
        {
            _client.UserJoined -= OnUserJoinedAsync;
            _client.UserLeft -= OnUserLeftAsync;
            _client.UserPresenceUpdated -= OnUserPresenceUpdatedAsync;
            _db = null;

            await PrettyConsole.LogAsync("Info", "Membership", "Service stopped successfully");
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            var member = EntityHelper.CreateMembership(user, true);

            await _db.Memberships.AddAsync(member);
            await _db.SaveChangesAsync();
        }

        private async Task OnUserLeftAsync(SocketGuildUser user)
        {
            var member = EntityHelper.CreateMembership(user, false);

            await _db.Memberships.AddAsync(member);
            await _db.SaveChangesAsync();
        }

        private async Task OnUserPresenceUpdatedAsync(Optional<SocketGuild> g, SocketUser user, SocketPresence before, SocketPresence after)
        {
            if (before.Status == after.Status)
                return;

            var guild = g.IsSpecified ? g.Value : null;
            var status = EntityHelper.CreateStatus(after, user, guild);

            await _db.Statuses.AddAsync(status);
            await _db.SaveChangesAsync();
        }
    }
}
