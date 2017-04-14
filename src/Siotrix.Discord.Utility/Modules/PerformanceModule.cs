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
    [Group("performance")]
    [Summary("General performance information and specs for Siotrix.")]
    [MinPermissions(AccessLevel.User)]
    public class PerformanceModule : ModuleBase<SocketCommandContext>
    {
        private Process _process;

        protected override void BeforeExecute()
        {
            _process = Process.GetCurrentProcess();
        }

        [Command]
        [Summary("General performance information and specs for Siotrix.")]
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

        [Command("uptime")]
        public Task UptimeAsync()
            => ReplyAsync(GetUptime());

        [Command("library"), Alias("lib")]
        public Task LibraryAsync()
            => ReplyAsync(GetLibrary());

        [Command("operatingsystem"), Alias("os")]
        public Task OperatingSystemAsync()
            => ReplyAsync(GetOperatingSystem());

        [Command("framework")]
        public Task FrameworkAsync()
            => ReplyAsync(GetFramework());

        [Command("memoryusage"), Alias("memory", "mem")]
        public Task MemoryUsageAsync()
            => ReplyAsync(GetMemoryUsage());

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
