using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

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
            var guildId = Context.Guild.Id;
            string iconurl = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gavatars.Where(p => p.GuildId == guildId.ToLong());
                    if (val == null || val.ToList().Count <= 0 || id == 2)
                        iconurl = db.Authors.First().AuthorIcon;
                    else
                        iconurl = val.First().Avatar;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return iconurl;
        }

        [Command]
        [Alias("list")]
        [Summary("Complete list of all performance information.")]
        [Remarks(" - no additional argument needed")]
        public async Task PerformanceAsync()
        {
            var gColor = await Context.GetGuildColorAsync();
            var gIconUrl = GetGuildIconUrl(0);
            var gThumbnail = await Context.GetGuildThumbNailAsync();

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName("Performance Summary for Siotrix Bot")
                    .WithUrl(SiotrixConstants.BotInvite))
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(Context.User.GetAvatarUrl())
                    .WithText($"{Context.User.Username}#{Context.User.Discriminator}"))
                .WithTimestamp(DateTime.UtcNow);

            builder.Color = GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex);
            builder.ThumbnailUrl = gThumbnail.ThumbNail;

            var desc = $"**Uptime:** {GetUptime()}\n" +
                       $"**Library:** {GetLibrary()}\n" +
                       $"**OS:** {GetOperatingSystem()}\n" +
                       $"**Framework:** {GetFramework()}\n" +
                       $"**Memory Usage:** {GetMemoryUsage()}\n" +
                       $"**Threads Running:** {GetThreads()}\n" +
                       $"**Latency:** {GetLatency()}\n\n" +
                       $"**Siotrix Official Discord:** " + SiotrixConstants.DiscordInv;

            builder.Description = desc;
            await ReplyAsync("", embed: builder);
        }

        [Command("uptime")]
        [Summary("Lists current update for Siotrix.")]
        [Remarks(" - no additional argument needed")]
        public Task UptimeAsync()
        {
            return ReplyAsync(GetUptime());
        }

        [Command("library")]
        [Alias("lib")]
        [Summary("Lists present Discord.Net library version.")]
        [Remarks(" - no additional argument needed")]
        public Task LibraryAsync()
        {
            return ReplyAsync(GetLibrary());
        }

        [Command("operatingsystem")]
        [Alias("os")]
        [Summary("Lists OS Siotrix is currently running on.")]
        [Remarks(" - no additional argument needed")]
        public Task OperatingSystemAsync()
        {
            return ReplyAsync(GetOperatingSystem());
        }

        [Command("framework")]
        [Summary("Current base framework version for Siotrix.")]
        [Remarks(" - no additional argument needed")]
        public Task FrameworkAsync()
        {
            return ReplyAsync(GetFramework());
        }

        [Command("memoryusage")]
        [Alias("memory", "mem")]
        [Summary("Lists how much memory load Siotrix has right now.")]
        [Remarks(" - no additional argument needed")]
        public Task MemoryUsageAsync()
        {
            return ReplyAsync(GetMemoryUsage());
        }

        [Command("threadcount")]
        [Alias("threads", "thread")]
        [Summary("Number of threads in use by Siotrix.")]
        [Remarks(" - no additional argument needed")]
        public Task ThreadCountAsync()
        {
            return ReplyAsync(GetThreads());
        }

        [Command("latency")]
        [Alias("lag", "ping")]
        [Summary("Lists current latency response of Siotrix.")]
        [Remarks(" - no additional argument needed")]
        public Task LatencyAsync()
        {
            return ReplyAsync(GetLatency());
        }

        public string GetUptime()
        {
            var uptime = DateTime.Now - _process.StartTime;
            return $"{uptime.Days} day {uptime.Hours} hr {uptime.Minutes} min {uptime.Seconds} sec";
        }

        public string GetLibrary()
        {
            return $"Discord.Net ({DiscordConfig.Version})";
        }

        public string GetOperatingSystem()
        {
            return $"{RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}";
        }

        public string GetFramework()
        {
            return RuntimeInformation.FrameworkDescription;
        }

        public string GetMemoryUsage()
        {
            return $"{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)}mb";
        }

        public string GetLatency()
        {
            return $"{Context.Client.Latency}ms";
        }

        public string GetThreads()
        {
            return
                $"{_process.Threads.OfType<ProcessThread>().Where(t => t.ThreadState == ThreadState.Running).Count()} / {_process.Threads.Count}";
        }
    }
}