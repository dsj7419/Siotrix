using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("warn")]
    [Summary("Warn a misbehaving user.")]
    [RequireContext(ContextType.Guild)]
    public class WarnModule : ModuleBase<SocketCommandContext>
    {
        private string GetWarnData(long guildId)
        {
            string list = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gwarns.Where(x => x.GuildId == Context.Guild.Id.ToLong());
                    if (result.Any())
                    {
                        var muteWarnNum = result.FirstOrDefault(x => x.Option == 1).WarnValue.ToString();
                        var time1 = TimeSpan.FromMinutes(result.FirstOrDefault(x => x.Option == 2).WarnValue);
                        var muteTimeNum = string.Format("{0}{1}{2}", time1.Days > 0 ? time1.Days + " days " : null,
                            time1.Hours > 0 ? time1.Hours + " hours " : null,
                            time1.Minutes > 0 ? time1.Minutes + " minutes " : null);
                        var banWarnNum = result.FirstOrDefault(x => x.Option == 3).WarnValue.ToString();
                        var time2 = TimeSpan.FromMinutes(result.FirstOrDefault(x => x.Option == 4).WarnValue);
                        var banTimeNum = string.Format("{0}{1}{2}", time2.Days > 0 ? time2.Days + " days " : null,
                            time2.Hours > 0 ? time2.Hours + " hours " : null,
                            time2.Minutes > 0 ? time2.Minutes + " minutes " : null);
                        var permBanNum = result.FirstOrDefault(x => x.Option == 5).WarnValue.ToString();
                        var time3 = TimeSpan.FromMinutes(result.FirstOrDefault(x => x.Option == 6).WarnValue);
                        var fallOffNum = string.Format("{0}{1}{2}", time3.Days > 0 ? time3.Days + " days " : null,
                            time3.Hours > 0 ? time3.Hours + " hours " : null,
                            time3.Minutes > 0 ? time3.Minutes + " minutes " : null);
                        list = "``Warning points before a  ``**" + muteTimeNum + "**``  mute`` : " + "**" +
                               muteWarnNum + "**\n" +
                               "``Warning points before a  ``**" + banTimeNum + "**``  ban`` : " + "**" +
                               banWarnNum + "**\n" +
                               "``Serious offences needed for permenent ban`` : " + "**" + permBanNum + "**\n" +
                               "``Rate at which warning points fall off a member`` : " + "**" + fallOffNum + "**\n";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return list;
        }

        private bool SaveAndUpdateWarnData(int option, int num)
        {
            var isSuccess = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gwarns.Where(x => x.Option == option && x.GuildId == Context.Guild.Id.ToLong());
                    if (!result.Any())
                    {
                        var record = new DiscordGuildWarnInfo();
                        record.GuildId = Context.Guild.Id.ToLong();
                        record.Option = option;
                        record.WarnValue = num;
                        db.Gwarns.Add(record);
                    }
                    else
                    {
                        var data = result.First();
                        data.WarnValue = num;
                        db.Gwarns.Update(data);
                    }
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

        private bool SaveAndUpdateWarningUsers(long userId, long guildId, int pointNum, string reason, DateTime time,
            long modId, InfractionType type)
        {
            string infractionType = null;
            switch (type)
            {
                case InfractionType.Manual:
                    infractionType = "Manual Infraction";
                    break;
                case InfractionType.Filter:
                    infractionType = "Filter Infraction";
                    break;
                case InfractionType.Repeat:
                    infractionType = "Repeat Infraction";
                    break;
                case InfractionType.Caps:
                    infractionType = "Caps Infraction";
                    break;
                default:
                    break;
            }

            var isSuccess = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var record = new DiscordGuildWarningUser();
                    record.UserId = userId;
                    record.GuildId = guildId;
                    record.PointNum = pointNum;
                    record.Reason = reason;
                    record.CreatedAt = time;
                    record.ModId = modId;
                    record.Type = infractionType;
                    record.Index = db.Gwarningusers.Where(x => x.GuildId == guildId && x.UserId == userId).Count() +
                                   1;
                    db.Gwarningusers.Add(record);
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

        [Command]
        [Summary(" - Increase a user's warning level by the specified amount")]
        [Remarks(" - Increase a user's warning level by the specified amount")]
        public async Task WarnAsync()
        {
            var value = GetWarnData(Context.Guild.Id.ToLong()) ?? "No Setting Warn";
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gPrefix = Context.GetGuildPrefix();
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
                    x.Name = "Warn List";
                    x.Value = value;
                });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command]
        [Summary(" - user, level, reason")]
        [Remarks(" - user, level, reason")]
        public async Task WarnAsync(SocketGuildUser user, int points, [Remainder] string reason)
        {
            var success = SaveAndUpdateWarningUsers(user.Id.ToLong(), Context.Guild.Id.ToLong(), points, reason,
                DateTime.Now, Context.User.Id.ToLong(), InfractionType.Manual);
            if (success)
                await ReplyAsync("👍");
        }

        /*[Command("set mutewarn")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetMuteWarnAsync(int num)
        {
            var success = SaveAndUpdateWarnData(1, num);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Command("set mutetime")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetMuteTimeAsync([Remainder]TimeSpan time)
        {
            var minutes = time.TotalMinutes;
            var success = SaveAndUpdateWarnData(2, (int)minutes);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Command("set banwarn")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetBanWarnAsync(int num)
        {
            var success = SaveAndUpdateWarnData(3, num);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Command("set bantime")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetBanTimeAsync([Remainder]TimeSpan time)
        {
            var minutes = time.TotalMinutes;
            var success = SaveAndUpdateWarnData(4, (int)minutes);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Command("set permban")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetPermBanAsync(int num)
        {
            var success = SaveAndUpdateWarnData(5, num);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Command("set falloff")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetFallOffAsync([Remainder]TimeSpan time)
        {
            var minutes = time.TotalMinutes;
            var success = SaveAndUpdateWarnData(6, (int)minutes);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }*/

        [Command("set")]
        [Summary(
            "Length of Time person is muted, banned, or set the falloff time(how long a warning lasts on a user once issued).")]
        [Remarks(
            " <parameter> <timespan> - Parameters are: **mutetime** : time person is muted for when it hits warning number. **bantime** : time person is banned for when it hits ban number. **falloff** : time person is falloff for when it hits falloff number.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetTimeSpanAsync(string param, [Remainder] TimeSpan time)
        {
            var success = false;
            switch (param)
            {
                case "mutetime":
                    success = SetMuteTime(time);
                    break;
                case "bantime":
                    success = SetBanTime(time);
                    break;
                case "falloff":
                    success = SetFallOff(time);
                    break;
                default:
                    success = false;
                    break;
            }
            if (success)
                await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        private bool SetMuteTime(TimeSpan time)
        {
            var minutes = time.TotalMinutes;
            var success = SaveAndUpdateWarnData(2, (int) minutes);
            return success;
        }

        private bool SetBanTime(TimeSpan time)
        {
            var minutes = time.TotalMinutes;
            var success = SaveAndUpdateWarnData(4, (int) minutes);
            return success;
        }

        private bool SetFallOff(TimeSpan time)
        {
            var minutes = time.TotalMinutes;
            var success = SaveAndUpdateWarnData(6, (int) minutes);
            return success;
        }

        [Command("set")]
        [Summary("Number of warnings before user is muted, banned, or permanently banned.")]
        [Remarks(
            "<parameter> <number> - Parameters are: **mutewarn** :  number of warnings before user is muted. **banwarn** : number of warnings before user is temporarily banned. **permban** : number of SERIOUS INFRACTIONS before person is permanently banned.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetActionAsync(string param, int num)
        {
            var success = false;
            switch (param)
            {
                case "mutewarn":
                    success = SetMuteWarn(num);
                    break;
                case "banwarn":
                    success = SetBanWarn(num);
                    break;
                case "permban":
                    success = SetPermBan(num);
                    break;
                default:
                    success = false;
                    break;
            }
            if (success)
                await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        private bool SetMuteWarn(int num)
        {
            var success = SaveAndUpdateWarnData(1, num);
            return success;
        }

        private bool SetBanWarn(int num)
        {
            var success = SaveAndUpdateWarnData(3, num);
            return success;
        }

        private bool SetPermBan(int num)
        {
            var success = SaveAndUpdateWarnData(5, num);
            return success;
        }
    }
}