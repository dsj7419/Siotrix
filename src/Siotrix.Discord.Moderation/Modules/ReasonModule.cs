using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    public class ReasonModule : ModuleBase<SocketCommandContext>
    {
        private long GetModLogChannelId()
        {
            long id = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    id = db.Gmodlogchannels.Where(p => p.GuildId.Equals(Context.Guild.Id.ToLong())).First().ChannelId;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return id;
        }


        [Command("reason")]
        [Summary("Give or edit an existing reason for a specified case number.")]
        [Remarks(" <number> (reason)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task ReasonAsync(long case_number, [Remainder] string reason)
        {
            if (!case_number.Equals(ActionResult.CaseId))
            {
                await ReplyAsync("Wrong Number. Try again!");
                return;
            }

            var mod_channel_id = GetModLogChannelId();
            if (mod_channel_id <= 0) return;
            var channel = Context.Guild.GetChannel(mod_channel_id.ToUlong()) as ISocketMessageChannel;

            var g_icon_url = Context.GetGuildIconUrl();
            var g_name = Context.GetGuildName();
            var g_url = Context.GetGuildUrl();
            var g_color = Context.GetGuildColor();
            var g_thumbnail = Context.GetGuildThumbNail();
            var g_footer = Context.GetGuildFooter();
            string value = null;
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(g_icon_url)
                    .WithName(g_name)
                    .WithUrl(g_url))
                .WithColor(g_color)
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(g_footer[0])
                    .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);
            var action_user_name = Context.Guild.GetUser(ActionResult.UserId.ToUlong()).Mention;
            if (ActionResult.CommandName.Equals("mute"))
                value = "User : " + action_user_name + "(" + ActionResult.UserId + ")\n" + "Moderator : " +
                        Context.User.Username + "(" + Context.User.Id + ")\n" +
                        "Length : " + ActionResult.TimeLength + "minutes" + "\n" +
                        "Reason : " + reason + " (edited)";
            else
                value = "User : " + action_user_name + "(" + ActionResult.UserId + ")\n" + "Moderator : " +
                        Context.User.Username + "(" + Context.User.Id + ")\n" +
                        "Reason : " + reason + " (edited)";
            builder
                .AddField(x =>
                {
                    x.Name = "Case " + "#" + case_number.ToString() + " | " + ActionResult.CommandName;
                    x.Value = value;
                });
            CaseExtensions.SaveCaseDataAsync(ActionResult.CommandName, ActionResult.CaseId, ActionResult.UserId,
                Context.Guild.Id.ToLong(), reason);
            await ActionResult.Instance.ModifyAsync(x => { x.Embed = builder.Build(); });
            await ReplyAsync("Case #" + case_number + " has been updated.");
        }

        [Command("reason")]
        [Summary("Updates reason for your last case.")]
        [Remarks(" (reason)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task ReasonAsync([Remainder] string reason)
        {
            var mod_channel_id = GetModLogChannelId();
            if (mod_channel_id <= 0) return;
            var channel = Context.Guild.GetChannel(mod_channel_id.ToUlong()) as ISocketMessageChannel;

            var g_icon_url = Context.GetGuildIconUrl();
            var g_name = Context.GetGuildName();
            var g_url = Context.GetGuildUrl();
            var g_color = Context.GetGuildColor();
            var g_thumbnail = Context.GetGuildThumbNail();
            var g_footer = Context.GetGuildFooter();
            string value = null;
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(g_icon_url)
                    .WithName(g_name)
                    .WithUrl(g_url))
                .WithColor(g_color)
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(g_footer[0])
                    .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);
            var action_user_name = Context.Guild.GetUser(ActionResult.UserId.ToUlong()).Mention;
            if (ActionResult.CommandName.Equals("mute"))
                value = "User : " + action_user_name + "(" + ActionResult.UserId + ")\n" + "Moderator : " +
                        Context.User.Username + "(" + Context.User.Id + ")\n" +
                        "Length : " + ActionResult.TimeLength + "minutes" + "\n" +
                        "Reason : " + reason + " (edited)";
            else
                value = "User : " + action_user_name + "(" + ActionResult.UserId + ")\n" + "Moderator : " +
                        Context.User.Username + "(" + Context.User.Id + ")\n" +
                        "Reason : " + reason + " (edited)";
            builder
                .AddField(x =>
                {
                    x.Name = "Case " + "#" + ActionResult.CaseId.ToString() + " | " + ActionResult.CommandName;
                    x.Value = value;
                });
            CaseExtensions.SaveCaseDataAsync(ActionResult.CommandName, ActionResult.CaseId, ActionResult.UserId,
                Context.Guild.Id.ToLong(), reason);
            await ActionResult.Instance.ModifyAsync(x => { x.Embed = builder.Build(); });
            await ReplyAsync("Case #" + ActionResult.CaseId + " has been updated.");
        }
    }
}