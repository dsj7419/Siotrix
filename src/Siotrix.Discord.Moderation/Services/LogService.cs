using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    public class LogService : IService
    {
        //var channel = context.Guild.GetChannel(data.ChannelId.ToUlong()) as ISocketMessageChannel;

        private readonly DiscordSocketClient _client;
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

        private async void OnUnMuteReceivedAsync(IGuildUser user, MuteExtensions.MuteType muteType,
            SocketCommandContext context, bool isAuto)
        {
            try
            {
                long caseId = 0;
                var guild = user.Guild as SocketGuild;
                string unmuteData = null;
                LogChannelExtensions.IsUsableLogChannel(guild.Id.ToLong());
                var channel = guild.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
                var modChannel =
                    guild.GetChannel(LogChannelExtensions.ModlogchannelId.ToUlong()) as ISocketMessageChannel;

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
                if (isAuto)
                    unmuteData = user.Username + "#" + user.Discriminator + " has been auto " + unmutes + " unmuted.";
                else
                    unmuteData = user.Username + "#" + user.Discriminator + " has been " + unmutes + " unmuted by " +
                                  context.User.Username + "#" + context.User.Discriminator + ".";
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(user.GetAvatarUrl())
                        .WithName(unmuteData))
                    .WithColor(new Color(127, 255, 127));
                if (!LogChannelExtensions.IsToggledLog)
                    await channel.SendMessageAsync(user.Mention, false, builder.Build());

                var gIconUrl = await context.GetGuildIconUrlAsync();
                var gName = await context.GetGuildNameAsync();
                var gUrl = await context.GetGuildUrlAsync();
                var gThumbnail = await context.GetGuildThumbNailAsync();
                var gFooter = await context.GetGuildFooterAsync();
                var gPrefix = await context.GetGuildPrefixAsync();
                string value = null;
                var modBuilder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(gIconUrl.Avatar)
                        .WithName(gName.GuildName)
                        .WithUrl(gUrl.SiteUrl))
                    .WithColor(new Color(127, 255, 0))
                    .WithThumbnailUrl(gThumbnail.ThumbNail)
                    .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(gFooter.FooterIcon)
                        .WithText(gFooter.FooterText))
                    .WithTimestamp(DateTime.UtcNow);
                caseId = context.GetCaseNumber();

                if (isAuto)
                    value = "User : " + user.Mention + " (" + user.Id + ")" + "\n" + "Moderator : " +
                            context.Guild.CurrentUser.Mention + "\n" +
                            "Reason : auto";
                else
                    value = "User : " + user.Mention + " (" + user.Id + ")" + "\n" + "Moderator : " +
                            context.User.Username + " (" + context.User.Id + ")" + "\n" +
                            "Reason : Type " + gPrefix + "reason " + caseId + "<reason> to add it.";
                modBuilder
                    .AddField(x =>
                    {
                        x.Name = "Case #" + caseId + " | unmute";
                        x.Value = value;
                    });
                if (!LogChannelExtensions.IsToggledModlog)
                {
                    var msgInstance = await modChannel.SendMessageSafeAsync("", false, modBuilder.Build());
                    ActionResult.CommandName = "unmute";
                    ActionResult.CaseId = caseId;
                    ActionResult.UserId = user.Id.ToLong();
                    ActionResult.Instance = msgInstance;
                    ActionResult.IsFoundedCaseNumber = true;
                    Console.WriteLine("Service-Unmute +++++++++++++++++++++++++++++++{0}", caseId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void OnMuteReceivedAsync(IGuildUser user, MuteExtensions.MuteType muteType,
            SocketCommandContext context, int minutes, bool isAuto)
        {
            try
            {
                long caseId = 0;
                var guild = user.Guild as SocketGuild;
                string muteData = null;
                string value = null;
                LogChannelExtensions.IsUsableLogChannel(guild.Id.ToLong());
                var channel = guild.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
                var modChannel =
                    guild.GetChannel(LogChannelExtensions.ModlogchannelId.ToUlong()) as ISocketMessageChannel;

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

                if (isAuto)
                    muteData = user.Username + "#" + user.Discriminator + " has been auto " + mutes + " muted.";
                else
                    muteData = user.Username + "#" + user.Discriminator + " has been " + mutes + " muted by " +
                                context.User.Username + "#" + context.User.Discriminator + ".";
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(user.GetAvatarUrl())
                        .WithName(muteData))
                    .WithColor(new Color(127, 255, 0));
                if (!LogChannelExtensions.IsToggledLog)
                    await channel.SendMessageAsync(user.Mention, false, builder.Build());

                var gIconUrl = await context.GetGuildIconUrlAsync();
                var gName = await context.GetGuildNameAsync();
                var gUrl = await context.GetGuildUrlAsync();
                var gThumbnail = await context.GetGuildThumbNailAsync();
                var gFooter = await context.GetGuildFooterAsync();
                var gPrefix = await context.GetGuildPrefixAsync();
                var modBuilder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(gIconUrl.Avatar)
                        .WithName(gName.GuildName)
                        .WithUrl(gUrl.SiteUrl))
                    .WithColor(new Color(127, 255, 0))
                    .WithThumbnailUrl(gThumbnail.ThumbNail)
                    .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(gFooter.FooterIcon)
                        .WithText(gFooter.FooterText))
                    .WithTimestamp(DateTime.UtcNow);
                caseId = context.GetCaseNumber();

                if (isAuto)
                    value = "User : " + user.Mention + " (" + user.Id + ")" + "\n" + "Moderator : " +
                            context.Guild.CurrentUser.Mention + "\n" +
                            "Length : " + minutes + "minutes" + "\n" +
                            "Reason : auto";
                else
                    value = "User : " + user.Mention + " (" + user.Id + ")" + "\n" + "Moderator : " +
                            context.User.Username + " (" + context.User.Id + ")" + "\n" +
                            "Length : " + minutes + "minutes" + "\n" +
                            "Reason : Type " + gPrefix + "reason " + caseId + "<reason> to add it.";
                modBuilder
                    .AddField(x =>
                    {
                        x.Name = "Case #" + caseId + " | mute";
                        x.Value = value;
                    });
                if (!LogChannelExtensions.IsToggledModlog)
                {
                    var msgInstance = await modChannel.SendMessageSafeAsync("", false, modBuilder.Build());
                    ActionResult.CommandName = "mute";
                    ActionResult.CaseId = caseId;
                    ActionResult.UserId = user.Id.ToLong();
                    ActionResult.TimeLength = minutes;
                    ActionResult.Instance = msgInstance;
                    Console.WriteLine("Service-Mute ------------------{0}", caseId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private long GetLogChannelId(long guildId)
        {
            long id = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Glogchannels.Where(p => p.GuildId.Equals(guildId));
                    if (data.Any() || data.Count() > 0)
                        id = data.First().ChannelId;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return id;
        }

        private long GetAnnounceChannelId(long guildId)
        {
            long channelId = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncechannels.Where(p => p.GuildId == guildId);
                    if (list.Any())
                        channelId = list.First().ChannelId;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return channelId;
        }

        private string GetAction(string str)
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

        private Color GetActionColor(string str)
        {
            var color = new Color();
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
                    if (data.Any() || data.Count() > 0)
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
            long caseId = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Casenums.Where(x => x.GuildId.Equals(context.Guild.Id.ToLong()));
                    if (!data.Any())
                        caseId = 1;
                    else
                        caseId = data.Last().GCaseNum + 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return caseId;
        }

        private bool CheckGuildUser(long userId, long guildId)
        {
            var isFound = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var data = db.Messages.Where(x => x.AuthorId == userId && x.GuildId == guildId);
                    if (data.Any())
                        isFound = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isFound;
        }

        private async Task OnRoleDeletedAsync(SocketRole role)
        {
            var channelId = GetLogChannelId(role.Guild.Id.ToLong());
            var logChannel = _client.GetChannel(channelId.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName("Role has been deleted."))
                .WithColor(new Color(255, 0, 0));
            await logChannel.SendMessageAsync("", false, builder.Build());
        }

        private async Task OnRoleUpdatedAsync(SocketRole role, SocketRole updateRole)
        {
            var channelId = GetLogChannelId(role.Guild.Id.ToLong());
            var logChannel = _client.GetChannel(channelId.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder();
            builder.WithColor(new Color(255, 127, 255));
            if (updateRole.Name != role.Name)
                builder.WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"{role.Name} role name has been updated to: {updateRole.Name}."));
            else if (updateRole.Color.ToString() != role.Color.ToString())
                if (updateRole.Color.RawValue is 0)
                {
                    builder.WithAuthor(new EmbedAuthorBuilder()
                        .WithName($"{updateRole.Name} role color has been removed(previously: {role.Color})"));
                }
                else
                {
                    var rolecolor = role.Color.ToString();
                    var updateRoleColor = updateRole.Color.ToString();

                    if (rolecolor.Length != 7)
                    {
                        rolecolor = rolecolor.Substring(1);
                        rolecolor = "#" + rolecolor.PadLeft(6, '0');
                    }

                    if (updateRoleColor.Length != 7)
                    {
                        updateRoleColor = updateRoleColor.Substring(1);
                        updateRoleColor = "#" + updateRoleColor.PadLeft(6, '0');
                    }

                    builder.WithAuthor(new EmbedAuthorBuilder()
                        .WithName(
                            $"{updateRole.Name} role color has been updated from {rolecolor} to {updateRoleColor}."));
                }
            else if (updateRole.IsMentionable != role.IsMentionable)
                if (updateRole.IsMentionable)
                    builder.WithAuthor(new EmbedAuthorBuilder()
                        .WithName($"{updateRole.Name} role is now mentionable."));
                else
                    builder.WithAuthor(new EmbedAuthorBuilder()
                        .WithName($"{updateRole.Name} role is no longer mentionable."));
            else if (updateRole.IsHoisted != role.IsHoisted)
                if (updateRole.IsHoisted)
                    builder.WithAuthor(new EmbedAuthorBuilder()
                        .WithName($"{updateRole.Name} role will now be shown separately from the other roles."));
                else
                    builder.WithAuthor(new EmbedAuthorBuilder()
                        .WithName($"{updateRole.Name} role is no longer shown separately."));
            else if (updateRole.Position != role.Position)
                builder.WithAuthor(new EmbedAuthorBuilder()
                    .WithName(
                        $"{updateRole.Name} role position has been updated from {role.Position} to {updateRole.Position}."));
            else if (updateRole.Permissions.RawValue != role.Permissions.RawValue)
                builder.WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"{updateRole.Name} role permissions have been updated."));
            else
                builder.WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"{updateRole.Name} role has been updated."));
            await logChannel.SendMessageAsync("", false, builder.Build());
        }

        private async Task OnRoleCreatedAsync(SocketRole role)
        {
            var channelId = GetLogChannelId(role.Guild.Id.ToLong());
            var logChannel = _client.GetChannel(channelId.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName("Role has been created."))
                .WithColor(new Color(127, 255, 0));
            await logChannel.SendMessageAsync("", false, builder.Build());
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            try
            {
                if (!ActionResult.IsSuccess) return;
                var msg = message as SocketUserMessage;
                var context = new SocketCommandContext(_client, msg);
                var argPos = 0;
                string spec = null;
                var val = await context.GetGuildPrefixAsync();
                string content = null;
                long caseId = 0;

                LogChannelExtensions.IsUsableLogChannel(context.Guild.Id.ToLong());
                var channel =
                    context.Guild.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
                var modChannel =
                    context.Guild.GetChannel(LogChannelExtensions.ModlogchannelId.ToUlong()) as ISocketMessageChannel;

                spec = val.Prefix;
                if (message.Author.IsBot
                    || msg == null
                    || !msg.Content.Except("?").Any()
                    || msg.Content.Trim().Length <= 1
                    || msg.Content.Trim()[1] == '?'
                    || !(msg.HasStringPrefix(spec, ref argPos) ||
                         msg.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                    return;
                if (msg.HasStringPrefix(spec, ref argPos))
                    content = MessageParser.ParseStringPrefix(msg, spec);
                else if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                    content = MessageParser.ParseMentionPrefix(msg);
                if (msg.HasStringPrefix(spec, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                {
                    var words = content.Split(' ');
                    var action = GetAction(words[0]);
                    if (action == null)
                        return; // If action is not kick or ban or mute command, not display log information.
                    var id = MentionUtils.ParseUser(words[1]);
                    var user = _client.GetUser(id);
                    var userIdentifier = user.Username + "#" + user.Discriminator;
                    var userMention = user.Mention;
                    var modIdentifier = context.User.Username + "#" + context.User.Discriminator;

                    var actionColor = GetActionColor(words[0]);

                    var builder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(user.GetAvatarUrl())
                            .WithName(userIdentifier + " has been " + action + " by " + modIdentifier))
                        .WithColor(actionColor);
                    if (!LogChannelExtensions.IsToggledLog)
                        await channel.SendMessageAsync(userMention, false, builder.Build());

                    var gIconUrl = await context.GetGuildIconUrlAsync();
                    var gName = await context.GetGuildNameAsync();
                    var gUrl = await context.GetGuildUrlAsync();
                    var gThumbnail = await context.GetGuildThumbNailAsync();
                    var gFooter = await context.GetGuildFooterAsync();
                    var gPrefix = await context.GetGuildPrefixAsync();
                    var modBuilder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(gIconUrl.Avatar)
                            .WithName(gName.GuildName)
                            .WithUrl(gUrl.SiteUrl))
                        .WithColor(actionColor)
                        .WithThumbnailUrl(gThumbnail.ThumbNail)
                        .WithFooter(new EmbedFooterBuilder()
                            .WithIconUrl(gFooter.FooterIcon)
                            .WithText(gFooter.FooterText))
                        .WithTimestamp(DateTime.UtcNow);
                    //case_id = GetCaseNumberAync(words[0], context, user as SocketGuildUser);
                    caseId = context.GetCaseNumber();
                    modBuilder
                        .AddField(x =>
                        {
                            x.Name = "Case #" + caseId + " | " + words[0];
                            x.Value = "User : " + user.Mention + " (" + user.Id.ToString() + ")" + "\n" +
                                      "Moderator : " +
                                      context.User.Username + " (" + context.User.Id.ToString() + ")" + "\n" +
                                      "Reason : Type " + gPrefix + "reason " + caseId + "<reason> to add it.";
                        });
                    if (!LogChannelExtensions.IsToggledModlog)
                    {
                        var msgInstance = await modChannel.SendMessageSafeAsync("", false, modBuilder.Build());
                        ActionResult.CommandName = words[0];
                        ActionResult.CaseId = caseId;
                        ActionResult.UserId = user.Id.ToLong();
                        ActionResult.Instance = msgInstance;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task OnMesssageUpdatedAsync(Cacheable<IMessage, ulong> cachemsg, SocketMessage message,
            ISocketMessageChannel channel)
        {
            var msg = await _db.GetMessageAsync(message.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                LogChannelExtensions.IsUsableLogChannel(msg.GuildId.Value);
                var oldmsg = await cachemsg.GetOrDownloadAsync();
                var logChannel =
                    _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                if (!oldmsg.Content.Equals(message.Content))
                {
                    var builder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(user.GetAvatarUrl())
                            .WithName("Message has been updated by " + user.Username + "#" + user.Discriminator +
                                      " in #" + channel.Name))
                        .WithDescription("Before: " + oldmsg.Content + "\n" +
                                         "After: " + message.Content)
                        .WithColor(new Color(0, 127, 255));

                    if (!LogChannelExtensions.IsToggledLog)
                        await logChannel.SendMessageAsync(user.Mention, false, builder.Build());
                }
            }
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachemsg, ISocketMessageChannel channel)
        {
            var logChannel = _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder();
            var numberOfCleanupMessages = MessageExtensions.NumberOfMessages;
            if (numberOfCleanupMessages > 0)
            {
                var cleanupUser = _client.GetUser(MessageExtensions.UserId);
                builder
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(cleanupUser.GetAvatarUrl())
                        .WithName(cleanupUser.Username + "#" + cleanupUser.Discriminator + " has deleted " +
                                  numberOfCleanupMessages + " messages."))
                    .WithColor(new Color(0, 127, 127));
                await logChannel.SendMessageAsync("", false, builder.Build());
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
                    if (!LogChannelExtensions.IsToggledLog)
                        await logChannel.SendMessageAsync(user.Mention, false, builder.Build());
                }
            }
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var msg = await _db.GetMessageAsync(cachemsg.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                LogChannelExtensions.IsUsableLogChannel(msg.GuildId.Value);
                var logChannel =
                    _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(user.GetAvatarUrl())
                        .WithName("Reaction has been added by " + user.Username + "#" + user.Discriminator))
                    .WithColor(new Color(255, 127, 127));
                if (!LogChannelExtensions.IsToggledLog)
                    await logChannel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachemsg,
            ISocketMessageChannel channel, SocketReaction reaction)
        {
            var msg = await _db.GetMessageAsync(cachemsg.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                LogChannelExtensions.IsUsableLogChannel(msg.GuildId.Value);
                var logChannel =
                    _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(user.GetAvatarUrl())
                        .WithName("Reaction has been removed by " + user.Username + "#" + user.Discriminator))
                    .WithColor(new Color(127, 127, 0));
                if (!LogChannelExtensions.IsToggledLog)
                    await logChannel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

        private async Task OnReactionsClearedAsync(Cacheable<IUserMessage, ulong> cachemsg,
            ISocketMessageChannel channel)
        {
            var msg = await _db.GetMessageAsync(cachemsg.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                LogChannelExtensions.IsUsableLogChannel(msg.GuildId.Value);
                var logChannel =
                    _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(user.GetAvatarUrl())
                        .WithName("Reaction has been cleared by " + user.Username + "#" + user.Discriminator))
                    .WithColor(new Color(0, 127, 127));
                if (!LogChannelExtensions.IsToggledLog)
                    await logChannel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            string customMessage = null;
            //var channel_id = GetLogChannelId(user.Guild.Id.ToLong());
            LogChannelExtensions.IsUsableLogChannel(user.Guild.Id.ToLong());
            var announceChannelId = GetAnnounceChannelId(user.Guild.Id.ToLong());
            var logChannel = _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
            var announceChannel = _client.GetChannel(announceChannelId.ToUlong()) as ISocketMessageChannel;
            var isFoundUser = CheckGuildUser(user.Id.ToLong(), user.Guild.Id.ToLong());
            if (isFoundUser)
                customMessage = GetWecomeMessage(3, user.Guild.Id.ToLong());
            else
                customMessage = GetWecomeMessage(1, user.Guild.Id.ToLong());
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(user.GetAvatarUrl())
                    .WithName(user.Username + "#" + user.Discriminator + " has joined."))
                .WithColor(new Color(0, 255, 127));
            if (!LogChannelExtensions.IsToggledLog)
                await logChannel.SendMessageAsync(user.Mention, false, builder.Build());
            await announceChannel.SendMessageAsync(ReplaceInfo(user, customMessage));
        }

        private string GetWecomeMessage(int id, long guildId)
        {
            string msg = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncemessages.Where(p => p.MessageId == id && p.GuildId == guildId);
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
            var logChannel = _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(user.GetAvatarUrl())
                    .WithName(user.Id + " has been unbanned."))
                .WithColor(new Color(255, 255, 127));
            if (!LogChannelExtensions.IsToggledLog)
                await logChannel.SendMessageAsync(user.Mention, false, builder.Build());
        }

        private async Task OnUserLeftAsync(SocketGuildUser user)
        {
            LogChannelExtensions.IsUsableLogChannel(user.Guild.Id.ToLong());
            var announceChannelId = GetAnnounceChannelId(user.Guild.Id.ToLong());
            var logChannel = _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
            var announceChannel = _client.GetChannel(announceChannelId.ToUlong()) as ISocketMessageChannel;
            var customMessage = GetWecomeMessage(2, user.Guild.Id.ToLong());
            var guildUser = _client.GetUser(user.Id);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(user.GetAvatarUrl())
                    .WithName(user.Username + "#" + user.Discriminator + " has left."))
                .WithColor(new Color(0, 0, 127));
            if (!LogChannelExtensions.IsToggledLog)
                await logChannel.SendMessageAsync("", false, builder.Build());
            await announceChannel.SendMessageAsync(ReplaceInfo(user, customMessage));
        }

        private async Task GuildMemberUpdated_RoleChange(SocketGuildUser b, SocketGuildUser a)
        {
            if (b.Roles == a.Roles) return;
            LogChannelExtensions.IsUsableLogChannel(b.Guild.Id.ToLong());
            var logChannel = _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
            //  var guild = (_client.GetChannel(log_channel) as SocketGuildChannel).Guild;
            if (b.Roles.Count() > a.Roles.Count())
            {
                if (!LogChannelExtensions.IsToggledLog)
                {
                    var role = b.Roles.Except(a.Roles).FirstOrDefault();
                    var builder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(b.GetAvatarUrl())
                            .WithName(
                                $"{b.Nickname ?? b.Username}#{b.Discriminator} ({b.Id}) has lost role: {role.Name}"))
                        .WithColor(new Color(255, 67, 164));
                    await logChannel.SendMessageAsync("", false, builder.Build());
                }
            }
            else
            {
                if (!LogChannelExtensions.IsToggledLog)
                {
                    var role = a.Roles.Except(b.Roles).FirstOrDefault();
                    var builder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(b.GetAvatarUrl())
                            .WithName(
                                $"{b.Nickname ?? b.Username}#{b.Discriminator} ({b.Id}) has gained role: {role.Name}"))
                        .WithColor(new Color(67, 255, 164));
                    await logChannel.SendMessageAsync("", false, builder.Build());
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
            var logChannel = _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
            if (!LogChannelExtensions.IsToggledLog)
                if (b.Nickname == null)
                {
                    var builder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(b.GetAvatarUrl())
                            .WithName(
                                $"{b.Username}#{b.Discriminator} ({b.Id}) has taken on the nickname of {a.Nickname}."))
                        .WithColor(new Color(1, 1, 1));
                    await logChannel.SendMessageAsync("", false, builder.Build());
                }
                else if (a.Nickname == null)
                {
                    var builder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(b.GetAvatarUrl())
                            .WithName(
                                $"{b.Username}#{b.Discriminator} ({b.Nickname}) ({b.Id}) removed their nickname."))
                        .WithColor(new Color(1, 1, 1));
                    await logChannel.SendMessageAsync("", false, builder.Build());
                }
                else
                {
                    var builder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(b.GetAvatarUrl())
                            .WithName(
                                $"{b.Nickname ?? b.Username}#{b.Discriminator} ({b.Id}) changed their nickname to {a.Nickname}"))
                        .WithColor(new Color(1, 1, 1));
                    await logChannel.SendMessageAsync("", false, builder.Build());
                }
        }

        private async Task UserNameChangedAsync(SocketUser b, SocketUser a)
        {
            if (b.Username == a.Username) return;
            LogChannelExtensions.IsUsableLogChannel(b.Id.ToLong());
            var logChannel = _client.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
            if (!LogChannelExtensions.IsToggledLog)
            {
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(b.GetAvatarUrl())
                        .WithName($"{b.Username}#{b.Discriminator} ({b.Id}) changed their username to {a.Username}"))
                    .WithColor(new Color(1, 1, 1));
                await logChannel.SendMessageAsync("", false, builder.Build());
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