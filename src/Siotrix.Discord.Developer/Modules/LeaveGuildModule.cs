using Discord;
using Discord.Commands;
using Discord.Addons.EmojiTools;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.InteractiveCommands;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using Discord.WebSocket;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    [Group("leaveguild")]
    [Summary("Instructs the bot to leave a Guild specified by a developer. This could could be temporary or permanent.")]
    [MinPermissions(AccessLevel.BotOwner)]
    public class LeaveGuildModule : ModuleBase<SocketCommandContext>
    {
        private bool SaveLeaveGuild(long guild_id, string reason, string guild_name)
        {
            bool is_success = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var record = new DiscordBanGuildList();
                    record.GuildId = guild_id;
                    record.Reason = reason;
                    record.GuildName = guild_name;
                    db.Banguildlists.Add(record);
                    db.SaveChanges();
                    is_success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return is_success;
        }

        private string GetBanGuildList()
        {
            string list = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Banguildlists;
                    if (!result.Any())
                        list = "No banned guilds at this moment.";
                    else
                    {
                        int index = 0;
                        foreach(var item in result)
                        {
                            index++;
                            list += index.ToString() + ". " + item.GuildName + " - " + item.GuildId.ToString() + " - " + item.Reason + "\n";
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return list;
        }

        private string DeleteLeaveGuild(long guild_id)
        {
            string reason = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Banguildlists.Where(x => x.GuildId == guild_id);
                    if (result.Any())
                    {
                        reason = result.First().Reason;
                        db.Banguildlists.RemoveRange(result);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return reason;
        }

        [Command("guildkick")]
        [Summary("Instructs the bot to leave a Guild specified by a developer.")]
        [Remarks("<GuildID> <text> - include a brief reason why you are making the bot quit that guild.")]
        public async Task LeaveGuildAsync(ulong ID, [Remainder] string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                throw new Exception("You must provide a reason!");

            var client = Context.Client;
            var gld = Context.Client.GetGuild(ID) as IGuild;
            var ch = await gld.GetDefaultChannelAsync();

            var embed = new EmbedBuilder();
            embed.Description = $"Hello, I've been instructed by my developers to leave this guild..\n**Reason: **{msg}";
            embed.Color = new Color(0, 0, 255);
            embed.Author = new EmbedAuthorBuilder()
            {
                Name = "Siotrix - click to join my discord!",
                IconUrl = SiotrixConstants.BOT_AVATAR,
                Url = SiotrixConstants.DISCORD_INV
            };
            await ch.SendMessageAsync("", embed: embed);
            await Task.Delay(5000);
            await gld.LeaveAsync();
            await ReplyAsync($"Message has been sent and I've left the guild! {gld.Name}");
        }

        [Command("banlist")]
        [Summary("Lists all guilds banned from Siotrix joining.")]
        [Remarks(" - no additional arguments needed")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task BanGuildAsync()
        {
            var list = GetBanGuildList();
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
            string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithColor(new Color(255, 127, 0))
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);
            builder
           .AddField(x =>
           {
               x.Name = "Banned Guilds";
               x.Value = list;
           });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("guildban")]
        [Summary("Bans bot from guild, and makes it quit immediately.")]
        [Remarks("<GuildID> <text> - brief description why the ban is happening.")]
        public async Task BanGuildAsync(ulong ID, [Remainder] string msg)
        {
            //TODO: Must do ban code for leaveguild command
            if (string.IsNullOrWhiteSpace(msg))
                throw new Exception("You must provide a reason!");

            var client = Context.Client;
            var gld = Context.Client.GetGuild(ID) as IGuild;
            var ch = await gld.GetDefaultChannelAsync();

            var embed = new EmbedBuilder();
            embed.Description = $"The developers have banned me from this guild..\n**Reason: **{msg}";
            embed.Color = new Color(255, 0, 0);
            embed.Author = new EmbedAuthorBuilder()
            {
                Name = "Siotrix - click to join my discord!",
                IconUrl = SiotrixConstants.BOT_AVATAR,
                Url = SiotrixConstants.DISCORD_INV
            };
            await ch.SendMessageAsync("", embed: embed);
            await Task.Delay(5000);
            await gld.LeaveAsync();
            var success = SaveLeaveGuild(ID.ToLong(), msg, gld.Name);
            if(success)
                await ReplyAsync($"Message has been sent and I've left the guild forever! {gld.Name}");
            
        }

        [Command("guildunban")]
        [Summary("Un-bans bot from guild, allowing guild to re-invite Siotrix.")]
        [Remarks("<Id> - ID number in the ban list of the guild you want to unban.")]
        public async Task UnbanGuildAsync(ulong guild_id)
        {
            var reason = DeleteLeaveGuild(guild_id.ToLong());
            if (reason != null)
            {
                string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
                string g_name = GuildEmbedName.GetGuildName(Context);
                string g_url = GuildEmbedUrl.GetGuildUrl(Context);
                string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
                string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
                string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(g_icon_url)
                    .WithName(g_name)
                    .WithUrl(g_url))
                    .WithColor(new Color(255, 0, 0))
                    .WithThumbnailUrl(g_thumbnail)
                    .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(g_footer[0])
                    .WithText(g_footer[1]))
                    .WithTimestamp(DateTime.UtcNow);
                builder
                    .WithTitle("Unauthorized Guild")
                    .WithDescription("Hi, thank you for inviting me to the server. Unfortunately I am not able to stay as you have been added to my banlist.\nReason: " + " ***" + reason + " ***")
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("If you have questions, feel free to join our Discord at : "),
                        Value = SiotrixConstants.DISCORD_INV
                    });
                await ReplyAsync("", embed: builder);
            }
        }
    }
}