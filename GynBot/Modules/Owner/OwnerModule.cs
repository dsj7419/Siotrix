using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GynBot.Common.Attributes;
using GynBot.Common.Enums;
using GynBot.Common.Types;
using GynBot.Common.Utilities;
using Discord.WebSocket;
using System.Text;
using System.Net.Http;
using System.IO;

namespace GynBot.Modules.Public
{
    [Name("Owner Commands")]
    public partial class OwnerModule : ModuleBase<SocketCommandContext>
    {

        [Command("leave")]
        [Summary("Instructs the bot to leave this Guild.")]
        [Remarks("leave")]
        [MinPermissions(AccessLevelEnum.ServerOwner)]
        public async Task Leave()
        {
            if (Context.Guild == null) { await ReplyAsync("This command can only be ran in your guild."); return; }
            await ReplyAsync("Leaving~");
            await Context.Guild.LeaveAsync();
        }

        [Command("ping")]
        [Summary("Check bot latency.")]
        [Remarks("ping")]
        [MinPermissions(AccessLevelEnum.ServerOwner)]
        public async Task sayasync()
        {

            if (Context.Message.Content.Contains("ping"))
            {

                var a = (Context.Client as DiscordSocketClient).Latency;
                var data = new EmbedBuilder();
                var fb = new EmbedFieldBuilder();


                data.WithTitle("PONG!                                                                                        ");
                data.WithThumbnailUrl("https://s-media-cache-ak0.pinimg.com/564x/b5/a9/30/b5a930c07975d0935afbe210363edcde.jpg");

                data.AddField(x =>
                {
                    x.Name = $"Latency                                                          ";
                    x.Value = $"{a.ToString()}ms                                                  ";
                });


                data.WithFooter(x =>
                {
                    if (a < 300)

                    {
                        x.Text = "Fast :D";
                        x.IconUrl = "https://s-media-cache-ak0.pinimg.com/236x/63/62/79/636279b10193e8521a06b6717ebccf14.jpg";
                        data.WithColor(new Color(0x49ff00));
                    }
                    else
                    {
                        x.Text = "Slow D:";
                        x.IconUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8c/SadSmiley.svg/1024px-SadSmiley.svg.png";
                        data.WithColor(new Color(0xFF0000));
                    }

                });
                data.WithTimestamp(DateTime.UtcNow);




                await ReplyAsync("", embed: data);
                return;
            }

        }

        [Command("echo")]
        [Summary("Echo's input into a specified channel.")]
        [Remarks("say #general I am alive!")]
        [MinPermissions(AccessLevelEnum.ServerOwner)]
        public void Say([Summary("Target channel")] ITextChannel channel, [Remainder, Summary("Text to echo")] string text)
        {
            (Context.Client.GetChannel(channel.Id) as SocketTextChannel)?.SendMessageAsync(text);
        }

        [Command("powerdown"), Alias("pd")]
        [Summary("Terminates the bot application")]
        [Remarks("powerdown")]
        [MinPermissions(AccessLevelEnum.BotOwner)]
        public async Task PowerdownAsync()
        {
            await ReplyAsync("Powering down!").ConfigureAwait(false);
            await Context.Client.StopAsync().ConfigureAwait(false);
            await Task.Delay(1500).ConfigureAwait(false);
        }

        [Command("getinvite")]
        [Summary("Makes an invite to the specified guild")]
        [Remarks("getinvite 123456789987654321")]
        [MinPermissions(AccessLevelEnum.BotOwner)]
        public async Task GetInviteAsync([Summary("Target guild id")]ulong guild)
        {
            var channel = Context.Client.GetChannel((Context.Client.GetGuild(guild)).DefaultChannel.Id);
            var invite = await (channel as SocketGuildChannel).CreateInviteAsync();
            await ReplyAsync(invite.Url);
        }

        [Command("guildlist")]
        [Summary("Lists guilds and owner for that guild")]
        [Remarks("guildlist")]
        [MinPermissions(AccessLevelEnum.BotOwner)]
        public async Task GuildListAsync()
        {
            var cl = Context.Client as DiscordSocketClient;
            StringBuilder sb = new StringBuilder();
            var Color = new Color(114, 137, 218);
            var prefix = Configuration.Load().Prefix;

            foreach (SocketGuild guild in cl.Guilds)
            {
                sb.AppendLine($"**Guild Name: **{guild.Name}\n**Guild ID: ** {guild.Id}\n**Guild Owner: **{guild.Owner} || {guild.OwnerId}\n");
            }
            EmbedBuilder builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl("http://cdn.mysitemyway.com/etc-mysitemyway/icons/legacy-previews/icons-256/blue-jelly-icons-alphanumeric/069500-blue-jelly-icon-alphanumeric-information1.png")
                .WithName("GynBot - A multi-purpose bot with a single global purpose")
                .WithUrl("https://discord.gg/RMUPGSf"))
            .WithColor(Color)
            .WithThumbnailUrl("https://s-media-cache-ak0.pinimg.com/564x/b5/a9/30/b5a930c07975d0935afbe210363edcde.jpg")
            .WithTitle("Information Sheet")
            .WithDescription($"Have Gynbot join your server! Use the command {Format.Bold($"{prefix}invite")} to see how!")
            .WithDescription(sb.ToString())
            .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl("http://www.supagrowth.com/img/PBN-hunter-icon.ico")
                .WithText("Holding down the fort since 2017."))
            .WithTimestamp(DateTime.UtcNow);

            await ReplyAsync("", embed: builder);

        }

        [Command("broadcast")]
        [Summary("Broadcasts a message to the default channel of all servers the bot is connected to.")]
        [Remarks("broadcast IMPORTANT MESSAGE")]
        [MinPermissions(AccessLevelEnum.BotOwner)]
        public async Task Broadcast([Remainder] string broadcast)
        {
            var guilds = Context.Client.Guilds;
            var defaultChannels = guilds.Select(g => g.GetChannel(g.Id)).Cast<ITextChannel>();
            await Task.WhenAll(defaultChannels.Select(c => c.SendMessageAsync($"***ANNOUNCEMENT @everyone: " + broadcast + "\n-Thank-You... Bot Staff***")));
        }
    }
}