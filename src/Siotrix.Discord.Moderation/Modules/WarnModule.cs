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
    [Group("warn")]
    [Summary("Warn a misbehaving user.")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class WarnModule : ModuleBase<SocketCommandContext>
    {
        private string GetWarnData(long guild_id)
        {
            int[] time = new int[5];
            string list = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gwarns.Where(x => x.GuildId.Equals(Context.Guild.Id.ToLong()));
                    if (result.Any())
                    {
                        string mute_warn_num = result.FirstOrDefault(x => x.Option == 1).WarnValue.ToString();
                        string mute_time_num = result.FirstOrDefault(x => x.Option == 2).WarnValue.ToString();
                        string ban_warn_num = result.FirstOrDefault(x => x.Option == 3).WarnValue.ToString();
                        string ban_time_num = result.FirstOrDefault(x => x.Option == 4).WarnValue.ToString();
                        string perm_ban_num = result.FirstOrDefault(x => x.Option == 5).WarnValue.ToString();
                        list = "``Max number of warnings before a  ``**" + mute_time_num + "**``  time mute`` : " + "**" + mute_warn_num + "**\n" +
                            "``Max number of warnings before a  ``**" + ban_time_num + "**``  time ban`` : " + "**" + ban_warn_num + "**\n" +
                            "``Max number of serious offenses before a permanent ban`` : " + "**" + perm_ban_num + "**\n";
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
            bool is_success = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gwarns.Where(x => x.Option.Equals(option) && x.GuildId.Equals(Context.Guild.Id.ToLong()));
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
                    is_success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return is_success;
        }

        [Command]
        [Remarks("Increase a user's warning level by the specified amount")]
        public async Task WarnAsync()
        {
            var value = GetWarnData(Context.Guild.Id.ToLong()) ?? "No Setting Warn";
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
               x.Name = "Warn List";
               x.Value = value;
           });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command]
        [Summary("=======")]
        [Remarks("Increase a user's warning level by the specified amount")]
        public async Task WarnAsync(SocketGuildUser user, int level, [Remainder] string reason)
        {
            var success = SaveAndUpdateWarnData(1, num);
            if (success)
                await ReplyAsync("👍");
        }

        [Command("set mutewarn")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetMuteWarnAsync(int num)
        {
            var success = SaveAndUpdateWarnData(1, num);
            if (success)
                await ReplyAsync("👍");
        }

        [Command("set mutetime")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetMuteTimeAsync(int num)
        {
            var success = SaveAndUpdateWarnData(2, num);
            if (success)
                await ReplyAsync("👍");
        }

        [Command("set banwarn")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetBanWarnAsync(int num)
        {
            var success = SaveAndUpdateWarnData(3, num);
            if (success)
                await ReplyAsync("👍");
        }

        [Command("set bantime")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetBanTimeAsync(int num)
        {
            var success = SaveAndUpdateWarnData(4, num);
            if (success)
                await ReplyAsync("👍");
        }

        [Command("set permban")]
        [Summary("==============")]
        [Remarks("====================")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task SetPermBanAsync(int num)
        {
            var success = SaveAndUpdateWarnData(5, num);
            if (success)
                await ReplyAsync("👍");
        }
    }
}