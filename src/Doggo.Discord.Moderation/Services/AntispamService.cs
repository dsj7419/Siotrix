using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doggo.Discord.Moderation
{
    public class AntispamService : IService
    {
        private const int _maxDiff = 5;             // Max difference to trigger the antispam
        private const int _singleUserTolerance = 3; // Min similar messages to begin actions against the user
        private const int _multiUserTolerance = 6;  // Min similar messages to begin actions against users
        private const int _expirySeconds = 10;      // Max time between messages to be considered spam

        private DiscordSocketClient _client;

        public AntispamService(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task StartAsync()
        {
            _client.MessageReceived += OnMessageReceivedAsync;
            return Task.CompletedTask;
        }
        
        public Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            return Task.CompletedTask;
        }
        
        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!HasManageMessages(s))
                return;




            await Task.Delay(0);
        }

        private bool HasManageMessages(SocketMessage s)
        {
            var channel = s.Channel as SocketTextChannel;
            if (channel == null)
                return false;
            var permissions = channel.Guild.CurrentUser.GetPermissions(channel);
            if (!permissions.ManageMessages)
                return false;
            return true;
        }
    }
}
