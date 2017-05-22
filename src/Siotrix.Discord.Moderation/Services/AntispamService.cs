using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    public class AntispamService : IService
    {
        private const int _maxDiff = 5;             // Max difference to trigger the antispam
        private const int _singleUserTolerance = 3; // Min similar messages to begin actions against the user
        private const int _multiUserTolerance = 6;  // Min similar messages to begin actions against users
        private const int _expirySeconds = 10;      // Max time between messages to be considered spam
        private long number_of_the_same_msg = 0;
        private long number_of_the_cap_msg = 0;
        private IUserMessage msg;
        private EmbedBuilder builder = null;

        private DiscordSocketClient _client;
        private readonly Dictionary<ulong, SocketUserMessage> _lastMessages = new Dictionary<ulong, SocketUserMessage>();

        public AntispamService(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task StartAsync()
        {
            _client.MessageReceived += OnMessageReceivedAsync;
            return Task.CompletedTask;
        }
        
        public Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            return Task.CompletedTask;
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
            var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;

            if (message.Content.Cap(message.Content.Length).Equals(message.Content))
            {
                number_of_the_cap_msg++;
                if(number_of_the_cap_msg == caps_spam_value)
                {
                    builder = GetBuilder(context, caps_spam_value, caps_spam_value, false);
                    msg = await MessageExtensions.SendMessageSafeAsync(log_channel, "", false, builder.Build());
                    return;
                }
                else if(number_of_the_cap_msg > caps_spam_value)
                {
                    builder = GetBuilder(context, caps_spam_value, number_of_the_cap_msg, false);
                    await msg.ModifyAsync(x => { x.Embed = builder.Build(); });
                    return;
                }
            }
            else
            {
                if (_lastMessages.TryGetValue(message.Author.Id, out var lastMessage)
                && message.Content == lastMessage.Content)
                {
                    // messages are the same, do what you want
                    number_of_the_same_msg++;
                    if (number_of_the_same_msg == repeat_spam_value)
                    {
                        builder = GetBuilder(context, repeat_spam_value, repeat_spam_value, true);
                        msg = await MessageExtensions.SendMessageSafeAsync(log_channel, "", false, builder.Build());
                        return;
                    }
                    else if (number_of_the_same_msg > repeat_spam_value)
                    {
                        builder = GetBuilder(context, repeat_spam_value, number_of_the_same_msg, true);
                        await msg.ModifyAsync(x => { x.Embed = builder.Build(); });
                        return;
                    }
                }
                else
                {
                    number_of_the_same_msg = 0;
                    _lastMessages[message.Author.Id] = message;
                }
            }
            
            await Task.Delay(0);
        }

        private EmbedBuilder GetBuilder(SocketCommandContext context, int set_num, long spam_num, bool is_repeat_message)
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
            if (!is_repeat_message)
            {
                value = "**Caps Setting Limit** : " + set_num.ToString() + "\n" +
                                "**Number of Caps Messages founded** : " + spam_num.ToString() + "\n" +
                                "**Note** : *" + context.User.Mention + "* keep to try caps spam attack!";
            }
            else
            {
                value = "**Repeat Setting Limit** : " + set_num.ToString() + "\n" +
                                "**Number of Repeat Messages founded** : " + spam_num.ToString() + "\n" +
                                "**Note** : *" + context.User.Mention + "* keep to try spam attack!";
            }
            embed
                .AddField(x =>
                {
                    x.Name = "Violation";
                    x.Value = value;
                });
            return embed;
        }

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
