using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Siotrix.Discord.Statistics
{
    [Name("Information")]
    [Summary("Message leaderboard system.")]
    public class LeaderBoardModule : ModuleBase<SocketCommandContext>
    {
        private Process _process;

        protected override void BeforeExecute(CommandInfo info)
        {
            _process = Process.GetCurrentProcess();
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
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
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
              //  .WithThumbnailUrl(g_thumbnail)
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
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
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
           //     .WithThumbnailUrl(g_thumbnail)
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
