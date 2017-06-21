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

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("warning")]
    [Summary("A custom automated warning system that can be controlled by guilds.")]
    [RequireContext(ContextType.Guild)]
    public class WarningModule : ModuleBase<SocketCommandContext>
    {
        private string GetWarningInfo(int id)
        {
            string info = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Warninginfos.Where(x => x.Level == id);
                    if (!result.Any())
                        info = "No Warning Information";
                    else
                        info = result.First().Info;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return info;
        }

        private string GetWarningUser(long user_id, int id, long guild_id)
        {
            string output = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Warninginfos.Where(x => x.Level == id);
                    if (result.Any())
                    {
                        var info = result.First().Info;
                        var data = db.Gwarningusers.Where(x => x.Level == id && x.UserId == user_id && x.GuildId == guild_id);
                        if (data.Any())
                        {
                            var item =data.First();
                            output = "*" + item.CreatedAt.ToUniversalTime().ToString() + "* " + info + " awarded by " + Context.Guild.GetUser(item.ModId.ToUlong()).Mention + "\n"
                                + "Reason : " + item.Reason + "\n" + "Points Awarded : " + item.Level;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return output;
        }

        private string GetWarningUsers(long user_id, long guild_id)
        {
            string output = null;
            int total = 0;
            string value = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Gwarningusers.Where(x => x.UserId == user_id && x.GuildId == guild_id);
                    var compare = db.Warninginfos.ToList();
                    var falloff = db.Gwarns.Where(x => x.GuildId == guild_id && x.Option == 6);
                    if (data.Any() && compare.Any() && falloff.Any()) 
                    {
                        int index = 0;
                        var list = data.OrderByDescending(x => x.CreatedAt).Take(10);
                        foreach (var item in list)
                        {
                            foreach(var i in compare)
                            {
                                if(item.Level == i.Level)
                                {
                                    index++;
                                    output += index.ToString() + ") " + item.Level + "    " + item.CreatedAt.ToUniversalTime().ToString() + "     " + i.Info + "\n";
                                    total += item.Level;
                                }
                            }
                        }
                        value = output + "Total Points Active: " + total.ToString() + "\n" + "Guild Max: 10 \n" + "Guild Falloff Time: " + falloff.First().WarnValue.ToString() + " Days\n";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return value;
        }

        private bool SaveWarningInformation(int id, string info)
        {
            bool is_success = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Warninginfos.Where(x => x.Level == id);
                    if (!result.Any())
                    {
                        var record = new DiscordWarningInfo();
                        record.Level = id;
                        record.Info = info;
                        db.Warninginfos.Add(record);
                    }
                    else
                    {
                        var data = result.First();
                        data.Info = info;
                        db.Warninginfos.Update(data);
                    }
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

        private string GetWarningPeriod(long guild_id, DateTime from, DateTime to)
        {
            string output = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Gwarningusers.Where(x => x.GuildId == guild_id && (x.CreatedAt >= from && x.CreatedAt <= to));
                    var compare = db.Warninginfos.ToList();
                    if (data.Any() && compare.Any())
                    {
                        foreach (var item in data)
                        {
                            foreach (var i in compare)
                            {
                                if (item.Level == i.Level)
                                {
                                    output += item.Level + "    " + item.CreatedAt.ToUniversalTime().ToString() + "     " + i.Info + "\n";
                                }
                            }
                        }
                    }
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
            var value = GetWarningInfo(id);
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
               x.Name = "Waning Informations";
               x.Value = value;
           });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command]
        [Summary(" - Save Information")]
        [Remarks(" - Save Information")]
        [MinPermissions(AccessLevel.User)]
        public async Task WarningAsync(int id, [Remainder] string info)
        {
            var success = SaveWarningInformation(id, info);
            if (success)
                await ReplyAsync("👍");
        }
    }
}
