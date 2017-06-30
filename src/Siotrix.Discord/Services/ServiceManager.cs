using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Siotrix.Discord.Audio;
using Siotrix.Discord.Moderation;
using Siotrix.Discord.Statistics;
using System.Threading.Tasks;
using System;
using Discord.Addons.InteractiveCommands;

namespace Siotrix.Discord
{
    public class ServiceManager : IService
    {
        public SiotrixModules Modules => _modules;

        private readonly DiscordSocketClient _client;
        private SiotrixModules _modules;
        private Configuration _config;


        // General
        private GlobalErrorHandler _handler;
      //  private CommandHandler _commands;

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
        private LogService _log;

        public ServiceManager(DiscordSocketClient client)
        {
            _client = client;
            _config = Configuration.Load();
            _modules = new SiotrixModules();
        }

        public async Task StartAsync()
        {
            _handler = new GlobalErrorHandler();
            await _handler.StartAsync().ConfigureAwait(false);

            if (_config.Modules.Audio)
                await StartAudioAsync().ConfigureAwait(false);
            if (_config.Modules.Events)
                await StartEventsAsync().ConfigureAwait(false);
            if (_config.Modules.Statistics)
                await StartStatisticsAsync().ConfigureAwait(false);

            //if (!_map.TryAdd(this))
            //    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add self to map");
            //_commands = new CommandHandler(_client, _map);

            await ConfigureServices();

       //     await _commands.StartAsync().ConfigureAwait(false);

            if (_config.Modules.Moderation)
                await StartModerationAsync().ConfigureAwait(false);

            //var handler = provider.GetService<CommandHandler>();
           // var handler = new CommandHandler(provider);
          //  await handler.StartAsync();
        }      


        public async Task StopAsync()
        {
            if (_config.Modules.Audio)
                await StopAudioAsync();
            if (_config.Modules.Events)
                await StopEventsAsync();
            if (_config.Modules.Statistics)
                await StopStatisticsAsync();
            if (_config.Modules.Moderation)
                await StopModerationAsync();

          //  await _commands.StopAsync();
        }

        #region Start

        public async Task StartAudioAsync()
        {
            _modules.Audio = true;
            _audio = new AudioService();

            await _audio.StartAsync();

            //if (!_map.TryAdd(_audio))
            //    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Audio to map");

        }

        public Task StartEventsAsync()
        {
            _modules.Events = true;
            return Task.CompletedTask;
        }
        
        public async Task StartModerationAsync()
        {
            _modules.Moderation = true;
            _antispam = new AntispamService(_client);
            _filter = new FilterService(_client);
            _warning = new WarningService(_client);
            _log = new LogService(_client);

            await _antispam.StartAsync();
            await _warning.StartAsync();
            await _filter.StartAsync();
            await _log.StartAsync();
            //await _warning.StartAsync();

            //if (!_map.TryAdd(_antispam))
            //    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Antispam to map");
            //if (!_map.TryAdd(_filter))
            //    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Filter to map");
            //if (!_map.TryAdd(_warning))
            //    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Warning to map");
            //if (!_map.TryAdd(_log))
            //    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Log to map");


        }

        public async Task StartStatisticsAsync()
        {
            _modules.Statistics = true;
            _membership = new MembershipService(_client);
            _message = new MessageService(_client);

            await _membership.StartAsync();
            await _message.StartAsync();

            //if (!_map.TryAdd(_membership))
            //    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Membership to map");
            //if (!_map.TryAdd(_message))
            //    await PrettyConsole.LogAsync("Error", "Manager", "Unable to add Message to map");

        }

        #endregion
        #region Stop

        public async Task StopAudioAsync()
        {
            _modules.Audio = false;
            _audio = null;
            await _audio.StopAsync();
        }

        public Task StopEventsAsync()
        {
            _modules.Events = false;
            return Task.CompletedTask;
        }

        public async Task StopModerationAsync()
        {
            _modules.Moderation = false;
            _antispam = null;
            _filter = null;
            _warning = null;
            _log = null;

            await _antispam.StopAsync();
            await _filter.StopAsync();
            await _warning.StopAsync();
            await _log.StopAsync();
        }

        public async Task StopStatisticsAsync()
        {
            _modules.Statistics = false;
            _membership = null;
            _message = null;

            await _membership.StopAsync();
            await _message.StopAsync();
        }

        #endregion

        public async Task ConfigureServices()
        {
            var config = Configuration.Load();

            var services = new ServiceCollection()
                .AddSingleton<CommandHandler>()
             //   .AddSingleton<Random>()
                .AddSingleton(_client)
                .AddSingleton(config)
                .AddSingleton(new CommandService(new CommandServiceConfig()
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Info,
                    ThrowOnError = false
                }))
                .AddSingleton(new InteractiveService(_client));

            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
            var handler = provider.GetService<CommandHandler>();
            await handler.StartAsync();
           // return provider;
        }
    }
}
