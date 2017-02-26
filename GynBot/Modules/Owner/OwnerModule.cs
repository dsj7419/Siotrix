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
    [Name("Owner Commands")]
    public partial class OwnerModule : ModuleBase<SocketCommandContext>
    {
        [Command("leave")]
        [Summary("Instructs the bot to leave this Guild.")]
        [Remarks("leave")]
        [MinPermissions(AccessLevel.ServerOwner)]
        public async Task Leave()
        {
            if (Context.Guild == null) { await ReplyAsync("This command can only be ran in your guild."); return; }
            await ReplyAsync("Leaving~");
            await Context.Guild.LeaveAsync();
        }

        [Command("echo")]
        [Summary("Echo's input into a specified channel.")]
        [Remarks("say #general I am alive!")]
        [MinPermissions(AccessLevel.ServerOwner)]
        public void Say([Summary("Target channel")] ITextChannel channel, [Remainder, Summary("Text to echo")] string text)
        {
            (Context.Client.GetChannel(channel.Id) as SocketTextChannel)?.SendMessageAsync(text);
        }

        [Command("powerdown"), Alias("pd")]
        [Summary("Terminates the bot application")]
        [Remarks("powerdown")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task PowerdownAsync()
        {
            await ReplyAsync("Powering down!").ConfigureAwait(false);
            await Context.Client.StopAsync().ConfigureAwait(false);
            await Task.Delay(1500).ConfigureAwait(false);
        }

        [Command("getinvite")]
        [Summary("Makes an invite to the specified guild")]
        [Remarks("getinvite 123456789987654321")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task GetInviteAsync([Summary("Target guild id")]ulong guild)
        {
            var channel = Context.Client.GetChannel((Context.Client.GetGuild(guild)).DefaultChannel.Id);
            var invite = await (channel as SocketGuildChannel).CreateInviteAsync();
            await ReplyAsync(invite.Url);
        }

        [Command("broadcast")]
        [Summary("Broadcasts a message to the default channel of all servers the bot is connected to.")]
        [Remarks("broadcast IMPORTANT MESSAGE")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task Broadcast([Remainder] string broadcast)
        {
            var guilds = Context.Client.Guilds;
            var defaultChannels = guilds.Select(g => g.GetChannel(g.Id)).Cast<ITextChannel>();
            await Task.WhenAll(defaultChannels.Select(c => c.SendMessageAsync($"***ANNOUNCEMENT @everyone: " + broadcast + "\n-Thank-You... Bot Staff***")));
        }
    }
}