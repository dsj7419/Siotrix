using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
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
using Discord.Commands;

namespace Siotrix.Discord
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private int error = 0;

        // public CommandHandler(DiscordSocketClient client, DependencyMap map)
        public CommandHandler(IServiceProvider provider)
        {
         //   _client = client;
         //   _map = map;
         //   _map.Add(new InteractiveService(_client));
            _provider = provider;
            _client = _provider.GetService<DiscordSocketClient>();
            _commands = _provider.GetService<CommandService>();
        }

        public async Task StartAsync()
        {



            //_commands = new CommandService(new CommandServiceConfig()
            // {
            //     CaseSensitiveCommands = false,
            //     DefaultRunMode = RunMode.Async
            // });

      //      _service.AddTypeReader(typeof(Uri), new UriTypeReader());
      //      _service.AddTypeReader(typeof(TimeSpan), new TimeSpanTypeReader());
      //      await _service.AddModulesAsync(Assembly.GetEntryAssembly());

            _commands.AddTypeReader(typeof(Uri), new UriTypeReader());
            _commands.AddTypeReader(typeof(TimeSpan), new TimeSpanTypeReader());
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            await _commands.AddModulesAsync(typeof(Entity).GetTypeInfo().Assembly);

            var config = Configuration.Load();
            if (config.Modules.Admin)
                await _commands.LoadAdminAsync();
            if (config.Modules.Audio)
                await _commands.LoadAudioAsync();
            if (config.Modules.Events)
                await _commands.LoadEventsAsync();
            if (config.Modules.Utility)
                await _commands.LoadUtilityAsync();
            if (config.Modules.Developer)
                await _commands.LoadDeveloperAsync();
            if (config.Modules.Moderation)
                await _commands.LoadModerationAsync();
            if (config.Modules.Roslyn)
                await _commands.LoadRoslynAsync();
            if (config.Modules.Statistics)
                await _commands.LoadStatisticsAsync();

            _client.MessageReceived += HandleCommandAsync;
            await PrettyConsole.LogAsync("Info", "Commands", $"Loaded {_commands.Commands.Count()} commands");
        }

        public Task StopAsync()
        {
            // _service = null;
            _client.MessageReceived -= HandleCommandAsync;
            return Task.CompletedTask;
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
            bool is_found = _commands.Commands.Where(p => p.Name.Equals(first_param)).Any();
            bool is_group_found = _commands.Modules.Where(p => p.Aliases.First().Equals(first_param)).Any();
            if (!is_found && !is_group_found)
                return true;
            return false;
        }

        private string GetHelpModule()
        {
            var modules_text = "";
            var modules = _commands.Modules.Where(x => !x.Name.ICEquals("default")).GroupBy(p => p.Name).ToList();
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

            var isMod = _commands.Modules.Any(x => x.Name.ICEquals(predicate));
            var isCommand = _commands.Commands.Any(x => x.Name.ICEquals(predicate));

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
                var command = _commands.Commands.Where(x => x.Name.ICEquals(predicate)).FirstOrDefault();
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
                var list = _commands.Modules.Where(p => p.Aliases.First().Equals(predicate));
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
                    result = "This is a module Name. Please try an alternate command!";
                    break;
                case 3:
                    result = "This command needs some additional parameters.";
                    break;
                case 4:
                    result = "This is a group command that requires additional sub-commands.";
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
            
            spec = PrefixExtensions.GetGuildPrefix(context);

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
                content = MessageParser.ParseStringPrefix(msg, spec);
            }
            else if(msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                content = MessageParser.ParseMentionPrefix(msg);
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
                
                var result = await _commands.ExecuteAsync(context, argPos, _provider);
                if (result.IsSuccess)
                {
                    ActionResult.IsSuccess = true;
                }
                else
                {
                    ActionResult.IsSuccess = false;
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
