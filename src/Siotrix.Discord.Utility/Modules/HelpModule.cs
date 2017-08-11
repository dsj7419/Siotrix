using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Siotrix.Discord.Utility
{
    [Name("Utility")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.User)]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider _map;

        private readonly CommandService _service;

        public HelpModule(CommandService service, IServiceProvider map)
        {
            _service = service;
            _map = map;
        }

        private string GetModuleNames()
        {
            var modulesText = "";
            var modules = _service.Modules.Where(x => !x.Name.IcEquals("default")).GroupBy(p => p.Name).ToList();
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


        [Command("help")]
        [Summary("Shows a list of modules.")]
        [Remarks(" - no argument needed.")]
        public async Task Help()
        {
            var gIconUrl = Context.GetGuildIconUrl();
            var gName = Context.GetGuildName();
            var gUrl = Context.GetGuildUrl();
            var gColor = Context.GetGuildColor();
            var gThumbnail = Context.GetGuildThumbNail();
            var moduleList = GetModuleNames();
            var gFooter = Context.GetGuildFooter();

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithColor(gColor)
                .WithThumbnailUrl(gThumbnail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter[0])
                    .WithText(gFooter[1]))
                .WithTimestamp(DateTime.UtcNow);

            builder
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Modules"),
                    Value = moduleList
                });
            await ReplyAsync("", embed: builder);
        }

        [Command("help")]
        [Summary("Get help on a module or a specific command.")]
        [Remarks(" [module] or [command name]")]
        public async Task Help(string predicate)
        {
            var gIconUrl = Context.GetGuildIconUrl();
            var gName = Context.GetGuildName();
            var gUrl = Context.GetGuildUrl();
            var gColor = Context.GetGuildColor();
            var gThumbnail = Context.GetGuildThumbNail();
            var gFooter = Context.GetGuildFooter();
            string groupCommands = null;
            string commands = null;
            var gPrefix = Context.GetGuildPrefix();
            string subCommands = null;
            string summary = null;
            string remark = null;
            var hasGroup = false;
            string elementSummaryRemarkList = null;
            var bufferData = "";
            var elementIndex = 0;
            var unnameCommandIndex = 0;
            string unnameCommmandSummaryRemarkList = null;

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithColor(gColor)
                .WithThumbnailUrl(gThumbnail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter[0])
                    .WithText(gFooter[1]))
                .WithTimestamp(DateTime.UtcNow);

            var isMod = _service.Modules.Any(x => x.Name.IcEquals(predicate));
            var isCommand = _service.Commands.Any(x => x.Name.IcEquals(predicate));

            if (isMod && !isCommand)
            {
                var modules = _service.Modules.Where(x => x.Name.IcEquals(predicate));
                Console.WriteLine(">>>>>>>>>>{0}======={1}------{2}", isMod, isCommand, modules.Count());
                foreach (var module in modules)
                {
                    var isGroup = module.Aliases.First().Any();
                    if (isGroup)
                        groupCommands += $"**• {module.Aliases.First()}**" + $" - {module.Summary}\n";
                    else
                        foreach (var cmd in module.Commands.GroupBy(p => p.Name))
                            commands += $"`` {cmd.First().Name} ``" + " , ";
                }

                if (commands.TrimEnd().EndsWith(","))
                    commands = commands.Truncate(2);

                if (groupCommands != null)
                    builder
                        .AddField(x =>
                        {
                            x.Name = Format.Underline("- Group Commands :");
                            x.Value = $"{groupCommands}";
                        });
                builder
                    .AddField(x =>
                    {
                        x.Name = Format.Underline("- Commands :");
                        x.Value = $"{commands}";
                    });
            }
            else if (!isMod && isCommand)
            {
                Console.WriteLine(">>>>>>>>>>{0}======={1}", isMod, isCommand);
                var command = _service.Commands.Where(x => x.Name.IcEquals(predicate)).FirstOrDefault();
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
                                                           $"```Usage : {gPrefix}{cmdName}{ele.Remarks}```\n";
                        else
                            elementSummaryRemarkList += $"[Option - {elementIndex}] " + $"**{ele.Summary}**\n" +
                                                           $"```Usage : {gPrefix}{groupName}{cmdName}{ele.Remarks}```\n";
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
                        remark = $"```Usage : {gPrefix}{command.Name} {command.Remarks}```";
                    else
                        remark = $"```Usage : {gPrefix}{groupName}{command.Name} {command.Remarks}```";
                    builder
                        .AddField(x =>
                        {
                            x.Name = Format.Underline($"{command.Name}");
                            x.Value = $"{summary}\n" + $"{remark}";
                        });
                }
            }
            else // isMod = false && isCommand = false
            {
                foreach (var item in _service.Modules)
                {
                    var existGroup = item.Aliases.First().Any();
                    if (!existGroup)
                    {
                    }
                    else
                    {
                        if (predicate.IcEquals(item.Aliases.First()))
                            foreach (var subCmd in item.Commands)
                                //Console.WriteLine("======{0}", sub_cmd.Name);
                                if (!bufferData.Equals(subCmd.Name))
                                {
                                    if (subCmd.Name.Length > 5)
                                    {
                                        var reverseCommandName =
                                            new string(subCmd.Name.ToArray().Reverse().ToArray());
                                        if (reverseCommandName.Substring(0, 5).Equals("cnysA"))
                                        {
                                            //Console.WriteLine("~~~~~~~~{0}", sub_cmd.Aliases.First().ToString());
                                            unnameCommandIndex++;
                                            unnameCommmandSummaryRemarkList +=
                                                $"[Option - {unnameCommandIndex}] " + $"**{subCmd.Summary}**\n" +
                                                $"```Usage : {gPrefix}{subCmd.Aliases.First()} {subCmd.Remarks}```\n";
                                            continue;
                                        }
                                    }
                                    bufferData = subCmd.Name;
                                    subCommands += $"``{bufferData}``" + " , ";
                                }
                    }
                }

                if (subCommands == null)
                {
                    if (unnameCommmandSummaryRemarkList != null)
                        subCommands = unnameCommmandSummaryRemarkList;
                    else
                        subCommands = "No help data!";
                }
                else
                {
                    if (subCommands.TrimEnd().EndsWith(","))
                        subCommands = subCommands.Truncate(2);
                    if (unnameCommmandSummaryRemarkList != null)
                        subCommands += "\n" + unnameCommmandSummaryRemarkList;
                }

                builder
                    .AddField(x =>
                    {
                        x.Name = Format.Underline($"{predicate}");
                        x.Value = $"{subCommands}";
                    });
            }
            await ReplyAsync("", embed: builder);
        }
    }
}