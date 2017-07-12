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
            bool is_success = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gspams.Where(x => x.Option.Equals(option) && x.GuildId.Equals(Context.Guild.Id.ToLong()));
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
                    is_success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return is_success;
        }

        private string GetSpamData(long guild_id)
        {
            string data = null;
            string list = null;
            string spam_value = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gspams.Where(x => x.GuildId.Equals(Context.Guild.Id.ToLong())).OrderBy(x => x.Option);
                    if (result.Any())
                    {
                        foreach(var item in result)
                        {
                            switch (item.Option)
                            {
                                case 1:
                                    data = "Repeat spam warning";
                                    spam_value = item.SpamValue.ToString();
                                    break;
                                case 2:
                                    data = "Repeat spam mute";
                                    spam_value = item.SpamValue.ToString();
                                    break;
                                case 3:
                                    data = "Spam Mute Time";
                                    var time1 = TimeSpan.FromMinutes((double)item.SpamValue);
                                    spam_value = string.Format("{0}{1}{2}", (time1.Days > 0) ? time1.Days.ToString() + " days " : null,
                                                    (time1.Hours > 0) ? time1.Hours.ToString() + " hours " : null,
                                                    (time1.Minutes > 0) ? time1.Minutes.ToString() + " minutes " : null);
                                    break;
                                case 4:
                                    data = "Caps spam warning";
                                    spam_value = item.SpamValue.ToString();
                                    break;
                                case 5:
                                    data = "Repeat Caps mute";
                                    spam_value = item.SpamValue.ToString();
                                    break;
                                case 6:
                                    data = "Caps Mute Time";
                                    var time2 = TimeSpan.FromMinutes((double)item.SpamValue);
                                    spam_value = string.Format("{0}{1}{2}", (time2.Days > 0) ? time2.Days.ToString() + " days " : null,
                                                    (time2.Hours > 0) ? time2.Hours.ToString() + " hours " : null,
                                                    (time2.Minutes > 0) ? time2.Minutes.ToString() + " minutes " : null);
                                    break;
                                default:
                                    break;
                            }
                            list += "``" + data + "``" + " : **" + spam_value + "**\n";
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

        [Command]
        [Summary("See current guild spam settings.")]
        [Remarks(" - No additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task GetSpamsAsync()
        {
            var value = GetSpamData(Context.Guild.Id.ToLong());
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
            bool success = false;
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
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
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
            bool success = false;
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
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
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
        public async Task MuteTimeAsync(string param, [Remainder]TimeSpan time)
        {
            bool success = false;
            var minutes = time.TotalMinutes;
            switch (param)
            {
                case "repeatspam":
                    success = SaveAndUpdateSpamData(3, (int)minutes);
                    break;
                case "capsspam":
                    success = SaveAndUpdateSpamData(6, (int)minutes);
                    break;
                default:
                    success = false;
                    break;
            }
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }
    }
}
