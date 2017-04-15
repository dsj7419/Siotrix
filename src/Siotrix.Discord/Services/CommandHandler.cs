using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Siotrix.Discord.Admin;
using Siotrix.Discord.Audio;
using Siotrix.Discord.Events;
using Siotrix.Discord.Utility;
using Siotrix.Discord.Developer;
using Siotrix.Discord.Moderation;
using Siotrix.Discord.Roslyn;
using Siotrix.Discord.Statistics;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Siotrix.Discord.Readers;
using Discord.Addons.InteractiveCommands;

namespace Siotrix.Discord
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
            _map.Add(new InteractiveService(_client));
        }

        public async Task StartAsync()
        {
            _service = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
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
            if (config.Modules.Utility)
                await _service.LoadUtilityAsync();
            if (config.Modules.Developer)
                await _service.LoadDeveloperAsync();
            if (config.Modules.Moderation)
                await _service.LoadModerationAsync();
            if (config.Modules.Roslyn)
                await _service.LoadRoslynAsync();
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
            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            string spec = null;
            using (var db = new LogDatabase())
            {
                var guild_id = context.Guild.Id;
                try
                {
                    var arr = db.Gprefixs.Where(p => p.GuildId == guild_id.ToLong());
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        spec = "!"; 
                    }
                    else
                    {
                        spec = arr.First().Prefix;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (s.Author.IsBot
                || msg == null
                || !msg.Content.Except("?").Any()
                || msg.Content.Trim().Length <= 1
                || msg.Content.Trim()[1] == '?'
                || (!(msg.HasStringPrefix(spec, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))))
                return;

            if (msg.HasStringPrefix(spec, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos, _map);

                if (!result.IsSuccess)
                {
                    if (result.Error == CommandError.UnknownCommand)
                    {
                        await context.Channel.SendMessageAsync("Command not recognized");
                        return;
                    }

                    if (result is ExecuteResult r)
                        PrettyConsole.NewLine(r.Exception.ToString());
                    else {
                    var embed = new EmbedBuilder().WithColor(new Color(255, 0, 0)).WithTitle("**Error:**").WithDescription(result.ErrorReason);
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }
    }
}
