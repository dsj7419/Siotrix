using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GynBot.Common.Attributes;
using GynBot.Common.Enums;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GynBot.Modules.Public
{
    [Name("Basic Commands")]
    public class PublicModule : ModuleBase<SocketCommandContext>
    {

        [Command("invite")]
        [Remarks("Returns the OAuth2 Invite URL of the bot")]
        [MinPermissions(AccessLevel.User)]
        public async Task Invite()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"A user with `MANAGE_SERVER` can invite me to your server here: <https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot>");
        }

        [Command("info")]
        [Remarks("General Information about the Bot and Server")]
        [MinPermissions(AccessLevel.User)]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var Color = new Color(114, 137, 218);
            var Author = $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n";

            var builder = new EmbedBuilder()
            .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl("http://cdn.mysitemyway.com/etc-mysitemyway/icons/legacy-previews/icons-256/blue-jelly-icons-alphanumeric/069500-blue-jelly-icon-alphanumeric-information1.png")
                .WithName("GynBot - A multi-purpose bot with a single global purpose")
                .WithUrl("https://discord.gg/RMUPGSf"))
            .WithColor(Color)
            .WithThumbnailUrl("https://s-media-cache-ak0.pinimg.com/564x/b5/a9/30/b5a930c07975d0935afbe210363edcde.jpg")
            .WithTitle("Information Sheet")
            .WithDescription($"Have Gynbot join your server! Use the command {Format.Bold("invite")} to see how!")
            .AddField(x =>
            {
                x.Name = $"- Author: {application.Owner.Username} (ID {application.Owner.Id})";
                x.Value = $"- Library: Discord.Net ({DiscordConfig.Version})";
            })
            .AddField(x =>
            {
                x.Name = $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}";
                x.Value = $"- Uptime: {GetUptime()}\n\n";
            })
            .AddField(x =>
            {
                x.Name = $"{Format.Bold("Stats")}\n";
                x.Value = $"- Heap Size: {GetHeapSize()} MB\n";
            })
            .AddField(x =>
            {
                x.Name = $"Guilds: { (Context.Client as DiscordSocketClient).Guilds.Count}\n";
                x.Value = $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}";
            })
            .AddField(x =>
            {
                x.Name = $"{Format.Bold("User Numbers")}\n";
                x.Value = $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}";
            })
            .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl("http://www.supagrowth.com/img/PBN-hunter-icon.ico")
                .WithText("Holding down the fort since 2017."))
            .WithTimestamp(DateTime.UtcNow);

            await ReplyAsync("", false, builder.Build());
            
        } 

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}