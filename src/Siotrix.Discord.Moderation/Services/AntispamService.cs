using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    public class AntispamService : IService
    {
        private const int MaxDiff = 5; // Max difference to trigger the antispam
        private const int SingleUserTolerance = 3; // Min similar messages to begin actions against the user
        private const int MultiUserTolerance = 6; // Min similar messages to begin actions against users
        private const int ExpirySeconds = 10; // Max time between messages to be considered spam

        private readonly Dictionary<ulong, SocketUserMessage> _lastMessages = new Dictionary<ulong, SocketUserMessage>()
            ;

        private readonly DiscordSocketClient _client;
        private EmbedBuilder _builder;
        private IUserMessage _logMsg;
        private int _numberOfTheCapMsg;
        private int _numberOfTheSameMsg;
        private int _warningsOfCaps;
        private int _warningsOfRepeat;

        public AntispamService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task StartAsync()
        {
            _client.MessageReceived += OnMessageReceivedAsync;
            await PrettyConsole.LogAsync("Info", "Antispam", "Service started successfully").ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            await PrettyConsole.LogAsync("Info", "Antispam", "Service stopped successfully").ConfigureAwait(false);
        }

        private bool IsAllUpper(string input)
        {
            var regexItem = new Regex("^[a-zA-Z ]*$");
            if (regexItem.IsMatch(input))
            {
                for (var i = 0; i < input.Length; i++)
                    if (char.IsLetter(input[i]) && !char.IsUpper(input[i]))
                        return false;
                return true;
            }
            return false;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!HasManageMessages(s))
                return;
            var message = s as SocketUserMessage;
            if (message.Author.IsBot)
                return;
            if (message == null)
                return;
            var channel = message.Channel as SocketGuildChannel;
            var user = message.Author as SocketGuildUser;

            if (user.GetPermissions(channel).ManageMessages)
                return;
            var context = new SocketCommandContext(_client, message);
            LogChannelExtensions.IsUsableLogChannel(context.Guild.Id.ToLong());
            var repeatSpamValue = GetSpamValue(context.Guild.Id.ToLong(), 1);
            var capsSpamValue = GetSpamValue(context.Guild.Id.ToLong(), 4);
            var muteRepeatSpamValue = GetSpamValue(context.Guild.Id.ToLong(), 2);
            var muteCapsSpamValue = GetSpamValue(context.Guild.Id.ToLong(), 5);
            var muteRepeatTimeValue = GetSpamValue(context.Guild.Id.ToLong(), 3);
            var muteCapsTimeValue = GetSpamValue(context.Guild.Id.ToLong(), 6);
            var modChannel =
                context.Guild.GetChannel(LogChannelExtensions.ModlogchannelId.ToUlong()) as ISocketMessageChannel;

            //var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
            //if (IsAllUpper(message.Content))
            //{
            //    number_of_the_cap_msg++;
            //    if(number_of_the_cap_msg == caps_spam_value)
            //    {
            //        builder = GetBuilder(context, caps_spam_value, caps_spam_value, false);
            //        msg = await MessageExtensions.SendMessageSafeAsync(log_channel, "", false, builder.Build());
            //        return;
            //    }
            //    else if(number_of_the_cap_msg > caps_spam_value)
            //    {
            //        if (number_of_the_cap_msg == mute_caps_spam_value)
            //        {
            //            await MuteSpamUser(context.User as IGuildUser, mute_caps_time_value, context);
            //        }
            //        builder = GetBuilder(context, caps_spam_value, number_of_the_cap_msg, false);
            //        await msg.ModifyAsync(x => { x.Embed = builder.Build(); });
            //        return;
            //    }
            //}
            //else
            //{
            //    if (_lastMessages.TryGetValue(message.Author.Id, out var lastMessage)
            //    && message.Content == lastMessage.Content)
            //    {
            //        // messages are the same, do what you want
            //        number_of_the_same_msg++;
            //        if (number_of_the_same_msg == repeat_spam_value - 1)
            //        {
            //            builder = GetBuilder(context, repeat_spam_value, repeat_spam_value, true);
            //            msg = await MessageExtensions.SendMessageSafeAsync(log_channel, "", false, builder.Build());
            //            return;
            //        }
            //        else if (number_of_the_same_msg > repeat_spam_value - 1)
            //        {
            //            if(number_of_the_same_msg == mute_repeat_spam_value - 1)
            //            {
            //                await MuteSpamUser(context.User as IGuildUser, mute_repeat_time_value, context);
            //            }
            //            builder = GetBuilder(context, repeat_spam_value, number_of_the_same_msg, true);
            //            await msg.ModifyAsync(x => { x.Embed = builder.Build(); });
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        number_of_the_same_msg = 0;
            //        _lastMessages[message.Author.Id] = message;
            //    }
            //}

            //await Task.Delay(0);

            if (IsAllUpper(message.Content))
            {
                _numberOfTheCapMsg++;
                if (_numberOfTheCapMsg == capsSpamValue)
                {
                    _warningsOfCaps++;
                    if (_warningsOfCaps == muteCapsSpamValue)
                        await MuteSpamUser(context.User as IGuildUser, muteCapsTimeValue, context);
                    _builder = await GetBuilder(context, _warningsOfCaps, null, _numberOfTheCapMsg, true);
                    if (_warningsOfCaps == 1)
                        _logMsg = await modChannel.SendMessageSafeAsync("", false, _builder.Build());
                    else
                        await _logMsg.ModifyAsync(x => { x.Embed = _builder.Build(); });
                    _numberOfTheCapMsg = 0;
                }
            }
            else
            {
                if (_lastMessages.TryGetValue(message.Author.Id, out var lastMessage) &&
                    message.Content == lastMessage.Content)
                {
                    _numberOfTheSameMsg++;
                    if (_numberOfTheSameMsg == repeatSpamValue - 1)
                    {
                        _warningsOfRepeat++;
                        if (_warningsOfRepeat == muteRepeatSpamValue)
                            await MuteSpamUser(context.User as IGuildUser, muteRepeatTimeValue, context);
                        _builder = await GetBuilder(context, _warningsOfRepeat, message.Content, _numberOfTheSameMsg + 1,
                            false);
                        if (_warningsOfRepeat == 1)
                            _logMsg = await modChannel.SendMessageSafeAsync("", false, _builder.Build());
                        else
                            await _logMsg.ModifyAsync(x => { x.Embed = _builder.Build(); });
                        _numberOfTheSameMsg = 0;
                    }
                }
                else
                {
                    _numberOfTheSameMsg = 0;
                    _lastMessages[message.Author.Id] = message;
                }
            }
        }

        private async Task MuteSpamUser(IGuildUser user, int minutes, SocketCommandContext context)
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

        private async Task<EmbedBuilder> GetBuilder(SocketCommandContext context, int warnCount, string spamword, int spamCount,
            bool isCapsSpam)
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

            if (isCapsSpam)
                value = context.User.Mention + " has been issued **" + warnCount +
                        "** warning points for breaking caps spam\n" +
                        "Reason : CapsSpammed : **" + spamCount + "** times";
            else
                value = context.User.Mention + " has been issued **" + warnCount +
                        "** warning points for breaking spam threshold\n" +
                        "Reason : RepeatSpammed : ***" + spamword + "*** - **" + spamCount + "** times";

            embed
                .AddField(x =>
                {
                    x.Name = "WARNING";
                    x.Value = value;
                });
            return embed;
        }

        //private EmbedBuilder GetBuilder(SocketCommandContext context, int set_num, long spam_num, bool is_repeat_message)
        //{
        //    string value = null;
        //    string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrlAsync(context);
        //    string g_name = GuildEmbedName.GetGuildNameAsync(context);
        //    string g_url = GuildEmbedUrl.GetGuildUrlAsync(context);
        //    string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNailAsync(context);
        //    string[] g_footer = GuildEmbedFooter.GetGuildFooterAsync(context);
        //    string g_prefix = PrefixExtensions.GetGuildPrefix(context);
        //    var embed = new EmbedBuilder()
        //        .WithAuthor(new EmbedAuthorBuilder()
        //        .WithIconUrl(g_icon_url)
        //        .WithName(g_name)
        //        .WithUrl(g_url))
        //        .WithColor(new Color(255, 0, 0))
        //        .WithThumbnailUrl(g_thumbnail)
        //        .WithFooter(new EmbedFooterBuilder()
        //        .WithIconUrl(g_footer[0])
        //        .WithText(g_footer[1]))
        //        .WithTimestamp(DateTime.UtcNow);
        //    if (!is_repeat_message)
        //    {
        //        value = "**Caps Setting Limit** : " + set_num.ToString() + "\n" +
        //                        "**Number of Caps Messages founded** : " + spam_num.ToString() + "\n" +
        //                        "**Note** : *" + context.User.Mention + "* caps spam attack!";
        //    }
        //    else
        //    {
        //        value = "**Repeat Setting Limit** : " + set_num.ToString() + "\n" +
        //                        "**Number of Repeat Messages founded** : " + spam_num.ToString() + "\n" +
        //                        "**Note** : *" + context.User.Mention + "* spamming attack!";
        //    }
        //    embed
        //        .AddField(x =>
        //        {
        //            x.Name = "Violation";
        //            x.Value = value;
        //        });
        //    return embed;
        //}

        private bool HasManageMessages(SocketMessage s)
        {
            var channel = s.Channel as SocketTextChannel;
            if (channel == null)
                return false;
            var permissions = channel.Guild.CurrentUser.GetPermissions(channel);
            if (!permissions.ManageMessages)
                return false;
            return true;
        }

        private int GetSpamValue(long guildId, int optionValue)
        {
            var spamValue = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gspams.Where(p => p.GuildId.Equals(guildId) && p.Option.Equals(optionValue));
                    if (result.Any())
                        spamValue = result.First().SpamValue;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return spamValue;
        }
    }
}