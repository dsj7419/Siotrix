using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Siotrix.Discord.Attributes.Preconditions;

namespace Siotrix.Discord.Utility
{    
    [Name("Utility")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.User)]
    public class MyHelpModule : ModuleBase<SocketCommandContext>
    {

        private readonly CommandService _service;
        private readonly IDependencyMap _map;

        public MyHelpModule(CommandService service, IDependencyMap map)
        {
            _service = service;
            _map = map;
        }

        private string GetModuleNames()
        {
            var modules_text = "";
            var modules = _service.Modules.Where(x => !x.Name.ICEquals("default") && !x.Name.ICEquals("no-help")).GroupBy(p => p.Name).ToList();
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

        private string GetGuildIconUrl()
        {
            var guild_id = Context.Guild.Id;
            string iconurl = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gavatars.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        iconurl = db.Authors.First().AuthorIcon;
                    }
                    else
                    {
                        iconurl = val.First().Avatar;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return iconurl;
        }

        private string GetGuildName()
        {
            var guild_id = Context.Guild.Id;
            string name = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gnames.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        name = Context.Guild.Name;
                    }
                    else
                    {
                        name = val.First().GuildName;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return name;
        }

        private string GetGuildUrl()
        {
            var guild_id = Context.Guild.Id;
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gwebsiteurls.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        url = db.Authors.First().AuthorUrl;
                    }
                    else
                    {
                        url = val.First().SiteUrl;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return url;
        }

        private Color GetGuildColor()
        {
            var guild_id = Context.Guild.Id;
            int id = 0;
            byte rColor = 0;
            byte gColor = 0;
            byte bColor = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gcolors.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        id = 15;
                    }
                    else
                    {
                        id = val.First().ColorId;
                    }
                    var col_value = db.Colorinfos.Where(y => y.Id == id).First();
                    rColor = Convert.ToByte(col_value.RedParam);
                    gColor = Convert.ToByte(col_value.GreenParam);
                    bColor = Convert.ToByte(col_value.BlueParam);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return new Color(rColor, gColor, bColor);
        }

        private string GetGuildThumbNail()
        {
            var guild_id = Context.Guild.Id;
            string thumbnail_url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gthumbnails.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        thumbnail_url = "http://img04.imgland.net/WyZ5FoM.png";
                    }
                    else
                    {
                        thumbnail_url = val.First().ThumbNail;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return thumbnail_url;
        }

        private string[] GetGuildFooter()
        {
            var guild_id = Context.Guild.Id;
            string[] footer = new string[2];
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gfooters.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        footer[0] = db.Bfooters.First().FooterIcon;
                        footer[1] = db.Bfooters.First().FooterText;
                    }
                    else
                    {
                        footer[0] = val.First().FooterIcon;
                        footer[1] = val.First().FooterText;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return footer;
        }

        private string GetGuildPrefix()
        {
            var guild_id = Context.Guild.Id;
            string prefix = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gprefixs.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        prefix = "!";
                    }
                    else
                    {
                        prefix = val.First().Prefix;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return prefix;
        }

        [Command("myhelp")]
        [Summary("Shows info about commands")]
        [Remarks("help [module] [command]")]
        public async Task Help()
        {
            string g_icon_url = GetGuildIconUrl();
            string g_name = GetGuildName();
            string g_url = GetGuildUrl();
            Color g_color = GetGuildColor();
            string g_thumbnail = GetGuildThumbNail();
            string module_list = GetModuleNames();
            string[] g_footer = GetGuildFooter();

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

        [Name("no-help")]
        [Command("myhelp")]
        public async Task Help(string predicate)
        {
            string g_icon_url = GetGuildIconUrl();
            string g_name = GetGuildName();
            string g_url = GetGuildUrl();
            Color g_color = GetGuildColor();
            string g_thumbnail = GetGuildThumbNail();
            string module_list = GetModuleNames();
            string[] g_footer = GetGuildFooter();
            string group_commands = null;
            string commands = null;
            string g_prefix = GetGuildPrefix();
            string command_name = null;
            string sub_commands = null;
            string summary = null;
            string remark = null;
            bool has_group = false;

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
                System.Console.WriteLine(">>>>>>>>>>{0}======={1}", isMod, isCommand);
                var modules = _service.Modules.Where(x => x.Name.ICEquals(predicate));
                foreach (var module in modules)
                {
                    bool is_group = module.Aliases.First().Any();
                    if (is_group)
                    {
                        group_commands += $"**• {module.Aliases.First()}**" + $" - {module.Summary}\n";
                    }
                    else
                    {
                        var cmds = module.Commands.Where(x => !x.Name.ICEquals("no-help"));
                        foreach (var cmd in cmds)
                        {
                            commands += $"**• {cmd.Name}**" + $" - {cmd.Summary}\n";
                        }
                    }

                }

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
                var command_list = _service.Commands.Where(x => x.Name.ICEquals(predicate));
                foreach(var command in command_list)
                {
                    command_name = command.Name;
                    foreach (var compare_module in _service.Modules)
                    {
                        has_group = compare_module.Aliases.First().Any();
                        if (has_group)
                        {
                            string group_name = compare_module.Aliases.First() + " ";
                            foreach (var compare_cmd in compare_module.Commands.Where(x => !x.Name.ICEquals("no-help")))
                            {
                                if (command_name.ICEquals(compare_cmd.Name))
                                {
                                    summary = $"**{command.Summary}**";
                                    remark = $"```Usage : {g_prefix}{group_name}{command_name}{command.Remarks}```";
                                }
                            }
                        }
                        else
                        {
                            foreach (var compare_cmd in compare_module.Commands)
                            {
                                if (command_name.ICEquals(compare_cmd.Name))
                                {
                                    summary = $"**{command.Summary}**";
                                    remark = $"```Usage : {g_prefix}{command_name}{command.Remarks}```";
                                }
                            }
                        }
                    }
                }

                builder
                    .AddField(x =>
                    {
                        x.Name = Format.Underline($"{command_name}");
                        x.Value = $"{summary}\n" + $"{remark}";
                    });
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
                            foreach (var sub_cmd in item.Commands.Where(x => !x.Name.ICEquals("no-help")))
                            {
                                sub_commands += sub_cmd.Name + " , ";
                            }
                        }
                    }
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