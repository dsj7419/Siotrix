using Discord;
using Discord.WebSocket;
using Siotrix.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Siotrix.Discord.Utility
{
    [Name("utility")]
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
            return Context.ReplyAsync("", embed: builder);
        }

        [Name("no-help")]
        [Command("uptime")]
        public Task UptimeAsync()
            => Context.ReplyAsync(GetUptime());

        [Name("no-help")]
        [Command("library"), Alias("lib")]
        public Task LibraryAsync()
            => Context.ReplyAsync(GetLibrary());

        [Name("no-help")]
        [Command("operatingsystem"), Alias("os")]
        public Task OperatingSystemAsync()
            => Context.ReplyAsync(GetOperatingSystem());

        [Name("no-help")]
        [Command("framework")]
        public Task FrameworkAsync()
            => Context.ReplyAsync(GetFramework());

        [Name("no-help")]
        [Command("memoryusage"), Alias("memory", "mem")]
        public Task MemoryUsageAsync()
            => Context.ReplyAsync(GetMemoryUsage());

        [Name("no-help")]
        [Command("latency"), Alias("lag", "ping")]
        public Task LatencyAsync()
            => Context.ReplyAsync(GetLatency());

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
            => $"{Context.Client.Latency}ms";
    }
}
