using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Doggo.Discord.Moderation
{
    public class WarningService : IService
    {
        private DiscordSocketClient _client;

        public WarningService(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task StartAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
