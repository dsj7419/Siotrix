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

        [Command("list")]
        [Summary("Complete list of all performance information.")]
        [Remarks(" - no additional argument needed")]
        public Task PerformanceAsync()
        {
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            var builder = new EmbedBuilder();
            builder.Color = g_color;
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
    }
}
