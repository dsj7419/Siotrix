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

        private string ParseMentionPrefix(IUserMessage msg)
        {
            var text = msg.Content;
            if (text.Length <= 3 || text[0] != '<' || text[1] != '@') return null;

            int endPos = text.IndexOf('>');
            if (endPos == -1) return null;
            if (text.Length < endPos + 2 || text[endPos + 1] != ' ') return null;

            if (!MentionUtils.TryParseUser(text.Substring(0, endPos + 1), out ulong userId)) return null;
           // Console.WriteLine("^^^^^^^{0}`````````{1}***{2}", endPos, text[endPos], text.Length);
            string value = text.Substring(endPos + 2, text.Length - text.Substring(0, endPos + 1).Length - 1);
            return value;
        }

        private string ParseStringPrefix(IUserMessage msg, string spec)
        {
            var text = msg.Content;
            string value = text.Substring(spec.Length, text.Length - spec.Length);
            return value;
        }

        private string CheckAvaliableCommand(string[] words, SocketCommandContext context)
        {
            string cmd = words[0];
            string val = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var is_found_cmd = db.Gtoggles.Where(p => p.CommandName.Equals(cmd) && p.GuildId.Equals(context.Guild.Id.ToLong())).Any();
                    if (is_found_cmd)
                    {
                        val = "all";
                    }
                    else
                    {
                        var result = db.Gtogglechannels.Where(p => p.CommandName.Equals(cmd) && p.ChannelId.Equals(context.Channel.Id.ToLong()) && p.GuildId.Equals(context.Guild.Id.ToLong()));
                        if(result.Any() || result.Count() > 0)
                        {
                            val = context.Channel.Name;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        private bool CheckInputData(string[] input)
        {
            string first_param = input[0];
            bool is_found = _service.Commands.Where(p => p.Name.Equals(first_param)).Any();
            bool is_group_found = _service.Modules.Where(p => p.Aliases.First().Equals(first_param)).Any();
            if (!is_found && !is_group_found)
                return true;
            return false;
        }

        private string GetHelpModule()
        {
            var modules_text = "";
            var modules = _service.Modules.Where(x => !x.Name.ICEquals("default")).GroupBy(p => p.Name).ToList();
            if (modules.Any())
            {
                foreach (var module in modules)
                {
                    string tmptxt = $"`` {module.First().Name} `` , ";
                    modules_text += tmptxt;
                }

                if (modules_text.TrimEnd().EndsWith(","))
                    modules_text = modules_text.Truncate(2);
            }
            return modules_text;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            string spec = null;
            string content = null;
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
           // Console.WriteLine("////////////////////////" + context.Channel.Name);
            if(msg.HasStringPrefix(spec, ref argPos))
            {
                content = ParseStringPrefix(msg, spec);
            }
            else if(msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                content = ParseMentionPrefix(msg);
            }
            if (msg.HasStringPrefix(spec, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                string[] words = content.Split(' ');
                string data = CheckAvaliableCommand(words, context);
                if (data != null)
                {
                    if (data.Equals("all"))
                    {
                        await context.Channel.SendMessageAsync($"📣 : You can't use it because ***{words[0]}*** command **is toggles off** in this guild !");
                        return;
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"📣 : You can't use it because ***{words[0]}*** command **is toggles off** in ``#{data}`` channel!");
                        return;
                    }
                }
                var result = await _service.ExecuteAsync(context, argPos, _map);
                if (!result.IsSuccess)
                {
                    var embed = new EmbedBuilder();
                    if (result.Error == CommandError.UnknownCommand)
                    {
                        string help = GetHelpModule();
                        embed.AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Help"), Value = help });
                        //await context.Channel.SendMessageAsync("Command not recognized");
                    }

                    if (result is ExecuteResult r)
                        PrettyConsole.NewLine(r.Exception.ToString());
                    else 
                        embed.WithColor(new Color(255, 0, 0)).WithTitle("**Error:**").WithDescription(result.ErrorReason);
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
        }
    }
}
