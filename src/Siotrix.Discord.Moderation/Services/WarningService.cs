using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Addons.InteractiveCommands;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Siotrix.Discord.Moderation
{
    public class WarningService : IService
    {
        private DiscordSocketClient _client;
        private int count = 0;
        private IUserMessage log_msg;
        private EmbedBuilder builder = null;
        string bad_words = null;

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
            var context = new SocketCommandContext(_client, message);
            var dictionary = LogChannelExtensions.GetFilterWords(context.Guild.Id.ToLong());
            string[] words = msg.Content.Split(' ');
            LogChannelExtensions.IsUsableLogChannel(context.Guild.Id.ToLong());
            var badword = LogChannelExtensions.ParseMessages(words, dictionary);
            if (badword != null)
            {
                count++;
                bad_words += badword + " ";
                var warn_count = GetWarnValue(context.Guild.Id.ToLong(), 1);
                var warn_mute_time = GetWarnValue(context.Guild.Id.ToLong(), 2);
                var mod_channel = context.Guild.GetChannel(LogChannelExtensions.modlogchannel_id.ToUlong()) as ISocketMessageChannel;

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
                if(count == 1)
                {
                    builder = GetBuilder(context, 1, badword);
                    log_msg = await MessageExtensions.SendMessageSafeAsync(mod_channel, "", false, builder.Build());
                    return;
                }
                else
                {
                    if(count > warn_count)
                        await MuteWarnUser(context.User as IGuildUser, warn_mute_time, context);
                    builder = GetBuilder(context, count, bad_words);
                    await log_msg.ModifyAsync(x => { x.Embed = builder.Build(); });
                    return;
                }
            }
        }

        private int GetWarnValue(long guild_id, int option_value)
        {
            int warn_value = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gwarns.Where(p => p.GuildId.Equals(guild_id) && p.Option.Equals(option_value));
                    if (result.Any())
                    {
                        warn_value = result.First().WarnValue;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return warn_value;
        }

        private async Task MuteWarnUser(IGuildUser user, int minutes, SocketCommandContext context)
        {
            try
            {
                if (!user.IsBot)
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

        private EmbedBuilder GetBuilder(SocketCommandContext context, int warn_count, string badword)
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

            value = context.User.Mention + " has been issued **" + warn_count.ToString() + "** warning points for breaking filter rule\n" +
              "Reason : use of the words : ***" + badword + "***\n";

            embed
                .AddField(x =>
                {
                    x.Name = "WARNING";
                    x.Value = value;
                });
            return embed;
        }

        //private EmbedBuilder GetBuilder(SocketCommandContext context, int set_warn_count, long all_warn_count)
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
