using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Siotrix.Discord.Utility
{
    [Name("Information")]
    [Summary("General performance information and specs for Siotrix.")]
    [MinPermissions(AccessLevel.User)]
    public class PerformanceModule : ModuleBase<SocketCommandContext>
    {
        private Process _process;

        protected override void BeforeExecute()
        {
            _process = Process.GetCurrentProcess();
        }

        [Command("performance")]
        public Task PerformanceAsync()
        {
            var builder = new EmbedBuilder();
            builder.ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl();
            builder.Title = "Performance Summary";

            var uptime = (DateTime.Now - _process.StartTime);

            var desc = $"**Uptime:** {GetUptime()}\n" +
                       $"**Library:** {GetLibrary()}\n" +
                       $"**OS:** {GetOperatingSystem()}\n" +
                       $"**Framework:** {GetFramework()}\n" +
                       $"**Memory Usage:** {GetMemoryUsage()}\n" +
                       $"**Latency:** {GetLatency()}\n";

            builder.Description = desc;
            return ReplyAsync("", embed: builder);
        }

        [Name("no-help")]
        [Command("uptime")]
        public Task UptimeAsync()
            => ReplyAsync(GetUptime());

        [Name("no-help")]
        [Command("library"), Alias("lib")]
        public Task LibraryAsync()
            => ReplyAsync(GetLibrary());

        [Name("no-help")]
        [Command("operatingsystem"), Alias("os")]
        public Task OperatingSystemAsync()
            => ReplyAsync(GetOperatingSystem());

        [Name("no-help")]
        [Command("framework")]
        public Task FrameworkAsync()
            => ReplyAsync(GetFramework());

        [Name("no-help")]
        [Command("memoryusage"), Alias("memory", "mem")]
        public Task MemoryUsageAsync()
            => ReplyAsync(GetMemoryUsage());

        [Name("no-help")]
        [Command("latency"), Alias("lag", "ping")]
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
    }
}
