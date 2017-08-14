using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Siotrix.Discord.Admin;
using Siotrix.Discord.Audio;
using Siotrix.Discord.Developer;
using Siotrix.Discord.Events;
using Siotrix.Discord.Moderation;
using Siotrix.Discord.Readers;
using Siotrix.Discord.Roslyn;
using Siotrix.Discord.Statistics;
using Siotrix.Discord.Utility;

namespace Siotrix.Discord
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private int _error;

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
            _commands.AddTypeReader(typeof(IRole), new RoleTypeReader<IRole>());
            _commands.AddTypeReader(typeof(IUser), new UserTypeReader<IUser>());
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
            var cmd = words[0];
            string val = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var isFoundCmd = db.Gtoggles
                        .Where(p => p.CommandName.Equals(cmd) && p.GuildId.Equals(context.Guild.Id.ToLong())).Any();
                    if (isFoundCmd)
                    {
                        val = "all";
                    }
                    else
                    {
                        var result =
                            db.Gtogglechannels.Where(p => p.CommandName.Equals(cmd) &&
                                                          p.ChannelId.Equals(context.Channel.Id.ToLong()) &&
                                                          p.GuildId.Equals(context.Guild.Id.ToLong()));
                        if (result.Any() || result.Count() > 0)
                            val = context.Channel.Name;
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
            var firstParam = input[0];
            var isFound = _commands.Commands.Where(p => p.Name.Equals(firstParam)).Any();
            var isGroupFound = _commands.Modules.Where(p => p.Aliases.First().Equals(firstParam)).Any();
            if (!isFound && !isGroupFound)
                return true;
            return false;
        }

        private string GetHelpModule()
        {
            var modulesText = "";
            var modules = _commands.Modules.Where(x => !x.Name.IcEquals("default")).GroupBy(p => p.Name).ToList();
            if (modules.Any())
            {
                foreach (var module in modules)
                {
                    var tmptxt = $"`` {module.First().Name} `` , ";
                    modulesText += tmptxt;
                }

                if (modulesText.TrimEnd().EndsWith(","))
                    modulesText = modulesText.Truncate(2);
            }
            return modulesText;
        }

        public EmbedBuilder GetCommandHelp(string predicate, EmbedBuilder builder, string prefix)
        {
            string commands = null;
            string subCommands = null;
            string summary = null;
            string remark = null;
            var hasGroup = false;
            string elementSummaryRemarkList = null;
            var bufferData = "";
            var elementIndex = 0;

            var isMod = _commands.Modules.Any(x => x.Name.IcEquals(predicate));
            var isCommand = _commands.Commands.Any(x => x.Name.IcEquals(predicate));

            if (isMod && !isCommand)
            {
                commands = GetHelpModule();
                builder
                    .AddField(x =>
                    {
                        x.Name = Format.Underline("Help");
                        x.Value = $"{commands}";
                    });
                _error = 2;
            }
            else if (!isMod && isCommand)
            {
                Console.WriteLine(">>>>>>>>>>{0}======={1}", isMod, isCommand);
                var command = _commands.Commands.Where(x => x.Name.IcEquals(predicate)).FirstOrDefault();
                hasGroup = command.Module.Aliases.First().Any();
                var groupName = command.Module.Aliases.First() + " ";
                var list = command.Module.Commands.Where(p => p.Name.Equals(predicate)).ToList();
                if (list.Count > 1)
                {
                    foreach (var ele in list)
                    {
                        elementIndex++;
                        var cmdName = ele.Name + " ";
                        if (!hasGroup)
                            elementSummaryRemarkList += $"[Option - {elementIndex}] " + $"**{ele.Summary}**\n" +
                                                           $"```Usage : {prefix}{cmdName} {ele.Remarks}```\n";
                        else
                            elementSummaryRemarkList += $"[Option - {elementIndex}] " + $"**{ele.Summary}**\n" +
                                                           $"```Usage : {prefix}{groupName}{cmdName} {ele.Remarks}```\n";
                    }
                    builder
                        .AddField(x =>
                        {
                            x.Name = Format.Underline($"{command.Name}");
                            x.Value = $"{elementSummaryRemarkList}";
                        });
                }
                else
                {
                    summary = $"**{command.Summary}**";
                    if (!hasGroup)
                        remark = $"```Usage : {prefix}{command.Name} {command.Remarks}```";
                    else
                        remark = $"```Usage : {prefix}{groupName}{command.Name} {command.Remarks}```";
                    builder
                        .AddField(x =>
                        {
                            x.Name = Format.Underline($"{command.Name}");
                            x.Value = $"{summary}\n" + $"{remark}";
                        });
                }
                _error = 3;
            }
            else // isMod = false && isCommand = false
            {
                var list = _commands.Modules.Where(p => p.Aliases.First().Equals(predicate));
                if (list.Any())
                {
                    foreach (var subCommand in list.First().Commands)
                        if (!bufferData.Equals(subCommand.Name))
                        {
                            bufferData = subCommand.Name;
                            subCommands += $"``{bufferData}``" + " , ";
                        }
                    _error = 4;
                }
                else
                {
                    predicate = "Help";
                    subCommands = GetHelpModule();
                    _error = 1;
                }
                if (subCommands.TrimEnd().EndsWith(","))
                    subCommands = subCommands.Truncate(2);
                builder
                    .AddField(x =>
                    {
                        x.Name = Format.Underline($"{predicate}");
                        x.Value = $"{subCommands}";
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
            var user = msg.Author as SocketGuildUser;
            var val = await context.GetGuildPrefixAsync();
            var argPos = 0;
            string spec = null;
            string content = null;

            spec = val.Prefix;
            var blacklistedUser = await BlacklistExtensions.GetBlacklistAsync(context.Guild.Id, user.Id);
            if (blacklistedUser != null) return;

            if (s.Author.IsBot
                || msg == null
                || !msg.Content.Except("?").Any()
                || msg.Content.Trim().Length <= 1
                || msg.Content.Trim()[1] == '?'
                || !(msg.HasStringPrefix(spec, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;
            // Console.WriteLine("////////////////////////" + context.Channel.Name);
            if (msg.HasStringPrefix(spec, ref argPos))
                content = MessageParser.ParseStringPrefix(msg, spec);
            else if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                content = MessageParser.ParseMentionPrefix(msg);
            if (msg.HasStringPrefix(spec, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var words = content.Split(' ');
                var data = CheckAvaliableCommand(words, context);
                if (data != null)
                    if (data.Equals("all"))
                    {
                        await context.Channel.SendMessageAsync(
                            $"📣 : Unable to use: ***{words[0]}*** command **is toggled off** in this guild !");
                        return;
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(
                            $"📣 : Unable to use: ***{words[0]}*** command **is toggled off** in ``#{data}`` channel!");
                        return;
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
                    var reason = GetReasonResult(_error);
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