using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

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
            var uptime = DateTime.Now - _process.StartTime;
            return $"{uptime.Days} day {uptime.Hours} hr {uptime.Minutes} min {uptime.Seconds} sec";
        }

        private List<long> GetLeaderBoardDatasPerGuild(bool isAll)
        {
            var result = new List<long>();
            var today = DateTime.Now;
            var firstDayOfMonth = today.FirstDayOfMonth();
            var lastDayOfMonth = today.LastDayOfMonth();
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();
                    var data = list.Where(p => p.CreatedAt <= lastDayOfMonth && p.CreatedAt >= firstDayOfMonth);
                    if (isAll)
                    {
                        var list_per_user = list.GroupBy(p => p.AuthorId);
                        var index = 0;
                        var cnt_arr = new int[list_per_user.ToList().Count];
                        foreach (var item in list_per_user)
                        {
                            cnt_arr[index] = item.ToList().Count;
                            Console.WriteLine(">>>>>>>>>>>{0}", cnt_arr[index]);
                            index++;
                        }
                        Array.Sort(cnt_arr);
                        Array.Reverse(cnt_arr);
                        var loop = 0;
                        foreach (var x1 in cnt_arr)
                        {
                            Console.WriteLine("*******{0}", x1);
                            if (loop > 25)
                                break;
                            foreach (var x2 in list_per_user)
                                if (x1 == x2.ToList().Count)
                                {
                                    result.Add(x2.First().AuthorId);
                                    loop++;
                                }
                        }
                    }
                    else
                    {
                        var list_per_user = data.GroupBy(p => p.AuthorId);
                        var index = 0;
                        var cnt_arr = new int[list_per_user.ToList().Count];
                        foreach (var item in list_per_user)
                        {
                            cnt_arr[index] = item.ToList().Count;
                            Console.WriteLine("~~~~~~~~~~{0}", cnt_arr[index]);
                            index++;
                        }
                        Array.Sort(cnt_arr);
                        Array.Reverse(cnt_arr);
                        var loop = 0;
                        foreach (var x1 in cnt_arr)
                        {
                            Console.WriteLine("========{0}", x1);
                            if (loop > 25)
                                break;
                            foreach (var x2 in list_per_user)
                                if (x1 == x2.ToList().Count)
                                {
                                    result.Add(x2.First().AuthorId);
                                    loop++;
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
            var today = DateTime.Now;
            var values = new string[2];
            var firstDayOfMonth = today.FirstDayOfMonth();
            var lastDayOfMonth = today.LastDayOfMonth();
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();
                    var data2 = list.Where(p => p.AuthorId == val && p.CreatedAt <= lastDayOfMonth &&
                                                p.CreatedAt >= firstDayOfMonth);
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

        [Command("leaderboard")]
        [Alias("lb")]
        [Summary("Lists top message leaders in guild for the last 30 days.")]
        [Remarks(" - no additional arguments needed.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public Task StatsAsync()
        {
            var g_icon_url = Context.GetGuildIconUrl();
            var g_name = Context.GetGuildName();
            var g_url = Context.GetGuildUrl();
            var g_color = Context.GetGuildColor();
            var g_thumbnail = Context.GetGuildThumbNail();
            var g_footer = Context.GetGuildFooter();
            var list = GetLeaderBoardDatasPerGuild(false);
            var get_values = new string[2];
            var index = 0;

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
            foreach (var element in list)
            {
                index++;
                get_values = GetTopUsers(element, false);
                builder
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = index + ") " + get_values[0],
                        Value = get_values[1] + " messages"
                    });
            }
            return ReplyAsync("", embed: builder);
        }

        [Command("leaderboard")]
        [Alias("lb")]
        [Summary("Lists top message leaders in guild for all time.")]
        [Remarks("alltime - Keyword to activate all time leaderboard.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public Task StatsAsync(string alltime)
        {
            var g_icon_url = Context.GetGuildIconUrl();
            var g_name = Context.GetGuildName();
            var g_url = Context.GetGuildUrl();
            var g_color = Context.GetGuildColor();
            var g_thumbnail = Context.GetGuildThumbNail();
            var g_footer = Context.GetGuildFooter();
            var list = GetLeaderBoardDatasPerGuild(true);
            var get_values = new string[2];
            var index = 0;

            if (!alltime.Equals("alltime"))
                return ReplyAsync($"Input Error");
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
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = index + ") " + get_values[0],
                        Value = get_values[1] + " messages"
                    });
            }
            return ReplyAsync("", embed: builder);
        }
    }
}