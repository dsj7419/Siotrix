using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Siotrix.Discord.Attributes.Preconditions;
using System.Threading.Tasks;

namespace Siotrix.Discord.Statistics
{
    [Name("Information")]
    public class LeaderBoardModule : ModuleBase<SocketCommandContext>
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

        private string GetUptime()
        {
            var uptime = (DateTime.Now - _process.StartTime);
            return $"{uptime.Days} day {uptime.Hours} hr {uptime.Minutes} min {uptime.Seconds} sec";
        }

        private List<long> GetLeaderBoardDatasPerGuild(bool isAll)
        {
            List<long> result = new List<long>();
            DateTime today = DateTime.Now;
            DateTime firstDayOfMonth = DateTimeExtensions.FirstDayOfMonth(today);
            DateTime lastDayOfMonth = DateTimeExtensions.LastDayOfMonth(today);
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();
                    var data = list.Where(p => p.CreatedAt <= lastDayOfMonth && p.CreatedAt >= firstDayOfMonth);
                    if (isAll)
                    {
                        var list_per_user = list.GroupBy(p => p.AuthorId);
                        int index = 0;
                        int[] cnt_arr = new int[list_per_user.ToList().Count];
                        foreach (var item in list_per_user)
                        {
                            cnt_arr[index] = item.ToList().Count;
                            System.Console.WriteLine(">>>>>>>>>>>{0}", cnt_arr[index]);
                            index++;
                        }
                        Array.Sort(cnt_arr);
                        Array.Reverse(cnt_arr);
                        int loop = 0;
                        foreach (var x1 in cnt_arr)
                        {
                            System.Console.WriteLine("*******{0}", x1);
                            if (loop > 25)
                            {
                                break;
                            }
                            foreach (var x2 in list_per_user)
                            {
                                if (x1 == x2.ToList().Count)
                                {
                                    result.Add(x2.First().AuthorId);
                                    loop++;
                                }
                            }
                        }
                    }
                    else
                    {
                        var list_per_user = data.GroupBy(p => p.AuthorId);
                        int index = 0;
                        int[] cnt_arr = new int[list_per_user.ToList().Count];
                        foreach (var item in list_per_user)
                        {
                            cnt_arr[index] = item.ToList().Count;
                            System.Console.WriteLine("~~~~~~~~~~{0}", cnt_arr[index]);
                            index++;
                        }
                        Array.Sort(cnt_arr);
                        Array.Reverse(cnt_arr);
                        int loop = 0;
                        foreach (var x1 in cnt_arr)
                        {
                            System.Console.WriteLine("========{0}", x1);
                            if (loop > 25)
                            {
                                break;
                            }
                            foreach (var x2 in list_per_user)
                            {
                                if (x1 == x2.ToList().Count)
                                {
                                    result.Add(x2.First().AuthorId);
                                    loop++;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return result;
        }

        private string[] GetTopUsers(long val, bool isAll)
        {
            DateTime today = DateTime.Now;
            string[] values = new string[2];
            DateTime firstDayOfMonth = DateTimeExtensions.FirstDayOfMonth(today);
            DateTime lastDayOfMonth = DateTimeExtensions.LastDayOfMonth(today);
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();
                    var data2 = list.Where(p => p.AuthorId == val && p.CreatedAt <= lastDayOfMonth && p.CreatedAt >= firstDayOfMonth);
                    var data1 = db.Messages.Where(p => !p.IsBot && p.AuthorId == val).ToList();
                    if (isAll)
                    {
                        values[0] = data1.First().Name;
                        values[1] = data1.ToList().Count.ToString();
                    }
                    else
                    {
                        values[0] = data2.First().Name;
                        values[1] = data2.ToList().Count.ToString();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return values;
        }

        [Command("leaderboard"), Alias("lb")]
        [Summary("Lists top message leaders in guild for the last 30 days.")]
        [Remarks(" - no additional arguments needed.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public Task StatsAsync()
        {
            string g_icon_url = GetGuildIconUrl();
            string g_name = GetGuildName();
            string g_url = GetGuildUrl();
            Color g_color = GetGuildColor();
            string g_thumbnail = GetGuildThumbNail();
            string[] g_footer = GetGuildFooter();
            List<long> list = GetLeaderBoardDatasPerGuild(false);
            string[] get_values = new string[2];
            int index = 0;

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithDescription($"Last 30 days")
                .WithColor(g_color)
                .WithTitle("Current Leaderboard for " + g_name)
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);
            foreach(var element in list)
            {
                index++;
                get_values = GetTopUsers(element, false);
                builder
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = index.ToString() + ") " + get_values[0], Value = get_values[1] + " messages"});
            }
            return ReplyAsync("", embed: builder);
        }

        [Command("leaderboard"), Alias("lb")]
        [Summary("Lists top message leaders in guild for all time.")]
        [Remarks("alltime - Keyword to activate all time leaderboard.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public Task StatsAsync(string alltime)
        {
            string g_icon_url = GetGuildIconUrl();
            string g_name = GetGuildName();
            string g_url = GetGuildUrl();
            Color g_color = GetGuildColor();
            string g_thumbnail = GetGuildThumbNail();
            string[] g_footer = GetGuildFooter();
            List<long> list = GetLeaderBoardDatasPerGuild(true);
            string[] get_values = new string[2];
            int index = 0;

            if (!alltime.Equals("alltime"))
            {
                return ReplyAsync($"Input Error");
            }
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithDescription($"All Time")
                .WithColor(g_color)
                .WithTitle("Current Leaderboard for " + g_name)
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);
            foreach (var element in list)
            {
                index++;
                get_values = GetTopUsers(element, true);
                builder
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = index.ToString() + ") " + get_values[0], Value = get_values[1] + " messages" });
            }
            return ReplyAsync("", embed: builder);
        }
    }
}
