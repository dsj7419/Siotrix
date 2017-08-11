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
        public async Task ReasonAsync(long caseNumber, [Remainder] string reason)
        {
            if (!caseNumber.Equals(ActionResult.CaseId))
            {
                await ReplyAsync("Wrong Number. Try again!");
                return;
            }

            var modChannelId = GetModLogChannelId();
            if (modChannelId <= 0) return;
            var channel = Context.Guild.GetChannel(modChannelId.ToUlong()) as ISocketMessageChannel;

            var gIconUrl = Context.GetGuildIconUrl();
            var gName = Context.GetGuildName();
            var gUrl = Context.GetGuildUrl();
            var gColor = Context.GetGuildColor();
            var gThumbnail = Context.GetGuildThumbNail();
            var gFooter = Context.GetGuildFooter();
            string value = null;
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithColor(gColor)
                .WithThumbnailUrl(gThumbnail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter[0])
                    .WithText(gFooter[1]))
                .WithTimestamp(DateTime.UtcNow);
            var actionUserName = Context.Guild.GetUser(ActionResult.UserId.ToUlong()).Mention;
            if (ActionResult.CommandName.Equals("mute"))
                value = "User : " + actionUserName + "(" + ActionResult.UserId + ")\n" + "Moderator : " +
                        Context.User.Username + "(" + Context.User.Id + ")\n" +
                        "Length : " + ActionResult.TimeLength + "minutes" + "\n" +
                        "Reason : " + reason + " (edited)";
            else
                value = "User : " + actionUserName + "(" + ActionResult.UserId + ")\n" + "Moderator : " +
                        Context.User.Username + "(" + Context.User.Id + ")\n" +
                        "Reason : " + reason + " (edited)";
            builder
                .AddField(x =>
                {
                    x.Name = "Case " + "#" + caseNumber.ToString() + " | " + ActionResult.CommandName;
                    x.Value = value;
                });
            CaseExtensions.SaveCaseDataAsync(ActionResult.CommandName, ActionResult.CaseId, ActionResult.UserId,
                Context.Guild.Id.ToLong(), reason);
            await ActionResult.Instance.ModifyAsync(x => { x.Embed = builder.Build(); });
            await ReplyAsync("Case #" + caseNumber + " has been updated.");
        }

        [Command("reason")]
        [Summary("Updates reason for your last case.")]
        [Remarks(" (reason)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task ReasonAsync([Remainder] string reason)
        {
            var modChannelId = GetModLogChannelId();
            if (modChannelId <= 0) return;
            var channel = Context.Guild.GetChannel(modChannelId.ToUlong()) as ISocketMessageChannel;

            var gIconUrl = Context.GetGuildIconUrl();
            var gName = Context.GetGuildName();
            var gUrl = Context.GetGuildUrl();
            var gColor = Context.GetGuildColor();
            var gThumbnail = Context.GetGuildThumbNail();
            var gFooter = Context.GetGuildFooter();
            string value = null;
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithColor(gColor)
                .WithThumbnailUrl(gThumbnail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter[0])
                    .WithText(gFooter[1]))
                .WithTimestamp(DateTime.UtcNow);
            var actionUserName = Context.Guild.GetUser(ActionResult.UserId.ToUlong()).Mention;
            if (ActionResult.CommandName.Equals("mute"))
                value = "User : " + actionUserName + "(" + ActionResult.UserId + ")\n" + "Moderator : " +
                        Context.User.Username + "(" + Context.User.Id + ")\n" +
                        "Length : " + ActionResult.TimeLength + "minutes" + "\n" +
                        "Reason : " + reason + " (edited)";
            else
                value = "User : " + actionUserName + "(" + ActionResult.UserId + ")\n" + "Moderator : " +
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