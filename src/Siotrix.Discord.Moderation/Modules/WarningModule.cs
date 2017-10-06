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
    [Summary("Check warning status of yourself as a user, or others as a moderator")]
    [RequireContext(ContextType.Guild)]
    public class WarningModule : ModuleBase<SocketCommandContext>
    {
      /*  private string GetWarningUser(long userId, int id, long guildId)
        {
            string output = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Gusercases.Where(
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
                    var data = db.Gusercases.Where(x => x.UserId == userId && x.GuildId == guildId);
                    var falloff = db.Gwarnsettings.Where(x => x.GuildId == guildId && x.Option == 6);
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
                    var data = db.Gusercases.Where(x => x.GuildId == guildId && x.CreatedAt >= from &&
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
        } */

        [Command("check")]
        [Summary("Check your current active warning level.")]
        [Remarks(" - No additional arguments needed.")]
        [MinPermissions(AccessLevel.User)]
        public async Task WarningAsync()
        {
            if (!Context.User.IsBot)
            {                             
                var gIconUrl = await Context.GetGuildIconUrlAsync();
                var gName = await Context.GetGuildNameAsync();
                var gUrl = await Context.GetGuildUrlAsync();
                var gFooter = await Context.GetGuildFooterAsync();
                var gColor = await Context.GetGuildColorAsync();
                int count = 0;
                string value = null;

                var userWarnings = await UserCaseExtensions.GetUserCasessAsync(Context.Guild.Id, Context.User.Id, true);

                if (userWarnings == null)
                {
                    await ReplyAsync("You have no active warnings, good job!");
                    return;
                }

                var guildWarnInfo = await UserCaseExtensions.GetWarnSettingsAsync(Context.Guild.Id);
                var guildUserWarnTracking = await UserCaseExtensions.GetUserCaseTrackingAsync(Context.Guild.Id, Context.User.Id);

                foreach (var userWarning in userWarnings)
                {
                    count++;
                    if (userWarning.Type == "WARN")
                    {
                        value += $"{count}) Warning Points: {userWarning.WarningPoints} Reason: {userWarning.Reason}\n";
                    }
                    else
                    {
                        value += $"{count}) Warning Points: {userWarning.WarningPoints} Resuled in: {userWarning.Type} Reason: {userWarning.Reason}\n";
                    }
                }

                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(gIconUrl.Avatar)
                        .WithName(gName.GuildName)
                        .WithUrl(gUrl.SiteUrl))
                    .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                    .WithTitle($"Total active warning points: {guildUserWarnTracking.ActiveWarningUserPoints} of a guild maximum: {guildWarnInfo.TimesBeforeBan}")
                    .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(gFooter.FooterIcon)
                        .WithText(gFooter.FooterText))
                    .WithTimestamp(DateTime.UtcNow);
                builder
                    .AddField(x =>
                    {
                        x.Name = "Your currently active warnings:";
                        x.Value = value;
                    });
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        [Command("userdaterange")]
        [Summary("Check warnings a user as received between a certain date range")]
        [Remarks("(username) (from date) (to date)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task WarningAsync(SocketGuildUser user, DateTime from, DateTime to)
        {;
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gColor = await Context.GetGuildColorAsync();
            int count = 0;
            string value = null;

            var userWarnings = await UserCaseExtensions.GetUserCasessAsync(Context.Guild.Id, Context.User.Id, from, to);

            if (userWarnings == null)
            {
                await ReplyAsync("They have no active warnings.");
                return;
            }

            foreach (var userWarning in userWarnings)
            {
                count++;
                var newTime = String.Format("{0:r}", userWarning.CreatedAt);
                if (userWarning.Type == "WARN")
                {
                    value += $"{count}) Warning Points: {userWarning.WarningPoints} Case Number: {userWarning.CaseNum} Activate at: {newTime}\n" +
                             $"Reason: {userWarning.Reason}\n";
                }
                else if (userWarning.WarningPoints > 0)
                {
                    value +=
                        $"{count}) Warning Points: {userWarning.WarningPoints} Resuled in: {userWarning.Type} Case Number: {userWarning.CaseNum} Activate at: {newTime}\n" +
                        $"Reason: {userWarning.Reason}\n";
                }
                else
                {
                    value +=
                        $"{count}) Punish Type: {userWarning.Type} Case Number: {userWarning.CaseNum} Activate at: {newTime}\n" +
                        $"Reason: {userWarning.Reason}\n";
                }
            }

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl.Avatar)
                    .WithName(gName.GuildName)
                    .WithUrl(gUrl.SiteUrl))
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
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

        [Command("checkuser")]
        [Summary("Check warnings of a specified user, and optionally you can search all of users warnings")]
        [Remarks("@username [false] - use false to see all users warnings.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task WarningAsync(SocketGuildUser user, bool active = true)
        {
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gColor = await Context.GetGuildColorAsync();
            int count = 0;
            string value = null;

            var userWarnings = await UserCaseExtensions.GetUserCasessAsync(Context.Guild.Id, user.Id, active);

            if (userWarnings == null)
            {
                await ReplyAsync("They have no active warnings.");
                return;
            }

            if (active)
            {
                foreach (var userWarning in userWarnings)
                {
                    count++;
                    if (userWarning.Type == "WARN")
                    {
                        value += $"{count}) Case Number: {userWarning.CaseNum} Warning Points: {userWarning.WarningPoints} Reason: {userWarning.Reason}\n";
                    }
                    else if (userWarning.WarningPoints > 0)
                    {
                        value +=
                            $"{count}) Case Number: {userWarning.CaseNum} Warning Points: {userWarning.WarningPoints} Resuled in: {userWarning.Type} Reason: {userWarning.Reason}\n";
                    }
                    else
                    {
                        value +=
                            $"{count}) Case Number: {userWarning.CaseNum} Warning Points: {userWarning.WarningPoints} Resuled in: {userWarning.Type} Reason: {userWarning.Reason}\n";
                    }
                }
            }
            else
            {
                foreach (var userWarning in userWarnings)
                {
                    var newTime = String.Format("{0:r}", userWarning.CreatedAt);
                    var isActivate = userWarning.IsActive ? "active" : "not active";
                    count++;
                    if (userWarning.Type == "WARN")
                    {
                        value += $"{count}) {isActivate} Warning Points: {userWarning.WarningPoints} Case Number: {userWarning.CaseNum} Activate at: {newTime}\n" +
                                 $"Reason: {userWarning.Reason}\n";
                    }
                    else if (userWarning.WarningPoints > 0)
                    {
                        value +=
                            $"{count}) {isActivate} Warning Points: {userWarning.WarningPoints} Resuled in: {userWarning.Type} Case Number: {userWarning.CaseNum} Activate at: {newTime}\n" +
                            $"Reason: {userWarning.Reason}\n";
                    }
                    else
                    {
                        value +=
                            $"{count}) {isActivate} Punish Type: {userWarning.Type} Case Number: {userWarning.CaseNum} Activate at: {newTime}\n" +
                            $"Reason: {userWarning.Reason}\n";
                    }
                }
            }

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl.Avatar)
                    .WithName(gName.GuildName)
                    .WithUrl(gUrl.SiteUrl))
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
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

        [Command("caseinfo")]
        [Summary("Check detailed information about a specific guild case.")]
        [Remarks("(number)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task WarningAsync(int id)
        {
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gColor = await Context.GetGuildColorAsync();
            string timedLineBuilder = null;
            string value = null;
            string username = null;
            string modname = null;

            var userCase = await UserCaseExtensions.GetUserCaseAsync(Context.Guild.Id, id);            

            if (userCase == null)
            {
                await ReplyAsync("That is not a valid case number, try again.");
                return;
            }

            var user = await ((IGuild)Context.Guild).GetUserAsync(userCase.UserId.ToUlong());
            var mod = await ((IGuild)Context.Guild).GetUserAsync(userCase.ModId.ToUlong());
            
            var guildWarnInfo = await UserCaseExtensions.GetWarnSettingsAsync(Context.Guild.Id);
            if (guildWarnInfo == null)
            {
                await UserCaseExtensions.CreateWarnSettingsAsync(Context.Guild.Id, SiotrixConstants.TimesBeforeMute,
                    SiotrixConstants.MuteTimeLengthMinutes, SiotrixConstants.TimesBeforeBan,
                    SiotrixConstants.BanTimeLengthMinutes, SiotrixConstants.SrsInfractionsBeforePermBan,
                    SiotrixConstants.WarningFalloffMinutes);
                guildWarnInfo = await UserCaseExtensions.GetWarnSettingsAsync(Context.Guild.Id);
            }

            var userFalloff = UserCaseExtensions.ProcessUserCaseFalloffTimeLeft(userCase, guildWarnInfo);

            switch (userCase.Type)
            {
                case "MUTE":
                    break;
                case "TIMEBAN":
                    break;
                case "PERMBAN":
                    break;
                case "WARN":
                    break;
                default:
                    await ReplyAsync("This isnt the kind of info I can display with this command");
                    return;
            }

            value = $"User : {user.Mention} ({user.Id})\n" +
                    $"Moderator : {mod.Mention}\n" +
                    $"{timedLineBuilder}\n" +
                    $"Reason: {userCase.Reason}";

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl.Avatar)
                    .WithName(gName.GuildName)
                    .WithUrl(gUrl.SiteUrl))
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter.FooterIcon)
                    .WithText(gFooter.FooterText))
                .WithTimestamp(DateTime.UtcNow);
            builder
                .AddField(x =>
                {
                    x.Name = "Case Information for " + user.Username + "#" + user.Discriminator;
                    x.Value = value;
                });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }       
    }
}