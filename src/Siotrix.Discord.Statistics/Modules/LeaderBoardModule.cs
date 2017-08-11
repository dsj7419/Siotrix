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
                        var listPerUser = list.GroupBy(p => p.AuthorId);
                        var index = 0;
                        var cntArr = new int[listPerUser.ToList().Count];
                        foreach (var item in listPerUser)
                        {
                            cntArr[index] = item.ToList().Count;
                            Console.WriteLine(">>>>>>>>>>>{0}", cntArr[index]);
                            index++;
                        }
                        Array.Sort(cntArr);
                        Array.Reverse(cntArr);
                        var loop = 0;
                        foreach (var x1 in cntArr)
                        {
                            Console.WriteLine("*******{0}", x1);
                            if (loop > 25)
                                break;
                            foreach (var x2 in listPerUser)
                                if (x1 == x2.ToList().Count)
                                {
                                    result.Add(x2.First().AuthorId);
                                    loop++;
                                }
                        }
                    }
                    else
                    {
                        var listPerUser = data.GroupBy(p => p.AuthorId);
                        var index = 0;
                        var cntArr = new int[listPerUser.ToList().Count];
                        foreach (var item in listPerUser)
                        {
                            cntArr[index] = item.ToList().Count;
                            Console.WriteLine("~~~~~~~~~~{0}", cntArr[index]);
                            index++;
                        }
                        Array.Sort(cntArr);
                        Array.Reverse(cntArr);
                        var loop = 0;
                        foreach (var x1 in cntArr)
                        {
                            Console.WriteLine("========{0}", x1);
                            if (loop > 25)
                                break;
                            foreach (var x2 in listPerUser)
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
            var gIconUrl = Context.GetGuildIconUrl();
            var gName = Context.GetGuildName();
            var gUrl = Context.GetGuildUrl();
            var gColor = Context.GetGuildColor();
            var gThumbnail = Context.GetGuildThumbNail();
            var gFooter = Context.GetGuildFooter();
            var list = GetLeaderBoardDatasPerGuild(false);
            var getValues = new string[2];
            var index = 0;

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithDescription($"Last 30 days")
                .WithColor(gColor)
                .WithTitle("Current Leaderboard for " + gName)
                //  .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter[0])
                    .WithText(gFooter[1]))
                .WithTimestamp(DateTime.UtcNow);
            foreach (var element in list)
            {
                index++;
                getValues = GetTopUsers(element, false);
                builder
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = index + ") " + getValues[0],
                        Value = getValues[1] + " messages"
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
            var gIconUrl = Context.GetGuildIconUrl();
            var gName = Context.GetGuildName();
            var gUrl = Context.GetGuildUrl();
            var gColor = Context.GetGuildColor();
            var gThumbnail = Context.GetGuildThumbNail();
            var gFooter = Context.GetGuildFooter();
            var list = GetLeaderBoardDatasPerGuild(true);
            var getValues = new string[2];
            var index = 0;

            if (!alltime.Equals("alltime"))
                return ReplyAsync($"Input Error");
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithDescription($"All Time")
                .WithColor(gColor)
                .WithTitle("Current Leaderboard for " + gName)
                //     .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter[0])
                    .WithText(gFooter[1]))
                .WithTimestamp(DateTime.UtcNow);
            foreach (var element in list)
            {
                index++;
                getValues = GetTopUsers(element, true);
                builder
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = index + ") " + getValues[0],
                        Value = getValues[1] + " messages"
                    });
            }
            return ReplyAsync("", embed: builder);
        }
    }
}