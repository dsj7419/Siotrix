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
        private int error = 0;

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

        public EmbedBuilder GetCommandHelp(string predicate, EmbedBuilder builder, string prefix)
        {
            string commands = null;
            string sub_commands = null;
            string summary = null;
            string remark = null;
            bool has_group = false;
            string element_summary_remark_list = null;
            string buffer_data = "";
            int element_index = 0;
            bool exist_group = false;

            var isMod = _service.Modules.Any(x => x.Name.ICEquals(predicate));
            var isCommand = _service.Commands.Any(x => x.Name.ICEquals(predicate));

            if (isMod && !isCommand)
            {
                commands = GetHelpModule();
                builder
                    .AddField(x =>
                    {
                        x.Name = Format.Underline("Help");
                        x.Value = $"{commands}";
                    });
                error = 2;
            }
            else if (!isMod && isCommand)
            {
                System.Console.WriteLine(">>>>>>>>>>{0}======={1}", isMod, isCommand);
                var command = _service.Commands.Where(x => x.Name.ICEquals(predicate)).FirstOrDefault();
                has_group = command.Module.Aliases.First().Any();
                string group_name = command.Module.Aliases.First() + " ";
                var list = command.Module.Commands.Where(p => p.Name.Equals(predicate)).ToList();
                if (list.Count > 1)
                {
                    foreach (var ele in list)
                    {
                        element_index++;
                        string cmd_name = ele.Name + " ";
                        if (!has_group)
                        {
                            element_summary_remark_list += $"[Option - {element_index}] " + $"**{ele.Summary}**\n" + $"```Usage : {prefix}{cmd_name}{ele.Remarks}```\n";
                        }
                        else
                        {
                            element_summary_remark_list += $"[Option - {element_index}] " + $"**{ele.Summary}**\n" + $"```Usage : {prefix}{group_name}{cmd_name}{ele.Remarks}```\n";
                        }

                    }
                    builder
                    .AddField(x =>
                    {
                        x.Name = Format.Underline($"{command.Name}");
                        x.Value = $"{element_summary_remark_list}";
                    });
                }
                else
                {
                    summary = $"**{command.Summary}**";
                    if (!has_group)
                    {
                        remark = $"```Usage : {prefix}{command.Name}{command.Remarks}```";
                    }
                    else
                    {
                        remark = $"```Usage : {prefix}{group_name}{command.Name}{command.Remarks}```";
                    }
                    builder
                        .AddField(x =>
                        {
                            x.Name = Format.Underline($"{command.Name}");
                            x.Value = $"{summary}\n" + $"{remark}";
                        });
                }
                error = 3;
            }
            else // isMod = false && isCommand = false
            {
                var list = _service.Modules.Where(p => p.Aliases.First().Equals(predicate));
                if (list.Any())
                {
                    foreach(var sub_command in list.First().Commands)
                    {
                        if (!buffer_data.Equals(sub_command.Name))
                        {
                            buffer_data = sub_command.Name;
                            sub_commands += $"``{buffer_data}``" + " , ";
                        }
                    }
                    error = 4;
                }
                else
                {
                    predicate = "Help";
                    sub_commands = GetHelpModule();
                    error = 1;
                }
                if (sub_commands.TrimEnd().EndsWith(","))
                    sub_commands = sub_commands.Truncate(2);
                builder
                  .AddField(x =>
                  {
                      x.Name = Format.Underline($"{predicate}");
                      x.Value = $"{sub_commands}";
                  });
            }
            return builder;
        }

        private string GetReasonResult(int err)
        {
            string result = null;
            switch (err)
            {
                case 1:
                    result = "Unknown Command";
                    break;
                case 2:
                    result = "This is Module Name. Please Input again!";
                    break;
                case 3:
                    result = "This command need some parameters.";
                    break;
                case 4:
                    result = "You can use together with sub-commands because this is group command.";
                    break;
                default:
                    break;
            }
            return result;
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
                        await context.Channel.SendMessageAsync($"📣 : Unable to use: ***{words[0]}*** command **is toggled off** in this guild !");
                        return;
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"📣 : Unable to use: ***{words[0]}*** command **is toggled off** in ``#{data}`` channel!");
                        return;
                    }
                }
                var result = await _service.ExecuteAsync(context, argPos, _map);
                if (!result.IsSuccess)
                {
                    var embed = new EmbedBuilder();
                    GetCommandHelp(words[0], embed, spec);
                    string reason = GetReasonResult(error);
                    if (result is ExecuteResult r)
                        PrettyConsole.NewLine(r.Exception.ToString());
                    else 
                        embed.WithColor(new Color(255, 0, 0)).WithTitle("**Error:**").WithDescription(reason);
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
        }
    }
}
