using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    [Group("leaveguild")]
    [Summary(
        "Instructs the bot to leave a Guild specified by a developer. This could could be temporary or permanent.")]
    [MinPermissions(AccessLevel.BotOwner)]
    public class LeaveGuildModule : ModuleBase<SocketCommandContext>
    {
        private bool SaveLeaveGuild(long guildId, string reason, string guildName)
        {
            var isSuccess = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var record = new DiscordBanGuildList();
                    record.GuildId = guildId;
                    record.Reason = reason;
                    record.GuildName = guildName;
                    db.Banguildlists.Add(record);
                    db.SaveChanges();
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isSuccess;
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
                    {
                        list = "No banned guilds at this moment.";
                    }
                    else
                    {
                        var index = 0;
                        foreach (var item in result)
                        {
                            index++;
                            list += index + ". " + item.GuildName + " - " + item.GuildId + " - " + item.Reason + "\n";
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

        private string DeleteLeaveGuild(long guildId)
        {
            string reason = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Banguildlists.Where(x => x.GuildId == guildId);
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
        public async Task LeaveGuildAsync(ulong id, [Remainder] string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                throw new Exception("You must provide a reason!");

            var client = Context.Client;
            var gld = Context.Client.GetGuild(id) as IGuild;
            var ch = await gld.GetDefaultChannelAsync();

            var embed = new EmbedBuilder();
            embed.Description =
                $"Hello, I've been instructed by my developers to leave this guild..\n**Reason: **{msg}";
            embed.Color = new Color(0, 0, 255);
            embed.Author = new EmbedAuthorBuilder
            {
                Name = "Siotrix - click to join my discord!",
                IconUrl = SiotrixConstants.BotAvatar,
                Url = SiotrixConstants.DiscordInv
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
            var gIconUrl = Context.GetGuildIconUrl();
            var gName = Context.GetGuildName();
            var gUrl = Context.GetGuildUrl();
            var gThumbnail = Context.GetGuildThumbNail();
            var gFooter = Context.GetGuildFooter();
            var gPrefix = Context.GetGuildPrefix();
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithColor(new Color(255, 127, 0))
                .WithThumbnailUrl(gThumbnail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter[0])
                    .WithText(gFooter[1]))
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
        public async Task BanGuildAsync(ulong id, [Remainder] string msg)
        {
            //TODO: Must do ban code for leaveguild command
            if (string.IsNullOrWhiteSpace(msg))
                throw new Exception("You must provide a reason!");

            var client = Context.Client;
            var gld = Context.Client.GetGuild(id) as IGuild;
            var ch = await gld.GetDefaultChannelAsync();

            var embed = new EmbedBuilder();
            embed.Description = $"The developers have banned me from this guild..\n**Reason: **{msg}";
            embed.Color = new Color(255, 0, 0);
            embed.Author = new EmbedAuthorBuilder
            {
                Name = "Siotrix - click to join my discord!",
                IconUrl = SiotrixConstants.BotAvatar,
                Url = SiotrixConstants.DiscordInv
            };
            await ch.SendMessageAsync("", embed: embed);
            await Task.Delay(5000);
            await gld.LeaveAsync();
            var success = SaveLeaveGuild(id.ToLong(), msg, gld.Name);
            if (success)
                await ReplyAsync($"Message has been sent and I've left the guild forever! {gld.Name}");
        }

        [Command("guildunban")]
        [Summary("Un-bans bot from guild, allowing guild to re-invite Siotrix.")]
        [Remarks("<Id> - ID number in the ban list of the guild you want to unban.")]
        public async Task UnbanGuildAsync(ulong guildId)
        {
            var reason = DeleteLeaveGuild(guildId.ToLong());
            if (reason != null)
            {
                var gIconUrl = Context.GetGuildIconUrl();
                var gName = Context.GetGuildName();
                var gUrl = Context.GetGuildUrl();
                var gThumbnail = Context.GetGuildThumbNail();
                var gFooter = Context.GetGuildFooter();
                var gPrefix = Context.GetGuildPrefix();
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(gIconUrl)
                        .WithName(gName)
                        .WithUrl(gUrl))
                    .WithColor(new Color(255, 0, 0))
                    .WithThumbnailUrl(gThumbnail)
                    .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(gFooter[0])
                        .WithText(gFooter[1]))
                    .WithTimestamp(DateTime.UtcNow);
                builder
                    .WithTitle("Unauthorized Guild")
                    .WithDescription(
                        "Hi, thank you for inviting me to the server. Unfortunately I am not able to stay as you have been added to my banlist.\nReason: " +
                        " ***" + reason + " ***")
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("If you have questions, feel free to join our Discord at : "),
                        Value = SiotrixConstants.DiscordInv
                    });
                await ReplyAsync("", embed: builder);
            }
        }
    }
}