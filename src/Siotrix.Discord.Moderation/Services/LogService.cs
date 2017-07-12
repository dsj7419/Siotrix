using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    public class LogService : IService
    {
        //var channel = context.Guild.GetChannel(data.ChannelId.ToUlong()) as ISocketMessageChannel;
        
        private DiscordSocketClient _client;
        private LogDatabase _db;

        public LogService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task StartAsync()
        {
            _db = new LogDatabase();
            _client.MessageReceived += OnMessageReceivedAsync;
            _client.MessageUpdated += OnMesssageUpdatedAsync;
            _client.MessageDeleted += OnMessageDeletedAsync;
            _client.ReactionAdded += OnReactionAddedAsync;
            _client.ReactionRemoved += OnReactionRemovedAsync;
            _client.ReactionsCleared += OnReactionsClearedAsync;
            _client.UserJoined += OnUserJoinedAsync;
            _client.RoleCreated += OnRoleCreatedAsync;
            _client.RoleUpdated += OnRoleUpdatedAsync;
            _client.RoleDeleted += OnRoleDeletedAsync;
            _client.UserUnbanned += OnUserUnBannedAsync;
            _client.GuildMemberUpdated += GuildMemberUpdated_RoleChange;
            //_client.GuildMemberUpdated += UserUpdated_NameChange;
            _client.GuildMemberUpdated += GuildMemberUpdated_NickChange;
            _client.UserLeft += OnUserLeftAsync;
            _client.UserUpdated += UserNameChangedAsync;
            MuteExtensions.UserMuted += OnMuteReceivedAsync;
            MuteExtensions.UserUnmuted += OnUnMuteReceivedAsync;
            await PrettyConsole.LogAsync("Info", "Log", "Service started successfully").ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            _client.MessageUpdated -= OnMesssageUpdatedAsync;
            _client.MessageDeleted -= OnMessageDeletedAsync;
            _client.ReactionAdded -= OnReactionAddedAsync;
            _client.ReactionRemoved -= OnReactionRemovedAsync;
            _client.ReactionsCleared -= OnReactionsClearedAsync;
            _client.UserJoined -= OnUserJoinedAsync;
            _client.RoleCreated -= OnRoleCreatedAsync;
            _client.RoleUpdated -= OnRoleUpdatedAsync;
            _client.RoleDeleted -= OnRoleDeletedAsync;
            _client.UserUnbanned -= OnUserUnBannedAsync;
            _client.GuildMemberUpdated -= GuildMemberUpdated_RoleChange;
            //_client.GuildMemberUpdated -= UserUpdated_NameChange;
            _client.GuildMemberUpdated -= GuildMemberUpdated_NickChange;
            _client.UserUpdated -= UserNameChangedAsync;
            _client.UserLeft -= OnUserLeftAsync;
            MuteExtensions.UserMuted -= OnMuteReceivedAsync;
            MuteExtensions.UserUnmuted -= OnUnMuteReceivedAsync;
            _db = null;

            await PrettyConsole.LogAsync("Info", "Log", "Service stopped successfully").ConfigureAwait(false);
        }

        private async void OnUnMuteReceivedAsync(IGuildUser user, MuteExtensions.MuteType muteType, SocketCommandContext context, bool is_auto)
        {
            try
            {
                long case_id = 0;
                var guild = user.Guild as SocketGuild;
                string unmute_data = null;
                LogChannelExtensions.IsUsableLogChannel(guild.Id.ToLong());
                var channel = guild.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
                var mod_channel = guild.GetChannel(LogChannelExtensions.modlogchannel_id.ToUlong()) as ISocketMessageChannel;

                var unmutes = "";
                switch (muteType)
                {
                    case MuteExtensions.MuteType.Voice:
                        unmutes = "voice";
                        break;
                    case MuteExtensions.MuteType.Chat:
                        unmutes = "text";
                        break;
                    case MuteExtensions.MuteType.All:
                        unmutes = "all";
                        break;
                }
                if (is_auto)
                    unmute_data = user.Username + "#" + user.Discriminator + " has been auto " + unmutes + " unmuted.";
                else
                    unmute_data = user.Username + "#" + user.Discriminator + " has been " + unmutes + " unmuted by " + context.User.Username + "#" + context.User.Discriminator + ".";
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName(unmute_data))
                .WithColor(new Color(127, 255, 127));
                if (LogChannelExtensions.is_toggled_log)
                    await channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                else
                    await channel.SendMessageAsync(user.Mention, false, builder.Build());

                string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(context);
                string g_name = GuildEmbedName.GetGuildName(context);
                string g_url = GuildEmbedUrl.GetGuildUrl(context);
                string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(context);
                string[] g_footer = GuildEmbedFooter.GetGuildFooter(context);
                string g_prefix = PrefixExtensions.GetGuildPrefix(context);
                string value = null;
                var mod_builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(g_icon_url)
                    .WithName(g_name)
                    .WithUrl(g_url))
                    .WithColor(new Color(127, 255, 0))
                    .WithThumbnailUrl(g_thumbnail)
                    .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(g_footer[0])
                    .WithText(g_footer[1]))
                    .WithTimestamp(DateTime.UtcNow);
                case_id = CaseExtensions.GetCaseNumber(context);
                
                if (is_auto)
                {
                    value = "User : " + user.Mention + " (" + user.Id.ToString() + ")" + "\n" + "Moderator : " +
                                  context.Guild.CurrentUser.Mention + "\n" +
                                  "Reason : auto";
                }
                else
                {
                    value = "User : " + user.Mention + " (" + user.Id.ToString() + ")" + "\n" + "Moderator : " +
                                  context.User.Username + " (" + context.User.Id.ToString() + ")" + "\n" +
                                  "Reason : Type " + g_prefix + "reason " + case_id + "<reason> to add it.";
                }
                mod_builder
                    .AddField(x =>
                    {
                        x.Name = "Case #" + case_id + " | unmute";
                        x.Value = value;
                    });
                if (LogChannelExtensions.is_toggled_modlog)
                    await mod_channel.SendMessageAsync($"📣 : You can not see mod-log datas because this channel has been **toggled off** !");
                else
                {
                    IUserMessage msg_instance = await MessageExtensions.SendMessageSafeAsync(mod_channel, "", false, mod_builder.Build());
                    ActionResult.CommandName = "unmute";
                    ActionResult.CaseId = case_id;
                    ActionResult.UserId = user.Id.ToLong();
                    ActionResult.Instance = msg_instance;
                    ActionResult.IsFoundedCaseNumber = true;
                    Console.WriteLine("Service-Unmute +++++++++++++++++++++++++++++++{0}", case_id);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void OnMuteReceivedAsync(IGuildUser user, MuteExtensions.MuteType muteType, SocketCommandContext context, int minutes, bool is_auto)
        {
            try
            {
                long case_id = 0;
                var guild = user.Guild as SocketGuild;
                string mute_data = null;
                string value = null;
                LogChannelExtensions.IsUsableLogChannel(guild.Id.ToLong());
                var channel = guild.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
                var mod_channel = guild.GetChannel(LogChannelExtensions.modlogchannel_id.ToUlong()) as ISocketMessageChannel;

                var mutes = "";
                switch (muteType)
                {
                    case MuteExtensions.MuteType.Voice:
                        mutes = "voice";
                        break;
                    case MuteExtensions.MuteType.Chat:
                        mutes = "text";
                        break;
                    case MuteExtensions.MuteType.All:
                        mutes = "all";
                        break;
                }

                if (is_auto)
                    mute_data = user.Username + "#" + user.Discriminator + " has been auto " + mutes + " muted.";
                else
                    mute_data = user.Username + "#" + user.Discriminator + " has been " + mutes + " muted by " + context.User.Username + "#" + context.User.Discriminator + ".";
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName(mute_data))
                .WithColor(new Color(127, 255, 0));
                if (LogChannelExtensions.is_toggled_log)
                    await channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                else
                    await channel.SendMessageAsync(user.Mention, false, builder.Build());

                string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(context);
                string g_name = GuildEmbedName.GetGuildName(context);
                string g_url = GuildEmbedUrl.GetGuildUrl(context);
                string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(context);
                string[] g_footer = GuildEmbedFooter.GetGuildFooter(context);
                string g_prefix = PrefixExtensions.GetGuildPrefix(context);
                var mod_builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(g_icon_url)
                    .WithName(g_name)
                    .WithUrl(g_url))
                    .WithColor(new Color(127, 255, 0))
                    .WithThumbnailUrl(g_thumbnail)
                    .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(g_footer[0])
                    .WithText(g_footer[1]))
                    .WithTimestamp(DateTime.UtcNow);
                case_id = CaseExtensions.GetCaseNumber(context);

                if (is_auto)
                {
                    value = "User : " + user.Mention + " (" + user.Id.ToString() + ")" + "\n" + "Moderator : " +
                                  context.Guild.CurrentUser.Mention + "\n" +
                                  "Length : " + minutes.ToString() + "minutes" + "\n" +
                                  "Reason : auto";
                }
                else
                {
                    value = "User : " + user.Mention + " (" + user.Id.ToString() + ")" + "\n" + "Moderator : " +
                                  context.User.Username + " (" + context.User.Id.ToString() + ")" + "\n" +
                                  "Length : " + minutes.ToString() + "minutes" + "\n" +
                                  "Reason : Type " + g_prefix + "reason " + case_id + "<reason> to add it.";
                }
                mod_builder
                    .AddField(x =>
                    {
                        x.Name = "Case #" + case_id + " | mute";
                        x.Value = value;
                    });
                if (LogChannelExtensions.is_toggled_modlog)
                    await mod_channel.SendMessageAsync($"📣 : You can not see mod-log datas because this channel has been **toggled off** !");
                else
                {
                    IUserMessage msg_instance = await MessageExtensions.SendMessageSafeAsync(mod_channel, "", false, mod_builder.Build());
                    ActionResult.CommandName = "mute";
                    ActionResult.CaseId = case_id;
                    ActionResult.UserId = user.Id.ToLong();
                    ActionResult.TimeLength = minutes;
                    ActionResult.Instance = msg_instance;
                    Console.WriteLine("Service-Mute ------------------{0}", case_id);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private long GetLogChannelId(long guild_id)
        {
            long id = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Glogchannels.Where(p => p.GuildId.Equals(guild_id));
                    if(data.Any() || data.Count() > 0) 
                        id = data.First().ChannelId;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return id;
        }

        private long GetAnnounceChannelId(long guild_id)
        {
            long channel_id = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncechannels.Where(p => p.GuildId == guild_id);
                    if (list.Any())
                    {
                        channel_id = list.First().ChannelId;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return channel_id;
        }

        private string getAction(string str)
        {
            string sentense = null;
            switch (str)
            {
                case "kick":
                    sentense = "kicked";
                    break;
                case "ban":
                    sentense = "banned";
                    break;
               /* case "mute":
                    sentense = "muted";
                    break;
                case "unmute":
                    sentense = "unmuted";
                    break;*/
                default:
                    break;
            }
            return sentense;
        }

        private Color getActionColor(string str)
        {
            Color color = new Color();
            switch (str)
            {
                case "kick":
                    color = new Color(127, 127, 255);
                    break;
                case "ban":
                    color = new Color(127, 0, 255);
                    break;
                /*case "mute":
                    color = new Color(127, 255, 127);
                    break;
                case "unmute":
                    color = new Color(0, 255, 127);
                    break;*/
                default:
                    break;
            }
            return color;
        }

        private long GetModLogChannelId(SocketCommandContext context)
        {
            long id = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Gmodlogchannels.Where(p => p.GuildId.Equals(context.Guild.Id.ToLong()));
                    if(data.Any() || data.Count() > 0)
                        id = data.First().ChannelId;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return id;
        }

        private long GetCaseNumberAync(string cmdName, SocketCommandContext context, SocketGuildUser user)
        {
            long case_id = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Casenums.Where(x => x.GuildId.Equals(context.Guild.Id.ToLong()));
                    if (!data.Any())
                        case_id = 1;
                    else
                        case_id = data.Last().GCaseNum + 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return case_id;
        }

        private bool CheckGuildUser(long userId, long guildId)
        {
            bool isFound = false;
            using(var db = new LogDatabase())
            {
                try
                {
                    var data = db.Messages.Where(x => x.AuthorId == userId && x.GuildId == guildId);
                    if (data.Any())
                        isFound = true;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isFound;
        }

        private async Task OnRoleDeletedAsync(SocketRole role)
        {
            var channel_id = GetLogChannelId(role.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(channel_id.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder()
            .WithAuthor(new EmbedAuthorBuilder()
            .WithName("Role has been deleted."))
            .WithColor(new Color(255, 0, 0));
            await log_channel.SendMessageAsync("", false, builder.Build());
        }

        private async Task OnRoleUpdatedAsync(SocketRole role, SocketRole update_role)
        {
            var channel_id = GetLogChannelId(role.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(channel_id.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder()
            .WithAuthor(new EmbedAuthorBuilder()
            .WithName("Role has been updated."))
            .WithColor(new Color(255, 127, 255));
            await log_channel.SendMessageAsync("", false, builder.Build());
        }

        private async Task OnRoleCreatedAsync(SocketRole role)
        {
            var channel_id = GetLogChannelId(role.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(channel_id.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder()
            .WithAuthor(new EmbedAuthorBuilder()
            .WithName("Role has been created."))
            .WithColor(new Color(127, 255, 0));
            await log_channel.SendMessageAsync("", false, builder.Build());
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            try
            {
                if (!ActionResult.IsSuccess) return;
                var msg = message as SocketUserMessage;
                var context = new SocketCommandContext(_client, msg);
                int argPos = 0;
                string spec = null;
                string content = null;
                long case_id = 0;

                LogChannelExtensions.IsUsableLogChannel(context.Guild.Id.ToLong());
                var channel = context.Guild.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
                var mod_channel = context.Guild.GetChannel(LogChannelExtensions.modlogchannel_id.ToUlong()) as ISocketMessageChannel;

                spec = PrefixExtensions.GetGuildPrefix(context);
                if (message.Author.IsBot
                    || msg == null
                    || !msg.Content.Except("?").Any()
                    || msg.Content.Trim().Length <= 1
                    || msg.Content.Trim()[1] == '?'
                    || (!(msg.HasStringPrefix(spec, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))))
                    return;
                if (msg.HasStringPrefix(spec, ref argPos))
                {
                    content = MessageParser.ParseStringPrefix(msg, spec);
                }
                else if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                {
                    content = MessageParser.ParseMentionPrefix(msg);
                }
                if (msg.HasStringPrefix(spec, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                {
                    string[] words = content.Split(' ');
                    var action = getAction(words[0]);
                    if (action == null) return; // If action is not kick or ban or mute command, not display log information.
                    var id = MentionUtils.ParseUser(words[1]);
                    var user = _client.GetUser(id);
                    var user_identifier = user.Username + "#" + user.Discriminator;
                    var user_mention = user.Mention;
                    var mod_identifier = context.User.Username + "#" + context.User.Discriminator;
                    
                    var action_color = getActionColor(words[0]);

                    var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(user.GetAvatarUrl())
                    .WithName(user_identifier + " has been " + action + " by " + mod_identifier))
                    .WithColor(action_color);
                    if (LogChannelExtensions.is_toggled_log)
                        await channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                    else
                        await channel.SendMessageAsync(user_mention, false, builder.Build());

                    string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(context);
                    string g_name = GuildEmbedName.GetGuildName(context);
                    string g_url = GuildEmbedUrl.GetGuildUrl(context);
                    string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(context);
                    string[] g_footer = GuildEmbedFooter.GetGuildFooter(context);
                    string g_prefix = PrefixExtensions.GetGuildPrefix(context);
                    var mod_builder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(g_icon_url)
                        .WithName(g_name)
                        .WithUrl(g_url))
                        .WithColor(action_color)
                        .WithThumbnailUrl(g_thumbnail)
                        .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(g_footer[0])
                        .WithText(g_footer[1]))
                        .WithTimestamp(DateTime.UtcNow);
                    //case_id = GetCaseNumberAync(words[0], context, user as SocketGuildUser);
                    case_id = CaseExtensions.GetCaseNumber(context);
                    mod_builder
                        .AddField(x =>
                        {
                            x.Name = "Case #" + case_id + " | " + words[0];
                            x.Value = "User : " + user.Mention + " (" + user.Id.ToString() + ")" + "\n" + "Moderator : " +
                                      context.User.Username + " (" + context.User.Id.ToString() + ")" + "\n" +
                                      "Reason : Type " + g_prefix + "reason " + case_id + "<reason> to add it.";
                        });
                    if (LogChannelExtensions.is_toggled_modlog)
                        await mod_channel.SendMessageAsync($"📣 : You can not see mod-log datas because this channel has been **toggled off** !");
                    else
                    {
                        IUserMessage msg_instance = await MessageExtensions.SendMessageSafeAsync(mod_channel, "", false, mod_builder.Build());
                        ActionResult.CommandName = words[0];
                        ActionResult.CaseId = case_id;
                        ActionResult.UserId = user.Id.ToLong();
                        ActionResult.Instance = msg_instance;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task OnMesssageUpdatedAsync(Cacheable<IMessage, ulong> cachemsg, SocketMessage message, ISocketMessageChannel channel)
        {
            var msg = await _db.GetMessageAsync(message.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                LogChannelExtensions.IsUsableLogChannel(msg.GuildId.Value);
                var oldmsg = await cachemsg.GetOrDownloadAsync();
                var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                if (!oldmsg.Content.Equals(message.Content))
                {
                    var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(user.GetAvatarUrl())
                    .WithName("Message has been updated by " + user.Username + "#" + user.Discriminator + " in #" + channel.Name))
                    .WithDescription("Before: " + oldmsg.Content + "\n" +
                                        "After: " + message.Content)
                    .WithColor(new Color(0, 127, 255));

                    if (LogChannelExtensions.is_toggled_log)
                        await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                    else
                        await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
                }
                
            }
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachemsg, ISocketMessageChannel channel)
        {
            var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
            EmbedBuilder builder = new EmbedBuilder();
            var number_of_cleanup_messages = MessageExtensions.number_of_messages;
            if(number_of_cleanup_messages > 0)
            {
                var cleanup_user = _client.GetUser(MessageExtensions.userId);
                builder
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(cleanup_user.GetAvatarUrl())
                .WithName(cleanup_user.Username + "#" + cleanup_user.Discriminator + " has deleted " + number_of_cleanup_messages.ToString() + " messages."))
                .WithColor(new Color(0, 127, 127));
                await log_channel.SendMessageAsync("", false, builder.Build());
                return;
            }
            else
            {
                var msg = await _db.GetMessageAsync(cachemsg.Id);
                if (!msg.IsBot)
                {
                    LogChannelExtensions.IsUsableLogChannel(msg.GuildId.Value);
                    var oldmsg = await cachemsg.GetOrDownloadAsync();

                    var user = _client.GetUser(msg.AuthorId.ToUlong());
                    builder
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(user.GetAvatarUrl())
                    .WithName("Message --(" + oldmsg.Content + ")-- has been deleted!"))
                    .WithColor(new Color(0, 127, 127));
                    if (LogChannelExtensions.is_toggled_log)
                        await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                    else
                        await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
                }
            }
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var msg = await _db.GetMessageAsync(cachemsg.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                LogChannelExtensions.IsUsableLogChannel(msg.GuildId.Value);
                var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName("Reaction has been added by " + user.Username + "#" + user.Discriminator))
                .WithColor(new Color(255, 127, 127));
                if (LogChannelExtensions.is_toggled_log)
                    await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                else
                    await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var msg = await _db.GetMessageAsync(cachemsg.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                LogChannelExtensions.IsUsableLogChannel(msg.GuildId.Value);
                var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName("Reaction has been removed by " + user.Username + "#" + user.Discriminator))
                .WithColor(new Color(127, 127, 0));
                if (LogChannelExtensions.is_toggled_log)
                    await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                else
                    await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

        private async Task OnReactionsClearedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel)
        {
            var msg = await _db.GetMessageAsync(cachemsg.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                LogChannelExtensions.IsUsableLogChannel(msg.GuildId.Value);
                var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName("Reaction has been cleared by " + user.Username + "#" + user.Discriminator))
                .WithColor(new Color(0, 127, 127));
                if (LogChannelExtensions.is_toggled_log)
                    await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                else
                    await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            string custom_message = null;
            //var channel_id = GetLogChannelId(user.Guild.Id.ToLong());
            LogChannelExtensions.IsUsableLogChannel(user.Guild.Id.ToLong());
            var announce_channel_id = GetAnnounceChannelId(user.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
            var announce_channel = _client.GetChannel(announce_channel_id.ToUlong()) as ISocketMessageChannel;
            var is_found_user = CheckGuildUser(user.Id.ToLong(), user.Guild.Id.ToLong());
            if (is_found_user)
                custom_message = GetWecomeMessage(3, user.Guild.Id.ToLong());
            else
                custom_message = GetWecomeMessage(1, user.Guild.Id.ToLong());
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName(user.Username + "#" + user.Discriminator + " has joined."))
                .WithColor(new Color(0, 255, 127));
            if (LogChannelExtensions.is_toggled_log)
                await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
            else
                await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
            await announce_channel.SendMessageAsync(ReplaceInfo(user, custom_message));
        }

        private string GetWecomeMessage(int id, long guild_id)
        {
            string msg = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncemessages.Where(p => p.MessageId == id && p.GuildId == guild_id);
                    if (list.Any())
                        msg = list.First().Message;
                    else
                        msg = "No message";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return msg;
        }

        private async Task OnUserUnBannedAsync(SocketUser user, SocketGuild guild)
        {
            //var channel_id = GetLogChannelId(guild.Id.ToLong());
            LogChannelExtensions.IsUsableLogChannel(guild.Id.ToLong());
            var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName(user.Id.ToString() + " has been unbanned."))
                .WithColor(new Color(255, 255, 127));
            if (LogChannelExtensions.is_toggled_log)
                await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
            else
                await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
        }

        private async Task OnUserLeftAsync(SocketGuildUser user)
        {
            LogChannelExtensions.IsUsableLogChannel(user.Guild.Id.ToLong());
            var announce_channel_id = GetAnnounceChannelId(user.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
            var announce_channel = _client.GetChannel(announce_channel_id.ToUlong()) as ISocketMessageChannel;
            var custom_message = GetWecomeMessage(2, user.Guild.Id.ToLong());
            var guild_user = _client.GetUser(user.Id);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName(user.Username + "#" + user.Discriminator + " has left."))
                .WithColor(new Color(0, 0, 127));
            if (LogChannelExtensions.is_toggled_log)
                await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
            else
                await log_channel.SendMessageAsync("", false, builder.Build());
            await announce_channel.SendMessageAsync(ReplaceInfo(user, custom_message));
        }

        private async Task GuildMemberUpdated_RoleChange(SocketGuildUser b, SocketGuildUser a)
        {
            if (b.Roles == a.Roles) return;
            LogChannelExtensions.IsUsableLogChannel(b.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
          //  var guild = (_client.GetChannel(log_channel) as SocketGuildChannel).Guild;
            if (b.Roles.Count() > a.Roles.Count())
            {
                if (!LogChannelExtensions.is_toggled_log)
                {
                    var role = b.Roles.Except(a.Roles).FirstOrDefault();
                    var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(b.GetAvatarUrl())
                    .WithName($"{b.Nickname ?? b.Username}#{b.Discriminator} ({b.Id}) has lost role: {role.Name}"))
                    .WithColor(new Color(255, 67, 164));
                    await log_channel.SendMessageAsync("", false, builder.Build());
                }
            }
            else
            {
                if (!LogChannelExtensions.is_toggled_log)
                {
                    var role = a.Roles.Except(b.Roles).FirstOrDefault();
                    var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(b.GetAvatarUrl())
                    .WithName($"{b.Nickname ?? b.Username}#{b.Discriminator} ({b.Id}) has gained role: {role.Name}"))
                    .WithColor(new Color(67, 255, 164));
                    await log_channel.SendMessageAsync("", false, builder.Build());
                }
            }
        }

        //private async Task UserUpdated_NameChange(SocketUser b, SocketUser a)
        //{
        //    if (b.Username == a.Username) return;
        //    LogChannelExtensions.IsUsableLogChannel(b.Id.ToLong());
        //    var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
        //    if (!LogChannelExtensions.is_toggled_log)
        //    {
        //        var builder = new EmbedBuilder()
        //        .WithAuthor(new EmbedAuthorBuilder()
        //        .WithIconUrl(b.GetAvatarUrl())
        //        .WithName($"{b.Username}#{b.Discriminator} ({b.Id}) changed their username to {a.Username}"))
        //        .WithColor(new Color(1, 1, 1));
        //        await log_channel.SendMessageAsync("", false, builder.Build());
        //    }
        //}

        private async Task GuildMemberUpdated_NickChange(SocketGuildUser b, SocketGuildUser a)
        {
            if (b.Nickname == a.Nickname) return;
            LogChannelExtensions.IsUsableLogChannel(b.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
            if (!LogChannelExtensions.is_toggled_log)
            {
                if (b.Nickname == null)
                {
                    var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(b.GetAvatarUrl())
                    .WithName($"{b.Username}#{b.Discriminator} ({b.Id}) has taken on the nickname of {a.Nickname}."))
                    .WithColor(new Color(1, 1, 1));
                    await log_channel.SendMessageAsync("", false, builder.Build());
                }
                else if (a.Nickname == null)
                {
                    var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(b.GetAvatarUrl())
                    .WithName($"{b.Username}#{b.Discriminator} ({b.Nickname}) ({b.Id}) removed their nickname."))
                    .WithColor(new Color(1, 1, 1));
                    await log_channel.SendMessageAsync("", false, builder.Build());
                }
                else
                {
                    var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(b.GetAvatarUrl())
                    .WithName($"{b.Nickname ?? b.Username}#{b.Discriminator} ({b.Id}) changed their nickname to {a.Nickname}"))
                    .WithColor(new Color(1, 1, 1));
                    await log_channel.SendMessageAsync("", false, builder.Build());
                }
            }
        }
        
        private async Task UserNameChangedAsync(SocketUser b, SocketUser a)
        {
            if (b.Username == a.Username) return;
            LogChannelExtensions.IsUsableLogChannel(b.Id.ToLong());
            var log_channel = _client.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
            if (!LogChannelExtensions.is_toggled_log)
            {
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(b.GetAvatarUrl())
                .WithName($"{b.Username}#{b.Discriminator} ({b.Id}) changed their username to {a.Username}"))
                .WithColor(new Color(1, 1, 1));
                await log_channel.SendMessageAsync("", false, builder.Build());
            }
        }

        private string ReplaceInfo(SocketGuildUser user, string message)
        {
            var edited = message.Replace("{user}", $"{user.Mention}#{user.Discriminator}");
            edited = edited.Replace("{server}", $"{user.Guild.Name}");
            edited = edited.Replace("{count}", $"{user.Guild.MemberCount}");
            edited = edited.Replace("{bot}", $"{_client.CurrentUser}");
            return edited;
        }
    }
}
