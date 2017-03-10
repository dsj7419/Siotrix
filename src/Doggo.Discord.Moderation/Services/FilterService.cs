using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Doggo.Discord.Moderation
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
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
