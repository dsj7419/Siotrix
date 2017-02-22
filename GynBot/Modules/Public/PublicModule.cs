using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GynBot.Common.Attributes;
using GynBot.Common.Enums;
using GynBot.Common.Types;
using GynBot.Modules.Public.Services;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Reflection;

namespace GynBot.Modules.Public
{
    [Name("Basic Commands")]
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        private CommandService _service;

        public PublicModule(IDependencyMap map)
        {
            _service = map.Get<CommandService>();
        }

        [Command("invite"), Alias("join")]
        [Summary("Returns the OAuth2 Invite URL of the bot")]
        [MinPermissions(AccessLevel.User)]
        public async Task Invite()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await this.SendEmbedAsync(BuildEmbed($"Invite me to your server at:{Environment.NewLine}", $"<https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot>", 0));
        }

        [Command("help"),
            Alias("commands", "command", "cmds", "cmd"),
            Summary("Information about the bot's commands.")]
        public async Task HelpAsync([Remainder, Summary("Command/Module name to search for")]string name = "")
        {
            var modules = _service.Modules.OrderBy(x => x.Name);
            var commands = modules.SelectMany(m => m.Commands.Select(x => x).Distinct(new CommandNameComparer()));

            var cmd = commands.FirstOrDefault(x => x.Aliases.Contains(name.ToLower()));
            var module = modules.FirstOrDefault(x => x.Name.ToLower().Contains(name.ToLower()));
            var helpMode = name == "" ? HelpMode.All : cmd != null ? HelpMode.Command : module != null ? HelpMode.Module : HelpMode.All;

            switch (helpMode)
            {
                case HelpMode.All:
                    var errMsg = name == "" ? "" :
                        module == null && cmd == null ? "Module/Command not found, showing generic help instead." : "";
                    await ReplyAsync(errMsg, embed: HelpService.GetGenericHelpEmbed(modules, Context).WithAuthor(Context.Client.CurrentUser));
                    break;

                case HelpMode.Module:
                    if (!module.CanExecute(Context))
                    {
                        await ReplyAsync("You do not have permission to see information for this module.");
                        return;
                    }
                    await ReplyAsync("", embed: HelpService.GetModuleHelpEmbed(module, Context).WithAuthor(Context.Client.CurrentUser));
                    break;

                case HelpMode.Command:
                    if (!cmd.CanExecute(Context))
                    {
                        await ReplyAsync("You do not have permission to see information for this command.");
                        return;
                    }
                    await ReplyAsync("", embed: HelpService.GetCommandHelpEmbed(cmd, Context).WithAuthor(Context.Client.CurrentUser));
                    break;
            }
        }

        [Command("info")]
        [Summary("General Information about the Bot and Server")]
        [Remarks("info")]
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

    private Task<IUserMessage> SendEmbedAsync(EmbedBuilder embed)
        {
            return this.SendEmbedAsync(embed, null, this.Context.Message);
        }

        private Task<IUserMessage> SendEmbedAsync(EmbedBuilder embed, IUserMessage nmsg)
        {
            return this.SendEmbedAsync(embed, null, nmsg);
        }

        private Task<IUserMessage> SendEmbedAsync(EmbedBuilder embed, string content)
        {
            return this.SendEmbedAsync(embed, content, this.Context.Message);
        }

        private async Task<IUserMessage> SendEmbedAsync(EmbedBuilder embed, string content, IUserMessage nmsg)
        {
            var msg = nmsg;
            var mod = msg.Author.Id == this.Context.Client.CurrentUser.Id;

            if (mod)
                await msg.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                    if (!string.IsNullOrWhiteSpace(content))
                        x.Content = content;
                    else
                        x.Content = msg.Content;
                });
            else if (!string.IsNullOrWhiteSpace(content))
                msg = await msg.Channel.SendMessageAsync(string.Concat(msg.Author.Mention, ": ", content), false, embed);
            else
                msg = await msg.Channel.SendMessageAsync(msg.Author.Mention, false, embed);

            return msg;
        }

        private static EmbedBuilder BuildEmbed(string title, string desc, int type)
        {
            var embed = new EmbedBuilder()
            {
                Title = title,
                Description = desc
            };
            switch (type)
            {
                default:
                case 0:
                    embed.Color = new Color(0, 127, 255);
                    break;

                case 1:
                    embed.Color = new Color(255, 0, 0);
                    break;

                case 2:
                    embed.Color = new Color(127, 255, 0);
                    break;
            }
            if (type == 1)
                embed.ThumbnailUrl = "http://i.imgur.com/F9HGvxs.jpg";
            return embed;
        }

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}