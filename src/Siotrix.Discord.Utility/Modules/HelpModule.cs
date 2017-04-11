using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Siotrix.Discord.Utility
{    
    [Name("Utility")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.User)]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {

        private readonly CommandService _service;
        private readonly IDependencyMap _map;

        public HelpModule(CommandService service, IDependencyMap map)
        {
            _service = service;
            _map = map;
        }

        [Command("help")]
        [Summary("Shows info about commands")]
        [Remarks("help [module] [command]")]
        public async Task Help()
        {            
            var sender = Context.Message.Author as SocketGuildUser;
            var headerTxt = $"**Usable commands for {sender?.Username} in {Context.Guild.Name}**";
            var commandTxt = "";
            var modulesTxt = "";

            var defModule =
                _service.Modules.FirstOrDefault(x =>
                   string.Equals(x.Name, "default", StringComparison.CurrentCultureIgnoreCase));

            if (defModule != null)
            {
                foreach (var command in defModule.Commands.Where(x => !x.Name.ICEquals("no-help")))
                {
                    var result =
                        await command.CheckPreconditionsAsync(Context, _map);
                    if (result.IsSuccess)
                        commandTxt += $"{command.Name}, ";

                }
            }

            if (string.IsNullOrEmpty(commandTxt))
                commandTxt =
                    $"No default commands found.";

            else if (commandTxt.TrimEnd().EndsWith(","))
                commandTxt =
                    commandTxt.Truncate(2);

            var modules = _service.Modules.Where(x =>
                !x.Name.ICEquals("default")).ToArray();

            if (modules.Any())
            {
                foreach (var module in modules)
                {
                    var modAlias = module.Aliases.First();
                    if (!string.IsNullOrEmpty(modAlias)
                        && !module.Name.ICEquals("no-help")
                        && module.Commands.Any()
                        )
                    {
                        string tmptxt = $"\n``{module.Name}``: ";
                        foreach (var command in module.Commands.Where(x => !x.Name.ICEquals("no-help")))
                        {
                            var result =
                                await command.CheckPreconditionsAsync(Context, _map);
                            if (result.IsSuccess)
                                tmptxt += $"{command.Name}, ";
                        }
                        if (!string.IsNullOrEmpty(tmptxt.Split(':')[1].Trim()))
                            modulesTxt += tmptxt;
                    }

                    if (modulesTxt.TrimEnd().EndsWith(","))
                        modulesTxt =
                            modulesTxt.Truncate(2);
                }
            }

            if (string.IsNullOrEmpty(commandTxt + modulesTxt))
            {
                await ReplyAsync($"No commands found.");
                return;
            }

            await ReplyAsync($"{headerTxt}\n" +
                            $"{commandTxt}\n" +
                            $"\n" +
                            $"**Modules**" +
                            $"{modulesTxt}\n\n" +
                            $"Get command specific help: `(prefixNEEDSTOBEIMPLEMENTED)help <module> <command>`");
        }

        [Name("no-help")]
        [Command("help")]
        public async Task Help(string predicate)
        {
            predicate = predicate.RemoveWhitespace().ToLower();
            var sender = Context.Message.Author as SocketGuildUser ?? Context.Message.Author;
            var headerTxt = $"**Usable commands for {sender.Username}**";
            var modPrefix = "module:";
            var startsWithModPrefix = predicate.StartsWith(modPrefix);
            var isMod = _service.Modules.Any(x => x.Name.ICEquals(predicate));
            var isCommand = _service.Commands.Any(x => x.Name.ICEquals(predicate));
            if (startsWithModPrefix || (isMod && !isCommand))
            {
                if (startsWithModPrefix)
                    predicate = predicate.Substring(modPrefix.Length);
                headerTxt += $" **in module {predicate}**";
                var module = _service.Modules.FirstOrDefault(x =>
                    x.Aliases.First().ICEquals(predicate));

                if (module == null)
                {
                    await ReplyAsync($"Module `{predicate}` not found".Cap(2000));
                    return;
                }

                var commands =
                    module.Commands
                        .Where(x =>
                            !x.Name.ICEquals("no-help"));

                var usableCommands = new List<CommandInfo>();
                var commandInfos = commands as CommandInfo[] ?? commands.ToArray();
                commandInfos.ToList().ForEach(async x =>
                {
                    var b = await x.CheckPreconditionsAsync(Context, _map);
                    if (b.IsSuccess)
                        usableCommands.Add(x);
                });

                await ReplyAsync(usableCommands.Any() ?
                            ($"{headerTxt}\n" +
                            usableCommands.Select(x => $"{x.Name}").PrettyPrint()).Cap(2000)
                            : $"No usable commands found.");

            }
            else
            {
                modPrefix = "command:";
                if (predicate.StartsWith(modPrefix))
                    predicate = predicate.Substring(modPrefix.Length);

                var command =
                    _service.Commands
                        .FirstOrDefault(x =>
                            x.Aliases.Contains(predicate)
                            && !x.Name.ICEquals("no-help"));

                if (command == null)
                {
                    await ReplyAsync($"Command `{predicate}` not found".Cap(2000));
                    return;
                }

                var checkPreconditionsAsync =
                    command.CheckPreconditionsAsync(Context, _map);

                if (checkPreconditionsAsync != null)
                {
                    var result =
                        await checkPreconditionsAsync;

                    var aliases =
                        command.Aliases
                            .Skip(1)
                            .ToArray();

                    var split = command.Remarks.Split('\n') ?? new string[0];

                    var usage =
                        split.Length > 1
                            ? $"`{split[0]}`\n**Example**: `{split[1]}`"
                            : $"`{command.Remarks}`";

                    await
                        ReplyAsync(
                            $"**Module **: `{command.Module.Name}`\n" +
                            $"**Command**: `{command.Aliases.First()}`" +
                            $"{(aliases.Any() ? $"\n**Aliases**: {aliases.PrettyPrint()}" : "")}\n" +
                            $"{(command.Summary?.Length > 0 ? $"**Summary**: `{command.Summary}`\n" : "")}" +
                            $"{(command.Remarks?.Length > 0 ? $"**Usage**: {usage}\n" : "")}" +
                            $"**Usable by {sender?.Username}**: `{(result.IsSuccess ? "yes" : "no")}`");
                }
                else
                    await ReplyAsync($"No command found");
            }

        }

        [Name("no-help")]
        [Command("help")]
        public async Task Help(string module, [Remainder] string predicate)
        {
            var sender = Context.Message.Author as SocketGuildUser;
            const string modPrefix = "module:";
            const string cmdPrefix = "command:";

            module = module.ToLower();
            predicate = predicate.RemoveWhitespace().ToLower();

            if (module.StartsWith(cmdPrefix))
            {
                var tmp = module;
                module = predicate;
                predicate = tmp;
            }
            else if (predicate.StartsWith(modPrefix))
            {
                var tmp = predicate;
                predicate = module;
                module = tmp;
            }

            var split = module.Split(':');
            if (split.Length > 1)
                module = split[1];
            split = predicate.Split(':');
            if (split.Length > 1)
                predicate = split[1];

            var command =
                _service.Modules
                    .FirstOrDefault(x =>
                        x.Name.ICEquals(module))
                    ?.Commands
                    .FirstOrDefault(x =>
                        x.Aliases.Contains($"{module} {predicate}")
                        && !x.Name.ICEquals("no-help"));

            if (command != null)
            {
                var result =
                    await command.CheckPreconditionsAsync(Context, _map);

                var aliases =
                    command.Aliases
                        .Skip(1)
                        .ToArray();

                split =
                    command.Remarks?.Split('\n') ?? new string[0];

                var usage =
                    split.Length > 1
                        ? $"`{split[0]}`\n**Example**: `{split[1]}`"
                        : $"`{command.Remarks}`";

                await
                    ReplyAsync(
                        $"**Module **: `{command.Module.Name}`\n" +
                        $"**Command**: `{command.Name}`" +
                        $"{(aliases.Any() ? $"\n**Aliases**: {aliases.PrettyPrint()}" : "")}\n" +
                        $"{(command.Summary?.Length > 0 ? $"**Summary**: `{command.Summary}`\n" : "")}" +
                        $"{(command.Remarks?.Length > 0 ? $"**Usage**: {usage}\n" : "")}" +
                        $"**Usable by {sender?.Username}**: `{(result.IsSuccess ? "yes" : "no")}`");
            }
            else
                await ReplyAsync($"Command `{predicate}` in module `{module}` not found");
        }
    }
}