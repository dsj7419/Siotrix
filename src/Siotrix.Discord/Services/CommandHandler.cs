using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Siotrix.Discord.Admin;
using Siotrix.Discord.Audio;
using Siotrix.Discord.Events;
using Siotrix.Discord.Utility;
using Siotrix.Discord.Developer;
using Siotrix.Discord.Moderation;
using Siotrix.Discord.Roslyn;
using Siotrix.Discord.Statistics;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Siotrix.Discord.Readers;
using Discord.Addons.InteractiveCommands;

namespace Siotrix.Discord
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;
        private DependencyMap _map;

        public CommandHandler(DiscordSocketClient client, DependencyMap map)
        {
            _client = client;
            _map = map;
            _map.Add(new InteractiveService(_client));
            client.JoinedGuild += Siotrix_JoinedGuild;
        }

        public object Interactive { get; private set; }

        public async Task StartAsync()
        {
            _service = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
            });
           
            _service.AddTypeReader(typeof(Uri), new UriTypeReader());
            _service.AddTypeReader(typeof(TimeSpan), new TimeSpanTypeReader());
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());

            var config = Configuration.Load();
            if (config.Modules.Admin)
                await _service.LoadAdminAsync();
            if (config.Modules.Audio)
                await _service.LoadAudioAsync();
            if (config.Modules.Events)
                await _service.LoadEventsAsync();
            if (config.Modules.Utility)
                await _service.LoadUtilityAsync();
            if (config.Modules.Developer)
                await _service.LoadDeveloperAsync();
            if (config.Modules.Moderation)
                await _service.LoadModerationAsync();
            if (config.Modules.Roslyn)
                await _service.LoadRoslynAsync();
            if (config.Modules.Statistics)
                await _service.LoadStatisticsAsync();

            _client.MessageReceived += HandleCommandAsync;
            await PrettyConsole.LogAsync("Info", "Commands", $"Loaded {_service.Commands.Count()} commands");
        }

        public Task StopAsync()
        {
            _service = null;
            _client.MessageReceived -= HandleCommandAsync;
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            string spec = null;
            using (var db = new LogDatabase())
            {
                var guild_id = context.Guild.Id;
                try
                {
                    var arr = db.Gprefixs.Where(p => p.GuildId == guild_id.ToLong());
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        spec = "!";
                    }
                    else
                    {
                        spec = arr.First().Prefix;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (s.Author.IsBot
                || msg == null
                || !msg.Content.Except("?").Any()
                || msg.Content.Trim().Length <= 1
                || msg.Content.Trim()[1] == '?'
                || (!(msg.HasStringPrefix(spec, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))))
                return;

            if (msg.HasStringPrefix(spec, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                System.Console.WriteLine("======={0}", _client.CurrentUser);
                var result = await _service.ExecuteAsync(context, argPos, _map);
                System.Console.WriteLine("111111111111");
                if (!result.IsSuccess)
                {
                    if (result.Error == CommandError.UnknownCommand)
                    {
                        await context.Channel.SendMessageAsync("Command not recognized");
                        return;
                    }

                    if (result is ExecuteResult r)
                        PrettyConsole.NewLine(r.Exception.ToString());
                    else
                    {
                        var embed = new EmbedBuilder().WithColor(new Color(255, 0, 0)).WithTitle("**Error:**").WithDescription(result.ErrorReason);
                        await context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        private async Task Siotrix_JoinedGuild(SocketGuild guild)
        {
            var gowner = guild.Owner; // Get Guild owner of guild Siotrix just joined.

            try
            {
                bool cansend = true;
                foreach (var p in guild.DefaultChannel.PermissionOverwrites)
                {
                    if (p.Permissions.SendMessages == PermValue.Deny)
                        cansend = false;
                }

                var builder = new EmbedBuilder()
                 .WithAuthor(new EmbedAuthorBuilder()
                 .WithIconUrl("https://i.imgur.com/3sc5OG3.png")
                 .WithName("Siotrix Bot")
                 .WithUrl("https://discord.gg/e6sku22"))
                 .WithColor(new Color(1, 1, 1))
                 .WithThumbnailUrl("http://img04.imgland.net/WyZ5FoM.png")
                 .WithTitle($"Hi {guild.Name}!")
                 .WithDescription($"Thank-You for inviting me to {guild.Name}! It's great to be here! I've already messaged {gowner.Username} about how to configure me, but I " +
                                        "also have an extensive help system as well. I'm completely customizable to make me look however you want!")
                 .AddField(new EmbedFieldBuilder() { IsInline = true, Name = "Live Support: ", Value = "https://discord.gg/e6sku22" })
                 .AddField(new EmbedFieldBuilder() { IsInline = true, Name = "Invite me to another guild: ", Value = "https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot>" })
                 .WithFooter(new EmbedFooterBuilder()
                 .WithIconUrl("http://img04.imgland.net/WyZ5FoM.png")
                 .WithText("A global bot with a local feel."))
                 .WithTimestamp(DateTime.UtcNow);

                if (!cansend)
                {
                    SocketTextChannel channel = null;
                    bool pes = true;
                    foreach (var cha in guild.TextChannels)
                    {
                        pes = true;
                        foreach (var per in cha.PermissionOverwrites)
                        {
                            if (per.Permissions.SendMessages == PermValue.Deny)
                            {
                                pes = false;
                                break;
                            }
                        }
                        if (pes == true)
                        {
                            channel = cha;
                            break;
                        }
                    }
                    builder.AddField(new EmbedFieldBuilder() { IsInline = true, Name = ":sob:", Value = "I was not able to send this in the default Channel" });
                    await channel.SendMessageAsync("", false, builder);
                }
                else
                {
                    await guild.DefaultChannel.SendMessageAsync("", false, builder);
                }
                PrettyConsole.NewLine($"Just joined {guild.Name} ({guild.Id}). Guild Owner: {gowner.Username}");
            }
            catch (Exception e)
            {
                PrettyConsole.NewLine($"GUILD JOIN ERROR: {e.ToString()}");
            }


            // Initiate Direct Message with guild owner to offer setup of Siotrix
            /* var ownerdm = await gowner.CreateDMChannelAsync();
             await ownerdm.SendMessageAsync($"Hi {gowner.Username}, I see you are the owner of {guild.Name}. Since you just invited me to your guild I wanted to see if you'd be interested in a quick interactive setup?\n\n" +
                                                         "You can answer with a simple yes or no.");
             var response = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));
             if (response.Content == "no") return;

             var appInfo = await _client.GetApplicationInfoAsync(); // Get app info of our bot
             var channel = await appInfo.Owner.CreateDMChannelAsync(); // Get the DM channel with our app owner, create it if it doesn't exist


             var msg = $"I just joined a guild: `{guild.Name}` ({guild.Id})";
             await channel.SendMessageAsync(msg); // Send a new message in our DM channel */
        }
    }
}
