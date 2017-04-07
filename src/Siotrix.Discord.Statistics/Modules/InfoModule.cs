using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Diagnostics;
using Siotrix.Discord.Attributes.Preconditions;
using System.Threading.Tasks;

namespace Siotrix.Discord.Statistics
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private Process _process;

        protected override void BeforeExecute()
        {
            _process = Process.GetCurrentProcess();
        }

        private string GetAuthorIconUrl()
        {
            string iconurl = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    iconurl = db.Authors.First().AuthorIcon;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return iconurl;
        }

        private string GetGuildIconUrl(int id)
        {
            var guild_id = Context.Guild.Id;
            string iconurl = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gavatars.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0 || id == 2)
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

        private string GetGuildName(int id)
        {
            var guild_id = Context.Guild.Id;
            string name = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if(id == 0 || id == 1)
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
                    else
                    {
                        name = db.Authors.First().AuthorName;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return name;
        }

        private string GetGuildUrl(int id)
        {
            var guild_id = Context.Guild.Id;
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gwebsiteurls.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0 || id == 2)
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

        private string GetGuildDescription(int id)
        {
            var guild_id = Context.Guild.Id;
            string description = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if(id == 2)
                    {
                        description = db.Binfos.First().BotInfo;
                    }
                    else
                    {
                        var val = db.Gdescriptions.Where(p => p.GuildId == guild_id.ToLong());
                        if (val == null || val.ToList().Count <= 0)
                        {
                            description = "Siotrix Bot has been made by Dan Johnson and Frank Thomas.";
                        }
                        else
                        {
                            description = val.First().Description;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return description;
        }

        private string[] GetGuildFooter(int id)
        {
            var guild_id = Context.Guild.Id;
            string[] footer = new string[2];
            using (var db = new LogDatabase())
            {
                try
                {
                    if(id == 2)
                    {
                        footer[0] = db.Bfooters.First().FooterIcon;
                        footer[1] = db.Bfooters.First().FooterText;
                    }
                    else
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

        private bool CheckBot(SocketUser user)
        {
            if (user.IsBot)
            {
                return true;
            }
            return false;
        }

        private string GetUptime()
        {
            var uptime = (DateTime.Now - _process.StartTime);
            return $"{uptime.Days} day {uptime.Hours} hr {uptime.Minutes} min {uptime.Seconds} sec";
        }

        private string[] GetLifeTimeMessages(SocketUser user)
        {
            int cnt = 0;
            string[] arr = new string[2];
            DateTime today = DateTime.Now;
            using (var db = new LogDatabase())
            {
                try
                {
                    cnt = db.Messages.Where(p => !p.IsBot && p.AuthorId == user.Id.ToLong()).ToList().Count;
                    double lifeTime = (today - db.Messages.Where(p => p.AuthorId.Equals(user.Id.ToLong()) && !p.IsBot).Min(m => m.CreatedAt)).TotalHours;
                    arr[0] = cnt.ToString();
                    arr[1] = (Math.Round((cnt / lifeTime), 2)).ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return arr;
        }

        private string GetLastMessageTime(SocketUser user)
        {
            string last = "-";
            using (var db = new LogDatabase())
            {
                try
                {
                    if(db.Messages.Where(p => !p.IsBot && p.AuthorId == user.Id.ToLong()).ToList().Count > 0)
                    {
                        DateTime date = db.Messages.Where(p => !p.IsBot && p.AuthorId == user.Id.ToLong()).Last().CreatedAt;
                        last = String.Format("{0:dddd, MMMM d, yyyy}", date);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return last;
        }

        private string GetActiveGuildName()
        {
            int cnt = 0;
            string name = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.GroupBy(p => p.GuildId).ToList();
                    foreach(var item in list)
                    {
                        if(db.Messages.Where(p => !p.IsBot && p.GuildId == item.First().GuildId).ToList().Count > cnt)
                        {
                            cnt = db.Messages.Where(p => !p.IsBot && p.GuildId == item.First().GuildId).ToList().Count;
                            name = item.First().GuildName;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return name;
        }

        [Command("info"), Alias("information")]
        [Summary("General information command to display guild, user info, and bot info")]
        [MinPermissions(AccessLevel.User)]
        public Task InfoAsync()
        {
            string g_icon_url = GetGuildIconUrl(0);
            string g_name = GetGuildName(0);
            string g_url = GetGuildUrl(0);
            Color g_color = GetGuildColor();
            string g_thumbnail = GetGuildThumbNail();
            string g_description = GetGuildDescription(0);
            string[] g_footer = GetGuildFooter(0);
            string g_prefix = GetGuildPrefix();

            DateTime dt = new DateTime(Context.Guild.CreatedAt.Year, Context.Guild.CreatedAt.Month, Context.Guild.CreatedAt.Day, 0, 0, 0, 0);
            string established_date = String.Format("{0:dddd, MMMM d, yyyy}", dt) + "-" + Math.Round((DateTime.Now - Context.Guild.CreatedAt.DateTime).TotalDays, 0) + " Days Old!";
            

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithDescription(g_description)
                .WithColor(g_color)
                .WithTitle("General Information sheet for " + g_name)
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);
            builder
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Server Owner"), Value = Context.Guild.Owner })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Guild Established"), Value = established_date})
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Roles"), Value = Context.Guild.Roles.Count() })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Member Count"), Value = Context.Guild.Users.Count() })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Bot Count"), Value = Context.Guild.Users.Where(b => b.IsBot).Count() })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Channel Count"), Value = Context.Guild.Channels.Count() })
                .AddField(x =>
                {
                    x.Name = Format.Underline("Avatar URL");
                    x.Value = GetGuildIconUrl(0);
                })
                .AddField(x =>
                {
                    x.Name = $"For guild statistical data type {g_prefix}stats";
                    x.Value = GetUptime();
                });

            return ReplyAsync("", embed: builder);
        }

        [Command("info"), Alias("information")]
        [Summary("General information command to display guild, user info, and bot info")]
        [MinPermissions(AccessLevel.User)]
        public Task InfoAsync(SocketUser user)
        {
            int id = 0;
            bool bot = CheckBot(user);
            var person = user as IGuildUser;
            if (!bot)
            {
                id = 1;
            }
            else
            {
                id = 2;
            }
            string g_icon_url = GetGuildIconUrl(id);
            string g_name = GetGuildName(id);
            string g_url = GetGuildUrl(id);
            Color g_color = GetGuildColor();
            string g_description = GetGuildDescription(id);
            string[] g_footer = GetGuildFooter(id);
            string[] m_count = GetLifeTimeMessages(user);

            System.Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>{0}", id);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithColor(g_color)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);
            if(id == 1)
            {
                double joined = (DateTime.Now - person.JoinedAt)?.TotalDays ?? 0;
                string join_date = String.Format("{0:dddd, MMMM d, yyyy}", person.JoinedAt?.DateTime ?? DateTime.Now);
                string last_seen = GetLastMessageTime(user);
                builder
                    .WithTitle("General Information sheet for " + Context.Guild.GetUser(user.Id).Username)
                    .WithThumbnailUrl(person.GetAvatarUrl().ToString())
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("ID : "), Value = person.Id })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Nickname : "), Value = person.Nickname ?? "no previous Nickname" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Past Names : "), Value = person.Nickname ?? "no previous Nickname" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Avatar : "), Value = person.GetAvatarUrl() })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Playing : "), Value = (person.Game.ToString() != "") ? "Activity" : "Inactivity" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Status : "), Value = person.Status.ToString() })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Joined Server : "), Value = join_date })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("lifetime messages : "), Value = m_count[0] })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Messages / hour : "), Value = m_count[1] })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Joined Server : "), Value = Math.Round(joined, 0) - 1 })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Last Seen : "), Value = last_seen })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Last Spoke : "), Value = last_seen })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Roles : "), Value = person.Guild.Roles.Count });
            }
            else if(id == 2)
            {
                builder
                    .WithTitle("Information for Siotrix Bot")
                    .WithDescription(g_description)
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .AddField( new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Owner : "), Value = Context.User.Id })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Guilds : "), Value = Context.Client.Guilds.Count })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Users : "), Value = Context.Guild.Users.Count })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("invite to guild : "), Value = "https://discordapp.com/oauth2/authorize?client_id=285812392930050048&scope=bot&permissions=0" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Siotrix Guild : "), Value = "https://discord.gg/saZDC" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Donate : "), Value = "https://discord.gg/saZDC" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Created : "), Value = Math.Round((DateTime.Now - Context.Client.CurrentUser.CreatedAt.DateTime).TotalDays, 0).ToString() + " Days" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Icon : "), Value = Context.Client.CurrentUser.GetAvatarUrl() })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Most Active Guild This Week : "), Value = GetActiveGuildName() })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Uptime : "), Value = GetUptime() })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Contributors : "), Value = Format.Bold("Dan Johnson and Frank Thomas") });
            }
            return ReplyAsync("", embed: builder);
        }
    }
}
