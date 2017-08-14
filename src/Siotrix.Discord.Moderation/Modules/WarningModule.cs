using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("warning")]
    [Summary("A custom automated warning system that can be controlled by guilds.")]
    [RequireContext(ContextType.Guild)]
    public class WarningModule : ModuleBase<SocketCommandContext>
    {
        private string GetWarningUser(long userId, int id, long guildId)
        {
            string output = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Gwarningusers.Where(
                        x => x.Index == id && x.UserId == userId && x.GuildId == guildId);
                    if (data.Any())
                    {
                        var item = data.First();
                        output = "*" + item.CreatedAt.ToUniversalTime() + "* " + item.Type + " awarded by " +
                                 Context.Guild.GetUser(item.ModId.ToUlong()).Mention + "\n"
                                 + "Reason : " + item.Reason + "\n" + "Points Awarded : " + item.PointNum;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return output;
        }

        private string GetWarningUsers(long userId, long guildId)
        {
            string output = null;
            var total = 0;
            string value = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Gwarningusers.Where(x => x.UserId == userId && x.GuildId == guildId);
                    var falloff = db.Gwarns.Where(x => x.GuildId == guildId && x.Option == 6);
                    if (data.Any() && falloff.Any())
                    {
                        var list = data.OrderByDescending(x => x.CreatedAt).Take(10);
                        foreach (var item in list)
                        {
                            output += item.Index + ") " + item.PointNum + "    " + item.CreatedAt.ToUniversalTime() +
                                      "     " + item.Type + "\n";
                            total += item.PointNum;
                        }
                        value = output + "Total Points Active: " + total + "\n" + "Guild Max: 10 \n" +
                                "Guild Falloff Time: " + falloff.First().WarnValue + " Days\n";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return value;
        }

        private string GetWarningPeriod(long guildId, DateTime from, DateTime to)
        {
            string output = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Gwarningusers.Where(x => x.GuildId == guildId && x.CreatedAt >= from &&
                                                           x.CreatedAt <= to);
                    if (data.Any())
                        foreach (var item in data)
                            output += item.PointNum + "    " + item.CreatedAt.ToUniversalTime() + "     " + item.Type +
                                      "\n";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return output;
        }

        [Command]
        [Summary("Check your current warning level")]
        [Remarks(" - No additional arguments needed.")]
        [MinPermissions(AccessLevel.User)]
        public async Task WarningAsync()
        {
            if (!Context.User.IsBot)
            {
                var user = Context.User;
                var value = GetWarningUsers(user.Id.ToLong(), Context.Guild.Id.ToLong()) ?? "No Active Warnings";
                var gIconUrl = await Context.GetGuildIconUrlAsync();
                var gName = await Context.GetGuildNameAsync();
                var gUrl = await Context.GetGuildUrlAsync();
                var gThumbnail = await Context.GetGuildThumbNailAsync();
                var gFooter = await Context.GetGuildFooterAsync();
                var gPrefix = await Context.GetGuildPrefixAsync();
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(gIconUrl.Avatar)
                        .WithName(gName.GuildName)
                        .WithUrl(gUrl.SiteUrl))
                    .WithColor(new Color(255, 127, 0))
                    .WithThumbnailUrl(gThumbnail.ThumbNail)
                    .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(gFooter.FooterIcon)
                        .WithText(gFooter.FooterText))
                    .WithTimestamp(DateTime.UtcNow);
                builder
                    .AddField(x =>
                    {
                        x.Name = "Active Warnings for " + user.Username + "#" + user.Discriminator;
                        x.Value = value;
                    });
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        [Command]
        [Summary("Check warnings you received between two dates.")]
        [Remarks(" (from date) (to date)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task WarningAsync(DateTime from, DateTime to)
        {
            var value = GetWarningPeriod(Context.Guild.Id.ToLong(), from, to);
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gPrefix = await Context.GetGuildPrefixAsync();
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl.Avatar)
                    .WithName(gName.GuildName)
                    .WithUrl(gUrl.SiteUrl))
                .WithColor(new Color(255, 127, 0))
                .WithThumbnailUrl(gThumbnail.ThumbNail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter.FooterIcon)
                    .WithText(gFooter.FooterText))
                .WithTimestamp(DateTime.UtcNow);
            builder
                .AddField(x =>
                {
                    x.Name = "Active Warnings between " + from.ToUniversalTime() + " and " + to.ToUniversalTime();
                    x.Value = value;
                });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command]
        [Summary("Check warnings of a specified user")]
        [Remarks(" @username")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task WarningAsync(SocketGuildUser user)
        {
            var value = GetWarningUsers(user.Id.ToLong(), Context.Guild.Id.ToLong()) ?? "No Active Warnings";
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gPrefix = await Context.GetGuildPrefixAsync();
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl.Avatar)
                    .WithName(gName.GuildName)
                    .WithUrl(gUrl.SiteUrl))
                .WithColor(new Color(255, 127, 0))
                .WithThumbnailUrl(gThumbnail.ThumbNail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter.FooterIcon)
                    .WithText(gFooter.FooterText))
                .WithTimestamp(DateTime.UtcNow);
            builder
                .AddField(x =>
                {
                    x.Name = "Active Warnings for " + user.Username + "#" + user.Discriminator;
                    x.Value = value;
                });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command]
        [Summary("Check detailed warning of specified user using id number from list.")]
        [Remarks(" @username [number] - number from users warning list")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task WarningAsync(SocketGuildUser user, int id)
        {
            var value = GetWarningUser(user.Id.ToLong(), id, Context.Guild.Id.ToLong()) ?? "No Information";
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gPrefix = await Context.GetGuildPrefixAsync();
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl.Avatar)
                    .WithName(gName.GuildName)
                    .WithUrl(gUrl.SiteUrl))
                .WithColor(new Color(255, 127, 0))
                .WithThumbnailUrl(gThumbnail.ThumbNail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter.FooterIcon)
                    .WithText(gFooter.FooterText))
                .WithTimestamp(DateTime.UtcNow);
            builder
                .AddField(x =>
                {
                    x.Name = "Warning information of " + user.Username + "#" + user.Discriminator;
                    x.Value = value;
                });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command]
        [Summary("View information about a specific warning of yourself using a number from the list.")]
        [Remarks(" [number] - number from personal warning list.")]
        [MinPermissions(AccessLevel.User)]
        public async Task WarningAsync(int id)
        {
            if (!Context.User.IsBot)
            {
                var value = GetWarningUser(Context.User.Id.ToLong(), id, Context.Guild.Id.ToLong()) ?? "No Information";
                var gIconUrl = await Context.GetGuildIconUrlAsync();
                var gName = await Context.GetGuildNameAsync();
                var gUrl = await Context.GetGuildUrlAsync();
                var gThumbnail = await Context.GetGuildThumbNailAsync();
                var gFooter = await Context.GetGuildFooterAsync();
                var gPrefix = await Context.GetGuildPrefixAsync();
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(gIconUrl.Avatar)
                        .WithName(gName.GuildName)
                        .WithUrl(gUrl.SiteUrl))
                    .WithColor(new Color(255, 127, 0))
                    .WithThumbnailUrl(gThumbnail.ThumbNail)
                    .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(gFooter.FooterIcon)
                        .WithText(gFooter.FooterText))
                    .WithTimestamp(DateTime.UtcNow);
                builder
                    .AddField(x =>
                    {
                        x.Name = "Waning Informations";
                        x.Value = value;
                    });
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }
    }
}