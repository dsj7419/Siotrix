using Discord.WebSocket;
using Doggo.Commands;
using Doggo.Discord.Admin;
using Doggo.Discord.Audio;
using Doggo.Discord.Events;
using Doggo.Discord.Moderation;
using Doggo.Discord.Roslyn;
using Doggo.Discord.Github;
using Doggo.Discord.Statistics;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Doggo.Discord
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;
        private DependencyMap _map;

        public CommandHandler(DiscordSocketClient client, DependencyMap map)
        {
            _client = client;
            _map = map;
        }

        public async Task StartAsync()
        {
            _service = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync
            });

            _service.AddTypeReader(typeof(Uri), new UriTypeReader());
            _service.AddTypeReader(typeof(TimeSpan), new TimeSpanTypeReader());
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());

            var config = Configuration.Load();
            if (config.Modules.Admin)
                await _service.LoadAdminAsync();
            if (config.Modules.Audio)
                await _service.LoadAudioAsync();
            if (config.Modules.Events)
                await _service.LoadEventsAsync();
            if (config.Modules.Moderation)
                await _service.LoadModerationAsync();
            if (config.Modules.Roslyn)
                await _service.LoadRoslynAsync();
            if (config.Modules.Github)
                await _service.LoadGithubAsync();
            if (config.Modules.Statistics)
                await _service.LoadStatisticsAsync();

            _client.MessageReceived += HandleCommandAsync;
            await PrettyConsole.LogAsync("Info", "Commands", $"Loaded {_service.Commands.Count()} commands");
        }

        public Task StopAsync()
        {
            _service = null;
            _client.MessageReceived -= HandleCommandAsync;
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null)
                return;

            var context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos, _map);

                if (!result.IsSuccess)
                {
                    if (result.Error == CommandError.UnknownCommand)
                    {
                        await context.ReplyAsync("Command not recognized");
                        return;
                    }

                    if (result is ExecuteResult r)
                        PrettyConsole.NewLine(r.Exception.ToString());
                    else
                        await context.ReplyAsync(result.ToString());
                }
            }
        }
    }
}
