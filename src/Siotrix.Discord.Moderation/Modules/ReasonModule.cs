using System;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;

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

        private async Task SaveCaseDataAsync(string cmd_name, long case_num, long user_id, long guild_id, string reason)
        {
            using (var db = new LogDatabase())
            {
                try
                {
                    var exist_data = db.Casenums.Where(x => x.GuildId.Equals(guild_id) && x.GCaseNum.Equals(case_num) && x.UserId.Equals(user_id) && x.CmdName.Equals(cmd_name));
                    if (exist_data.Any())
                    {
                        var data = exist_data.First();
                        data.Reason = reason;
                        db.Casenums.Update(data);
                    }
                    else
                    {
                        var record = new DiscordCaseNum();
                        record.GCaseNum = case_num;
                        record.GuildId = guild_id;
                        record.UserId = user_id;
                        record.CmdName = cmd_name;
                        record.Reason = reason;
                        db.Casenums.Add(record);
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        [Command("reason")]
        [Summary("Reason")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task ReasonAsync(long case_number, [Remainder]string reason)
        {
            if (!case_number.Equals(ActionResult.CaseId))
            {
                await ReplyAsync("Wrong Number. Try again!");
                return;
            }
                
            var mod_channel_id = GetModLogChannelId();
            if (mod_channel_id <= 0) return;
            var channel = Context.Guild.GetChannel(mod_channel_id.ToUlong()) as ISocketMessageChannel;

            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
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
            builder
                .AddField(x =>
                {
                    x.Name = "Case " + "#" + case_number.ToString() + " | " + ActionResult.CommandName;
                    x.Value = "User : " + action_user_name + "(" + ActionResult.UserId + ")\n" + "Moderator : " + Context.User.Username + "(" + Context.User.Id.ToString() + ")\n" + "Reason : " + reason + " (edited)";
                });
            await SaveCaseDataAsync(ActionResult.CommandName, ActionResult.CaseId, ActionResult.UserId, Context.Guild.Id.ToLong(), reason);
            await ActionResult.Instance.ModifyAsync(x => { x.Embed = builder.Build(); });
            await ReplyAsync("Case #" + case_number.ToString() + " has been updated.");
        }

        [Command("reason")]
        [Summary("Reason")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task ReasonAsync([Remainder]string reason)
        {
            var mod_channel_id = GetModLogChannelId();
            if (mod_channel_id <= 0) return;
            var channel = Context.Guild.GetChannel(mod_channel_id.ToUlong()) as ISocketMessageChannel;

            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
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
            builder
                .AddField(x =>
                {
                    x.Name = "Case " + "#" + ActionResult.CaseId.ToString() + " | " + ActionResult.CommandName;
                    x.Value = "User : " + action_user_name + "(" + ActionResult.UserId + ")\n" + "Moderator : " + Context.User.Username + "(" + Context.User.Id.ToString() + ")\n" + "Reason : " + reason + " (edited)";
                });
            await SaveCaseDataAsync(ActionResult.CommandName, ActionResult.CaseId, ActionResult.UserId, Context.Guild.Id.ToLong(), reason);
            await ActionResult.Instance.ModifyAsync(x => { x.Embed = builder.Build(); });
            await ReplyAsync("Case #" + ActionResult.CaseId.ToString() + " has been updated.");
        }
    }
}
