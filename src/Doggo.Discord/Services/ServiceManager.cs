using Discord.WebSocket;
using Doggo.Commands;
using Doggo.Discord.Audio;
using Doggo.Discord.Moderation;
using Doggo.Discord.Statistics;
using System.Threading.Tasks;

namespace Doggo.Discord
{
    public class ServiceManager : IService
    {
        private GlobalErrorHandler _handler;
        private DiscordSocketClient _client;
        private CommandHandler _commands;
        private Configuration _config;
        private DependencyMap _map;

        // Audio
        private AudioService _audio;

        // Events

        // Statistics
        private MembershipService _membership;
        private MessageService _message;

        // Moderation
        private AntispamService _antispam;
        private FilterService _filter;
        private WarningService _warning;

        public ServiceManager(DiscordSocketClient client)
        {
            _client = client;
            _map = new DependencyMap();
            _config = Configuration.Load();
        }

        public async Task StartAsync()
        {
            _handler = new GlobalErrorHandler();
            await _handler.StartAsync();
            
            if (_config.Modules.Audio)
            {
                _audio = new AudioService();

                await _audio.StartAsync();

                if (!_map.TryAdd(_audio))
                    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Audio to map");
            }

            if (_config.Modules.Events)
            {
                
            }

            if (_config.Modules.Statistics)
            {
                _membership = new MembershipService(_client);
                _message = new MessageService(_client);

                await _membership.StartAsync();
                await _message.StartAsync();
                
                if (!_map.TryAdd(_membership))
                    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Membership to map");
                if (!_map.TryAdd(_message))
                    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Message to map");
            }

            if (_config.Modules.Moderation)
            {
                _antispam = new AntispamService(_client);
                _filter = new FilterService(_client);
                _warning = new WarningService(_client);

                await _antispam.StartAsync();
                await _filter.StartAsync();
              //  await _warning.StartAsync();

                if (!_map.TryAdd(_antispam))
                    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Antispam to map");
                if (!_map.TryAdd(_filter))
                    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Filter to map");
                if (!_map.TryAdd(_warning))
                    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Warning to map");
            }

            _commands = new CommandHandler(_client, _map);
            await _commands.StartAsync();
        }

        public async Task StopAsync()
        {
            if (_config.Modules.Audio)
            {
                await _audio.StopAsync();
            }

            if (_config.Modules.Events)
            {

            }

            if (_config.Modules.Statistics)
            {
                await _membership.StopAsync();
                await _message.StopAsync();
            }

            if (_config.Modules.Moderation)
            {
                await _antispam.StopAsync();
                await _filter.StopAsync();
                await _warning.StopAsync();
            }

            await _commands.StopAsync();
        }
    }
}
