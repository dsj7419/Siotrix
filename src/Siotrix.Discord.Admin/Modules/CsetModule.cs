using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildAdmin)]
    public class CsetModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IDependencyMap _map;

        public CsetModule(CommandService service, IDependencyMap map)
        {
            _service = service;
            _map = map;
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

        [Command("cset")]
        public async Task Cset(string module_name)
        {
            string g_icon_url = GetGuildIconUrl();
            string g_name = GetGuildName();
            string g_url = GetGuildUrl();
            Color g_color = GetGuildColor();
            string g_thumbnail = GetGuildThumbNail();
            string[] g_footer = GetGuildFooter();
            string group_commands = null;
            string commands = null;
            string g_prefix = GetGuildPrefix();

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

            var isMod = _service.Modules.Any(x => x.Name.ICEquals(module_name));
            var isCommand = _service.Commands.Any(x => x.Name.ICEquals(module_name));
            if (isMod && !isCommand)
            {
                var modules = _service.Modules.Where(x => x.Name.ICEquals(module_name));
                foreach (var module in modules)
                {
                    bool is_group = module.Aliases.First().Any();
                    if (is_group)
                        group_commands += $"`` {module.Aliases.First()} ``" + ", ";
                    else
                    {
                        foreach (var cmd in module.Commands)
                        {
                            commands += $"`` {cmd.Name} ``" + ", ";
                        }
                    }
                }
                if (group_commands.TrimEnd().EndsWith(","))
                    group_commands = group_commands.Truncate(2);
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
            await ReplyAsync("", embed: builder);
        }

        [Command("cset")]
        public async Task Cset(string command, [Remainder]string str)
        {
            string g_icon_url = GetGuildIconUrl();
            string g_name = GetGuildName();
            string g_url = GetGuildUrl();
            Color g_color = GetGuildColor();
            string g_thumbnail = GetGuildThumbNail();
            string[] g_footer = GetGuildFooter();
            string g_prefix = GetGuildPrefix();

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
            bool is_found = _service.Commands.Where(p => p.Name.Equals(command)).Any();
            if (is_found)
            {
                string toggle = str.Substring(0, 6);
                if (toggle.Equals("toggle") && str.Length < 7)
                {
                    using (var db = new LogDatabase())
                    {
                        try
                        {
                            var val = new DiscordCset();
                            val.Name = command;
                            val.GuildId = Context.Guild.Id.ToLong();
                            val.ChannelId = 0;
                            val.Status = 1;
                            db.Gcsets.Add(val);
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    await ReplyAsync("👍");
                }
            }
        }

       /* [Command("cset")]
        public async Task Cset(string command, [Remainder]string toggle_channel, string channel_name)
        {
            string g_icon_url = GetGuildIconUrl();
            string g_name = GetGuildName();
            string g_url = GetGuildUrl();
            Color g_color = GetGuildColor();
            string g_thumbnail = GetGuildThumbNail();
            string[] g_footer = GetGuildFooter();
            string g_prefix = GetGuildPrefix();

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
            bool is_found = _service.Commands.Where(p => p.Name.Equals(command)).Any();
            if (is_found)
            {
                if (toggle_channel.Equals("togglechannel"))
                {
                    string name = "#" + Context.Message.Channel.Name;
                    if (channel_name.Equals(name))
                    {
                        using (var db = new LogDatabase())
                        {
                            try
                            {
                                var val = new DiscordCset();
                                val.Name = command;
                                val.GuildId = Context.Guild.Id.ToLong();
                                val.ChannelId = Context.Message.Channel.Id.ToLong();
                                val.Status = 2;
                                db.Gcsets.Add(val);
                                db.SaveChanges();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                        await ReplyAsync("👍");
                    }
                }
            }
        }*/
    }
}