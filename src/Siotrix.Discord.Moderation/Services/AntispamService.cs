using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    public class AntispamService : IService
    {
        private const int _maxDiff = 5;             // Max difference to trigger the antispam
        private const int _singleUserTolerance = 3; // Min similar messages to begin actions against the user
        private const int _multiUserTolerance = 6;  // Min similar messages to begin actions against users
        private const int _expirySeconds = 10;      // Max time between messages to be considered spam
        private int number_of_the_same_msg = 0;
        private int number_of_the_cap_msg = 0;
        private IUserMessage msg;
        private EmbedBuilder builder = null;
        private int warnings_of_repeat = 0;
        private int warnings_of_caps = 0;
        private IUserMessage log_msg;

        private DiscordSocketClient _client;
        private readonly Dictionary<ulong, SocketUserMessage> _lastMessages = new Dictionary<ulong, SocketUserMessage>();

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
                for (int i = 0; i < input.Length; i++)
                {
                    if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                        return false;
                }
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
            var context = new SocketCommandContext(_client, message);
            LogChannelExtensions.IsUsableLogChannel(context.Guild.Id.ToLong());
            var repeat_spam_value = GetSpamValue(context.Guild.Id.ToLong(), 1);
            var caps_spam_value = GetSpamValue(context.Guild.Id.ToLong(), 4);
            var mute_repeat_spam_value = GetSpamValue(context.Guild.Id.ToLong(), 2);
            var mute_caps_spam_value = GetSpamValue(context.Guild.Id.ToLong(), 5);
            var mute_repeat_time_value = GetSpamValue(context.Guild.Id.ToLong(), 3);
            var mute_caps_time_value = GetSpamValue(context.Guild.Id.ToLong(), 6);
            var mod_channel = context.Guild.GetChannel(LogChannelExtensions.modlogchannel_id.ToUlong()) as ISocketMessageChannel;

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
                number_of_the_cap_msg++;
                if (number_of_the_cap_msg == caps_spam_value)
                {
                    warnings_of_caps++;
                    if(warnings_of_caps == mute_caps_spam_value)
                        await MuteSpamUser(context.User as IGuildUser, mute_caps_time_value, context);
                    builder = GetBuilder(context, warnings_of_caps, null, number_of_the_cap_msg, true);
                    if(warnings_of_caps == 1)
                        log_msg = await MessageExtensions.SendMessageSafeAsync(mod_channel, "", false, builder.Build());
                    else
                        await log_msg.ModifyAsync(x => { x.Embed = builder.Build(); });
                    number_of_the_cap_msg = 0;
                    return;
                }
            }
            else
            {
                if(_lastMessages.TryGetValue(message.Author.Id, out var lastMessage) && message.Content == lastMessage.Content)
                {
                    number_of_the_same_msg++;
                    if(number_of_the_same_msg == repeat_spam_value - 1)
                    {
                        warnings_of_repeat++;
                        if(warnings_of_repeat == mute_repeat_spam_value)
                            await MuteSpamUser(context.User as IGuildUser, mute_repeat_time_value, context);
                        builder = GetBuilder(context, warnings_of_repeat, message.Content, number_of_the_same_msg + 1, false);
                        if(warnings_of_repeat == 1)
                            log_msg = await MessageExtensions.SendMessageSafeAsync(mod_channel, "", false, builder.Build());
                        else
                            await log_msg.ModifyAsync(x => { x.Embed = builder.Build(); });
                        number_of_the_same_msg = 0;
                        return;
                    }
                }
                else
                {
                    number_of_the_same_msg = 0;
                    _lastMessages[message.Author.Id] = message;
                }
            }
        }

        private async Task MuteSpamUser(IGuildUser user, int minutes, SocketCommandContext context)
        {
            try
            {
                if(!user.IsBot)
                    await MuteExtensions.TimedMute(user, TimeSpan.FromMinutes(minutes), minutes, context, true).ConfigureAwait(false);
                var is_save = MuteExtensions.SaveMuteUser(user, minutes);
                if (is_save)
                {
                    var case_id = CaseExtensions.GetCaseNumber(context);
                    CaseExtensions.SaveCaseDataAsync("mute", case_id, user.Id.ToLong(), context.Guild.Id.ToLong(), "auto");
                }
            }
            catch
            {
                await context.Channel.SendMessageAsync("mute_error").ConfigureAwait(false);
            }
        }

        private EmbedBuilder GetBuilder(SocketCommandContext context, int warn_count, string spamword, int spam_count, bool is_caps_spam)
        {
            string value = null;
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(context);
            string g_name = GuildEmbedName.GetGuildName(context);
            string g_url = GuildEmbedUrl.GetGuildUrl(context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(context);
            string g_prefix = PrefixExtensions.GetGuildPrefix(context);
            var embed = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithColor(new Color(255, 0, 0))
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);

            if (is_caps_spam)
                value = context.User.Mention + " has been issued **" + warn_count.ToString() + "** warning points for breaking caps spam\n" +
              "Reason : CapsSpammed : **" + spam_count.ToString() + "** times";
            else
                value = context.User.Mention + " has been issued **" + warn_count.ToString() + "** warning points for breaking spam threshold\n" +
              "Reason : RepeatSpammed : ***" + spamword + "*** - **" + spam_count.ToString() + "** times";

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
        //    string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(context);
        //    string g_name = GuildEmbedName.GetGuildName(context);
        //    string g_url = GuildEmbedUrl.GetGuildUrl(context);
        //    string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(context);
        //    string[] g_footer = GuildEmbedFooter.GetGuildFooter(context);
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

        private int GetSpamValue(long guild_id, int option_value)
        {
            int spam_value = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gspams.Where(p => p.GuildId.Equals(guild_id) && p.Option.Equals(option_value));
                    if (result.Any())
                    {
                        spam_value = result.First().SpamValue;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return spam_value;
        }
    }
}
