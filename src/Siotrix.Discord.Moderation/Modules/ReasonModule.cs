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

        [Command("reason")]
        [Summary("Reason")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task ReasonAsync(int case_number, [Remainder]string reason)
        {
            string[] data = ActionResult.Content.Split(',');
            if (!case_number.ToString().Equals(data[1]))
            {
                await ReplyAsync("Wrong Number Try again!");
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
            
            builder
                .AddField(x =>
                {
                    x.Name = "Case " + "#" + data[1].ToString() + " | " + data[0];
                    x.Value = "User : " + data[2] + "\n" + "Moderator : " + data[3] + "\n" + "Reason : " + reason + " (edited)";
                });

            await channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("reason")]
        [Summary("Reason")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task ReasonAsync(string reason)
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
            string[] data = ActionResult.Content.Split(',');
            builder
                .AddField(x =>
                {
                    x.Name = "Case " + "#" + data[1].ToString() + " | " + data[0];
                    x.Value = "User : " + data[2] + "\n" + "Moderator : " + data[3] + "\n" + "Reason : " + reason + " (edited)";
                });

            await channel.SendMessageAsync("", false, builder.Build());
        }
    }
}
