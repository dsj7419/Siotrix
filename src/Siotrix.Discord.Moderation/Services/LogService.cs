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
        private long modlogchannel_id = 0;
        private long logchannel_id = 0;
        private bool is_toggled_log = false;
        private bool is_toggled_modlog = false;

        public LogService(DiscordSocketClient client)
        {
            _client = client;
        }

        private void IsUsableLogChannel(long guild_id)
        {
            using (var db = new LogDatabase())
            {
                try
                {
                    var log_list = db.Glogchannels.Where(p => p.GuildId.Equals(guild_id));
                    var modlog_list = db.Gmodlogchannels.Where(p => p.GuildId.Equals(guild_id));
                    if (log_list.Count() > 0 || log_list.Any())
                    {
                        var data = log_list.First();
                        logchannel_id = data.ChannelId;
                        if (!data.IsActive)
                            is_toggled_log = true;
                        else
                            is_toggled_log = false;
                    }
                    if (modlog_list.Count() > 0 || modlog_list.Any())
                    {
                        var mod_data = modlog_list.First();
                        modlogchannel_id = mod_data.ChannelId;
                        if (!mod_data.IsActive)
                            is_toggled_modlog = true;
                        else
                            is_toggled_modlog = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
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
            _db = null;

            await PrettyConsole.LogAsync("Info", "Log", "Service stopped successfully").ConfigureAwait(false);
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
                case "mute":
                    sentense = "muted";
                    break;
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
                case "mute":
                    color = new Color(127, 255, 127);
                    break;
                default:
                    break;
            }
            return color;
        }

        private int getCaseNumber(string str)
        {
            int number = 0;
            switch (str)
            {
                case "kick":
                    number = 7;
                    break;
                case "ban":
                    number = 8;
                    break;
                case "mute":
                    number = 9;
                    break;
                default:
                    break;
            }
            return number;
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

        private async Task OnRoleDeletedAsync(SocketRole role)
        {
            var channel_id = GetLogChannelId(role.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(channel_id.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder()
            .WithAuthor(new EmbedAuthorBuilder()
            .WithName("Role has been deleted."))
            .WithColor(new Color(0, 127, 255));
            await log_channel.SendMessageAsync("", false, builder.Build());
        }

        private async Task OnRoleUpdatedAsync(SocketRole role, SocketRole update_role)
        {
            var channel_id = GetLogChannelId(role.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(channel_id.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder()
            .WithAuthor(new EmbedAuthorBuilder()
            .WithName("Role has been updated."))
            .WithColor(new Color(0, 127, 255));
            await log_channel.SendMessageAsync("", false, builder.Build());
        }

        private async Task OnRoleCreatedAsync(SocketRole role)
        {
            var channel_id = GetLogChannelId(role.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(channel_id.ToUlong()) as ISocketMessageChannel;
            var builder = new EmbedBuilder()
            .WithAuthor(new EmbedAuthorBuilder()
            .WithName("Role has been created."))
            .WithColor(new Color(0, 127, 255));
            await log_channel.SendMessageAsync("", false, builder.Build());
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (!ActionResult.IsSuccess) return;
            var msg = message as SocketUserMessage;
            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            string spec = null;
            string content = null;
            
            IsUsableLogChannel(context.Guild.Id.ToLong());
            var channel = context.Guild.GetChannel(logchannel_id.ToUlong()) as ISocketMessageChannel;
            var mod_channel = context.Guild.GetChannel(modlogchannel_id.ToUlong()) as ISocketMessageChannel;
                
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
                var id = MentionUtils.ParseUser(words[1]);
                var user = _client.GetUser(id);
                var user_identifier = user.Username + "#" + user.Discriminator;
                var user_mention = user.Mention;
                var mod_identifier = context.User.Username + "#" + context.User.Discriminator;
                var action = getAction(words[0]);
                var action_color = getActionColor(words[0]);
                var case_number = getCaseNumber(words[0]);
                if (action == null) return;
                
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName(user_identifier + " has been " + action + " by " + mod_identifier))
                .WithColor(action_color);
                if (is_toggled_log)
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
                mod_builder
                    .AddField(x =>
                    {
                        x.Name = "Case #" + case_number.ToString() + " | " + words[0];
                        x.Value = "User : " + user.Username + " (" + user.Id.ToString() + ")" + "\n" + "Moderator : " + 
                                  context.User.Username + " (" + context.User.Id.ToString() + ")" + "\n" + 
                                  "Reason : Type " + g_prefix + "reason " +  case_number.ToString() + "<reason> to add it.";
                    });
                //await mod_channel.SendMessageAsync("", false, mod_builder.Build());
                if (is_toggled_modlog)
                    await mod_channel.SendMessageAsync($"📣 : You can not see mod-log datas because this channel has been **toggled off** !");
                else
                {
                    IUserMessage msg_instance = await MessageExtensions.SendMessageSafeAsync(mod_channel, "", false, mod_builder.Build());

                    ActionResult.Content = words[0] + "," + case_number.ToString() + "," + user.Username + "(" + user.Id.ToString() + ")" + "," + context.User.Username + "(" + context.User.Id.ToString() + ")";
                    ActionResult.Instance = msg_instance;
                }
            }
        }

        private async Task OnMesssageUpdatedAsync(Cacheable<IMessage, ulong> cachemsg, SocketMessage message, ISocketMessageChannel channel)
        {
            var msg = await _db.GetMessageAsync(message.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                IsUsableLogChannel(msg.GuildId.Value);
                var oldmsg = await cachemsg.GetOrDownloadAsync();
                var log_channel = _client.GetChannel(logchannel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName("Message has been updated by " + user.Username + "#" + user.Discriminator + " in #" + (message.Channel as SocketTextChannel).Name))
                .WithDescription("Before: " + oldmsg.Content+"\n" +
                                 "After: " + message.Content)              
                .WithColor(new Color(0, 127, 255));
                if (is_toggled_log)
                    await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                else
                    await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachemsg, ISocketMessageChannel channel)
        {
            var msg = await _db.GetMessageAsync(cachemsg.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                IsUsableLogChannel(msg.GuildId.Value);
                var log_channel = _client.GetChannel(logchannel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName("Message has been deleted by " + user.Username + "#" + user.Discriminator))
                .WithColor(new Color(0, 127, 127));
                if (is_toggled_log)
                    await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                else
                    await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

     

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var msg = await _db.GetMessageAsync(cachemsg.Id);
            if (!msg.IsBot)
            {
                //var channel_id = GetLogChannelId(msg.GuildId.Value);
                IsUsableLogChannel(msg.GuildId.Value);
                var log_channel = _client.GetChannel(logchannel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName("Reaction has been added by " + user.Username + "#" + user.Discriminator))
                .WithColor(new Color(0, 127, 127));
                if (is_toggled_log)
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
                IsUsableLogChannel(msg.GuildId.Value);
                var log_channel = _client.GetChannel(logchannel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName("Reaction has been removed by " + user.Username + "#" + user.Discriminator))
                .WithColor(new Color(0, 127, 127));
                if (is_toggled_log)
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
                IsUsableLogChannel(msg.GuildId.Value);
                var log_channel = _client.GetChannel(logchannel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName("Reaction has been cleared by " + user.Username + "#" + user.Discriminator))
                .WithColor(new Color(0, 127, 127));
                if (is_toggled_log)
                    await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
                else
                    await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            //var channel_id = GetLogChannelId(user.Guild.Id.ToLong());
            IsUsableLogChannel(user.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(logchannel_id.ToUlong()) as ISocketMessageChannel;
            var guild_user = _client.GetUser(user.Id);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(guild_user.GetAvatarUrl())
                .WithName(user.Username + "#" + user.Discriminator + " has joined."))
                .WithColor(new Color(0, 255, 127));
            if (is_toggled_log)
                await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
            else
                await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
        }

        private async Task OnUserUnBannedAsync(SocketUser user, SocketGuild guild)
        {
            //var channel_id = GetLogChannelId(guild.Id.ToLong());
            IsUsableLogChannel(guild.Id.ToLong());
            var log_channel = _client.GetChannel(logchannel_id.ToUlong()) as ISocketMessageChannel;
            var guild_user = _client.GetUser(user.Id);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(guild_user.GetAvatarUrl())
                .WithName(user.Id.ToString() + " has been unbanned."))
                .WithColor(new Color(0, 255, 127));
            if (is_toggled_log)
                await log_channel.SendMessageAsync($"📣 : You can not see log datas because this channel has been **toggled off** !");
            else
                await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
        }
    }
}
