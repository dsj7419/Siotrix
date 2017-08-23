using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    public class WarningService : IService
    {
        private readonly DiscordSocketClient _client;
        private string _badWords;
        private EmbedBuilder _builder;
        private int _count;
        private IUserMessage _logMsg;

        public WarningService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task StartAsync()
        {
            _client.MessageReceived += OnMessageReceivedAsync;
            await PrettyConsole.LogAsync("Info", "Warning", "Service started successfully").ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            await PrettyConsole.LogAsync("Info", "Warning", "Service stopped successfully").ConfigureAwait(false);
        }

        private async Task OnMessageReceivedAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            var channel = message.Channel as SocketGuildChannel;
            var user = message.Author as SocketGuildUser;

            if (user.GetPermissions(channel).ManageMessages)
                return;

            var context = new SocketCommandContext(_client, message);
            var dictionary = LogChannelExtensions.GetFilterWords(context.Guild.Id.ToLong());
            var words = msg.Content.Split(' ');
            LogChannelExtensions.IsUsableLogChannel(context.Guild.Id.ToLong());
            var badword = LogChannelExtensions.ParseMessages(words, dictionary);
            if (badword != null)
            {
                _count = 1;
                _badWords += badword + " ";
                var warnCount = GetWarnValue(context.Guild.Id.ToLong(), 1);
                var warnMuteTime = GetWarnValue(context.Guild.Id.ToLong(), 2);
                var modChannel =
                    context.Guild.GetChannel(LogChannelExtensions.ModlogchannelId.ToUlong()) as ISocketMessageChannel;

                //if (count == warn_count)
                //{
                //    builder = GetBuilder(context, warn_count, warn_count);
                //    log_msg = await MessageExtensions.SendMessageSafeAsync(mod_channel, "", false, builder.Build());
                //    return;
                //}
                //else if (count > warn_count)
                //{
                //    await MuteWarnUser(context.User as IGuildUser, warn_mute_time, context);
                //    builder = GetBuilder(context, warn_count, count);
                //    await log_msg.ModifyAsync(x => { x.Embed = builder.Build(); });
                //    return;
                //}
                if (_count == 1)
                {
                    _builder = await GetBuilder(context, 1, badword);
                    _logMsg = await modChannel.SendMessageSafeAsync("", false, _builder.Build());
                }
                else
                {
                    if (_count > warnCount)
                        await MuteWarnUser(context.User as IGuildUser, warnMuteTime, context);
                    _builder = await GetBuilder(context, _count, _badWords);
                    await _logMsg.ModifyAsync(x => { x.Embed = _builder.Build(); });
                }
            }
        }

        private int GetWarnValue(long guildId, int optionValue)
        {
            var warnValue = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gwarns.Where(p => p.GuildId.Equals(guildId) && p.Option.Equals(optionValue));
                    if (result.Any())
                        warnValue = result.First().WarnValue;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return warnValue;
        }

        private async Task MuteWarnUser(IGuildUser user, int minutes, SocketCommandContext context)
        {
            try
            {
                if (!user.IsBot)
                    await MuteExtensions.TimedMute(user, TimeSpan.FromMinutes(minutes), minutes, context, true)
                        .ConfigureAwait(false);
                var isSave = MuteExtensions.SaveMuteUser(user, minutes);
                if (isSave)
                {
                    var caseId = context.GetCaseNumber();
                    CaseExtensions.SaveCaseDataAsync("mute", caseId, user.Id.ToLong(), context.Guild.Id.ToLong(),
                        "auto");
                }
            }
            catch
            {
                await context.Channel.SendMessageAsync("mute_error").ConfigureAwait(false);
            }
        }

        private async Task<EmbedBuilder> GetBuilder(SocketCommandContext context, int warnCount, string badword)
        {
            string value = null;
            var gIconUrl = await context.GetGuildIconUrlAsync();
            var gName = await context.GetGuildNameAsync();
            var gUrl = await context.GetGuildUrlAsync();
            var gThumbnail = await context.GetGuildThumbNailAsync();
            var gFooter = await context.GetGuildFooterAsync();
            var gPrefix = await context.GetGuildPrefixAsync();
            var embed = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl.Avatar)
                    .WithName(gName.GuildName)
                    .WithUrl(gUrl.SiteUrl))
                .WithColor(new Color(255, 0, 0))
                .WithThumbnailUrl(gThumbnail.ThumbNail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter.FooterIcon)
                    .WithText(gFooter.FooterText))
                .WithTimestamp(DateTime.UtcNow);

            value = context.User.Mention + " has been issued **" + warnCount +
                    "** warning point for breaking filter rule\n" +
                    "Reason : use of the word : ***" + badword + "***\n";

            embed
                .AddField(x =>
                {
                    x.Name = "WARNING";
                    x.Value = value;
                });
            return embed;
        }
        //        .WithIconUrl(g_icon_url)
        //        .WithAuthor(new EmbedAuthorBuilder()
        //    var embed = new EmbedBuilder()
        //    string g_prefix = PrefixExtensions.GetGuildPrefix(context);
        //    string[] g_footer = GuildEmbedFooter.GetGuildFooterAsync(context);
        //    string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNailAsync(context);
        //    string g_url = GuildEmbedUrl.GetGuildUrlAsync(context);
        //    string g_name = GuildEmbedName.GetGuildNameAsync(context);
        //    string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrlAsync(context);
        //    string value = null;
        //{

        //private EmbedBuilder GetBuilder(SocketCommandContext context, int set_warn_count, long all_warn_count)
        //        .WithName(g_name)
        //        .WithUrl(g_url))
        //        .WithColor(new Color(255, 0, 0))
        //        .WithThumbnailUrl(g_thumbnail)
        //        .WithFooter(new EmbedFooterBuilder()
        //        .WithIconUrl(g_footer[0])
        //        .WithText(g_footer[1]))
        //        .WithTimestamp(DateTime.UtcNow);

        //    value = "**Warnings Setting Limit** : " + set_warn_count.ToString() + "\n" +
        //                    "**Number of Warnings founded** : " + all_warn_count.ToString() + "\n" +
        //                    "**Note** : *" + context.User.Mention + "* number of warnings exceed!";

        //    embed
        //        .AddField(x =>
        //        {
        //            x.Name = "WARNING";
        //            x.Value = value;
        //        });
        //    return embed;
        //}
    }
}