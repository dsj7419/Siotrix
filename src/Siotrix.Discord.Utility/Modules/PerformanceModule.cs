using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;

namespace Siotrix.Discord.Utility
{
    [Name("Information")]
    [Group("performance")]
    [Summary("General performance information and specs for Siotrix.")]
    [MinPermissions(AccessLevel.User)]
    public class PerformanceModule : ModuleBase<SocketCommandContext>
    {
        private Process _process;

        protected override void BeforeExecute(CommandInfo info)
        {
            _process = Process.GetCurrentProcess();
         //   Console.WriteLine(info.Summary);
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

        [Command, Alias("list")]
        [Summary("Complete list of all performance information.")]
        [Remarks(" - no additional argument needed")]
        public Task PerformanceAsync()
        {
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_icon_url = GetGuildIconUrl(0);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName("Performance Summary for Siotrix Bot")
                .WithUrl(SiotrixConstants.BOT_INVITE))
            .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(Context.User.GetAvatarUrl())
                .WithText($"{Context.User.Username}#{Context.User.Discriminator}"))
                .WithTimestamp(DateTime.UtcNow);

            builder.Color = g_color;
            builder.ThumbnailUrl = g_thumbnail;

            var desc = $"**Uptime:** {GetUptime()}\n" +
                       $"**Library:** {GetLibrary()}\n" +
                       $"**OS:** {GetOperatingSystem()}\n" +
                       $"**Framework:** {GetFramework()}\n" +
                       $"**Memory Usage:** {GetMemoryUsage()}\n" +
                       $"**Threads Running:** {GetThreads()}\n" +
                       $"**Latency:** {GetLatency()}\n\n" +
                       $"**Siotrix Official Discord:** " + SiotrixConstants.DISCORD_INV;

            builder.Description = desc;
            return ReplyAsync("", embed: builder);
        }

        [Command("uptime")]
        [Summary("Lists current update for Siotrix.")]
        [Remarks(" - no additional argument needed")]
        public Task UptimeAsync()
            => ReplyAsync(GetUptime());

        [Command("library"), Alias("lib")]
        [Summary("Lists present Discord.Net library version.")]
        [Remarks(" - no additional argument needed")]
        public Task LibraryAsync()
            => ReplyAsync(GetLibrary());

        [Command("operatingsystem"), Alias("os")]
        [Summary("Lists OS Siotrix is currently running on.")]
        [Remarks(" - no additional argument needed")]
        public Task OperatingSystemAsync()
            => ReplyAsync(GetOperatingSystem());

        [Command("framework")]
        [Summary("Current base framework version for Siotrix.")]
        [Remarks(" - no additional argument needed")]
        public Task FrameworkAsync()
            => ReplyAsync(GetFramework());

        [Command("memoryusage"), Alias("memory", "mem")]
        [Summary("Lists how much memory load Siotrix has right now.")]
        [Remarks(" - no additional argument needed")]
        public Task MemoryUsageAsync()
            => ReplyAsync(GetMemoryUsage());

        [Command("threadcount"), Alias("threads", "thread")]
        [Summary("Number of threads in use by Siotrix.")]
        [Remarks(" - no additional argument needed")]
        public Task ThreadCountAsync()
           => ReplyAsync(GetThreads());

        [Command("latency"), Alias("lag", "ping")]
        [Summary("Lists current latency response of Siotrix.")]
        [Remarks(" - no additional argument needed")]
        public Task LatencyAsync()
            => ReplyAsync(GetLatency());

        public string GetUptime()
        {
            var uptime = (DateTime.Now - _process.StartTime);
            return $"{uptime.Days} day {uptime.Hours} hr {uptime.Minutes} min {uptime.Seconds} sec";
        }

        public string GetLibrary()
            => $"Discord.Net ({DiscordConfig.Version})";

        public string GetOperatingSystem()
            => $"{RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}";

        public string GetFramework()
            => RuntimeInformation.FrameworkDescription;

        public string GetMemoryUsage()
            => $"{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)}mb";

        public string GetLatency()
            => $"{(Context.Client as DiscordSocketClient).Latency}ms";

        public string GetThreads()
            => $"{((IEnumerable)_process.Threads).OfType<ProcessThread>().Where(t => t.ThreadState == ThreadState.Running).Count()} / {_process.Threads.Count}";
    }
}
