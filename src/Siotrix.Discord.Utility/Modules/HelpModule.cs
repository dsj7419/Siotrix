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
        private readonly IServiceProvider _map;

        public HelpModule(CommandService service, IServiceProvider map)
        {
            _service = service;
            _map = map;
        }

        private string GetModuleNames()
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


        [Command("help")]
        [Summary("Shows a list of modules.")]  
        [Remarks(" - no argument needed.")]
        public async Task Help()
        {
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string module_list = GetModuleNames();
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithColor(g_color)
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);

            builder
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Modules"), Value = module_list });
            await ReplyAsync("", embed: builder);
        }

        [Command("help")]
        [Summary("Get help on a module or a specific command.")]
        [Remarks(" [module] or [command name]")]
        public async Task Help(string predicate)
        {
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
            string group_commands = null;
            string commands = null;
            string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
            string sub_commands = null;
            string summary = null;
            string remark = null;
            bool has_group = false;
            string element_summary_remark_list = null;
            string buffer_data = "";
            int element_index = 0;
            int unname_command_index = 0;
            string unname_commmand_summary_remark_list = null;

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithColor(g_color)
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);

            var isMod = _service.Modules.Any(x => x.Name.ICEquals(predicate));
            var isCommand = _service.Commands.Any(x => x.Name.ICEquals(predicate));

            if(isMod && !isCommand)
            {
                var modules = _service.Modules.Where(x => x.Name.ICEquals(predicate));
                System.Console.WriteLine(">>>>>>>>>>{0}======={1}------{2}", isMod, isCommand, modules.Count());
                foreach (var module in modules)
                {
                    bool is_group = module.Aliases.First().Any();
                    if (is_group)
                    {
                        group_commands += $"**• {module.Aliases.First()}**" + $" - {module.Summary}\n";
                    }
                    else
                    {
                        foreach (var cmd in module.Commands.GroupBy(p => p.Name))
                        {
                            commands += $"`` {cmd.First().Name} ``" + " , ";
                        }
                    }
                }

                if (commands.TrimEnd().EndsWith(","))
                    commands = commands.Truncate(2);

                if (group_commands != null)
                {
                    builder
                    .AddField(x =>
                    {
                        x.Name = Format.Underline("- Group Commands :");
                        x.Value = $"{group_commands}";
                    });
                }
                builder
                    .AddField(x =>
                    {
                        x.Name = Format.Underline("- Commands :");
                        x.Value = $"{commands}";
                    });
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
                    foreach(var ele in list)
                    {
                        element_index++;
                        string cmd_name = ele.Name + " ";
                        if (!has_group)
                        {
                            element_summary_remark_list += $"[Option - {element_index}] " + $"**{ele.Summary}**\n" + $"```Usage : {g_prefix}{cmd_name}{ele.Remarks}```\n";
                        }
                        else
                        {
                            element_summary_remark_list += $"[Option - {element_index}] " + $"**{ele.Summary}**\n" + $"```Usage : {g_prefix}{group_name}{cmd_name}{ele.Remarks}```\n";
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
                        remark = $"```Usage : {g_prefix}{command.Name}{command.Remarks}```";
                    }
                    else
                    {
                        remark = $"```Usage : {g_prefix}{group_name}{command.Name}{command.Remarks}```";
                    }
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
                foreach(var item in _service.Modules)
                {
                    bool exist_group = item.Aliases.First().Any();
                    if (!exist_group)
                    {
                        continue;
                    }
                    else
                    {
                        if (predicate.ICEquals(item.Aliases.First()))
                        {
                            foreach (var sub_cmd in item.Commands)
                            {
                                //Console.WriteLine("======{0}", sub_cmd.Name);
                                if (!buffer_data.Equals(sub_cmd.Name))
                                {
                                    if(sub_cmd.Name.Length > 5)
                                    {
                                        var reverse_command_name = new string(sub_cmd.Name.ToArray().Reverse().ToArray());
                                        if (reverse_command_name.Substring(0, 5).Equals("cnysA"))
                                        {
                                            //Console.WriteLine("~~~~~~~~{0}", sub_cmd.Aliases.First().ToString());
                                                unname_command_index++;
                                                unname_commmand_summary_remark_list += $"[Option - {unname_command_index}] " + $"**{sub_cmd.Summary}**\n" + $"```Usage : {g_prefix}{sub_cmd.Aliases.First()} {sub_cmd.Remarks}```\n";
                                            continue;
                                        }
                                            
                                    }
                                    buffer_data = sub_cmd.Name;
                                    sub_commands += $"``{buffer_data}``" + " , ";
                                }
                            }
                        }
                    }
                }

                if(sub_commands == null)
                {
                    if (unname_commmand_summary_remark_list != null)
                        sub_commands = unname_commmand_summary_remark_list;
                    else
                        sub_commands = "No help data!";
                }
                else
                {
                    if (sub_commands.TrimEnd().EndsWith(","))
                        sub_commands = sub_commands.Truncate(2);
                    if (unname_commmand_summary_remark_list != null)
                        sub_commands += "\n" + unname_commmand_summary_remark_list;
                }

                builder
                  .AddField(x =>
                  {
                      x.Name = Format.Underline($"{predicate}");
                      x.Value = $"{sub_commands}";
                  });
            }
            await ReplyAsync("", embed: builder);
        }
    }
}