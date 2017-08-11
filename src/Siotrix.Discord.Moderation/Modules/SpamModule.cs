using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("spam")]
    [Summary("Special guild-specific spam moderation commands.")]
    [RequireContext(ContextType.Guild)]
    public class SpamModule : ModuleBase<SocketCommandContext>
    {
        private bool SaveAndUpdateSpamData(int option, int num)
        {
            var isSuccess = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gspams.Where(x => x.Option.Equals(option) &&
                                                      x.GuildId.Equals(Context.Guild.Id.ToLong()));
                    if (!result.Any())
                    {
                        var record = new DiscordGuildSpamInfo();
                        record.GuildId = Context.Guild.Id.ToLong();
                        record.Option = option;
                        record.SpamValue = num;
                        db.Gspams.Add(record);
                    }
                    else
                    {
                        var data = result.First();
                        data.SpamValue = num;
                        db.Gspams.Update(data);
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

        private string GetSpamData(long guildId)
        {
            string data = null;
            string list = null;
            string spamValue = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gspams.Where(x => x.GuildId.Equals(Context.Guild.Id.ToLong()))
                        .OrderBy(x => x.Option);
                    if (result.Any())
                        foreach (var item in result)
                        {
                            switch (item.Option)
                            {
                                case 1:
                                    data = "Repeat spam warning";
                                    spamValue = item.SpamValue.ToString();
                                    break;
                                case 2:
                                    data = "Repeat spam mute";
                                    spamValue = item.SpamValue.ToString();
                                    break;
                                case 3:
                                    data = "Spam Mute Time";
                                    var time1 = TimeSpan.FromMinutes(item.SpamValue);
                                    spamValue = string.Format("{0}{1}{2}",
                                        time1.Days > 0 ? time1.Days + " days " : null,
                                        time1.Hours > 0 ? time1.Hours + " hours " : null,
                                        time1.Minutes > 0 ? time1.Minutes + " minutes " : null);
                                    break;
                                case 4:
                                    data = "Caps spam warning";
                                    spamValue = item.SpamValue.ToString();
                                    break;
                                case 5:
                                    data = "Repeat Caps mute";
                                    spamValue = item.SpamValue.ToString();
                                    break;
                                case 6:
                                    data = "Caps Mute Time";
                                    var time2 = TimeSpan.FromMinutes(item.SpamValue);
                                    spamValue = string.Format("{0}{1}{2}",
                                        time2.Days > 0 ? time2.Days + " days " : null,
                                        time2.Hours > 0 ? time2.Hours + " hours " : null,
                                        time2.Minutes > 0 ? time2.Minutes + " minutes " : null);
                                    break;
                                default:
                                    break;
                            }
                            list += "``" + data + "``" + " : **" + spamValue + "**\n";
                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return list;
        }

        [Command]
        [Summary("See current guild spam settings.")]
        [Remarks(" - No additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task GetSpamsAsync()
        {
            var value = GetSpamData(Context.Guild.Id.ToLong());
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
                    x.Name = "Spam Settings";
                    x.Value = value;
                });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        /* [Command("allow repeatspam")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetRepeatAsync(int num)
        {
            var success = SaveAndUpdateSpamData(1, num);
            if(success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Command("allow capsspam")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetCapsAsync(int num)
        {
            var success = SaveAndUpdateSpamData(4, num);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }*/

        [Command("allow")]
        [Summary("Sets how many lines of spam are allowed before a warning is issued.")]
        [Remarks(" (repeatspam or capsspam)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AllowSpamAsync(string param, int num)
        {
            var success = false;
            switch (param)
            {
                case "repeatspam":
                    success = SaveAndUpdateSpamData(1, num);
                    break;
                case "capsspam":
                    success = SaveAndUpdateSpamData(4, num);
                    break;
                default:
                    success = false;
                    break;
            }
            if (success)
                await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        /*[Command("makemute repeatspam")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task MuteRepeatAsync(int num)
        {
            var success = SaveAndUpdateSpamData(2, num);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Command("makemute capsspam")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task MuteCapsAsync(int num)
        {
            var success = SaveAndUpdateSpamData(5, num);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }*/

        [Command("spammute")]
        [Summary("Sets how many lines of spam are allowed before a mute is issued.")]
        [Remarks(" (repeatspam or capsspam)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task MakeMuteAsync(string param, int num)
        {
            var success = false;
            switch (param)
            {
                case "repeatspam":
                    success = SaveAndUpdateSpamData(2, num);
                    break;
                case "capsspam":
                    success = SaveAndUpdateSpamData(5, num);
                    break;
                default:
                    success = false;
                    break;
            }
            if (success)
                await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        /*[Command("mutetime repeatspam")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task MuteTimeRepeatAsync([Remainder]TimeSpan time)
        {
            var minutes = time.TotalMinutes;
            var success = SaveAndUpdateSpamData(3, (int)minutes);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Command("mutetime capsspam")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task MuteTimeCapsAsync([Remainder]TimeSpan time)
        {
            var minutes = time.TotalMinutes;
            var success = SaveAndUpdateSpamData(6, (int)minutes);
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }*/

        [Command("mutetime")]
        [Summary("How long to mute the person if they break the spammute parameters set by guild.")]
        [Remarks(" (repeatspam or capsspam) [time duration] - Can use 2d or 1h20m or 1 week.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task MuteTimeAsync(string param, [Remainder] TimeSpan time)
        {
            var success = false;
            var minutes = time.TotalMinutes;
            switch (param)
            {
                case "repeatspam":
                    success = SaveAndUpdateSpamData(3, (int) minutes);
                    break;
                case "capsspam":
                    success = SaveAndUpdateSpamData(6, (int) minutes);
                    break;
                default:
                    success = false;
                    break;
            }
            if (success)
                await ReplyAsync(SiotrixConstants.BotSuccess);
        }
    }
}