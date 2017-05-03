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
            //_client.ReactionAdded += OnReactionAddedAsync;
            //_client.ReactionRemoved += OnReactionRemovedAsync;
            //_client.ReactionsCleared += OnReactionsClearedAsync;
            _client.UserJoined += OnUserJoinedAsync;

            await PrettyConsole.LogAsync("Info", "Log", "Service started successfully").ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            _client.MessageUpdated -= OnMesssageUpdatedAsync;
            _client.MessageDeleted -= OnMessageDeletedAsync;
            //_client.ReactionAdded -= OnReactionAddedAsync;
            //_client.ReactionRemoved -= OnReactionRemovedAsync;
            //_client.ReactionsCleared -= OnReactionsClearedAsync;
            _client.UserJoined -= OnUserJoinedAsync;
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
                    id = db.Glogchannels.Where(p => p.GuildId.Equals(guild_id)).First().ChannelId;
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
                    id = db.Gmodlogchannels.Where(p => p.GuildId.Equals(context.Guild.Id.ToLong())).First().ChannelId;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return id;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (!ActionResult.IsSuccess) return;
            var msg = message as SocketUserMessage;
            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            string spec = null;
            string content = null;
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
                var channel_id = GetLogChannelId(context.Guild.Id.ToLong());
                if (channel_id <= 0) return;
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
                
                var channel = context.Guild.GetChannel(channel_id.ToUlong()) as ISocketMessageChannel;
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName(user_identifier + " has been " + action + " by " + mod_identifier))
                .WithColor(action_color);
                await channel.SendMessageAsync(user_mention, false, builder.Build());

                var mod_channel_id = GetModLogChannelId(context);
                var mod_channel = context.Guild.GetChannel(mod_channel_id.ToUlong()) as ISocketMessageChannel;
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
                await mod_channel.SendMessageAsync("", false, mod_builder.Build());

                ActionResult.Content = words[0] + "," + case_number.ToString() + "," + user.Username + "(" + user.Id.ToString() + ")" + "," + context.User.Username + "(" + context.User.Id.ToString() + ")";
                return;
            }
        }

        private async Task OnMesssageUpdatedAsync(Cacheable<IMessage, ulong> cachemsg, SocketMessage message, ISocketMessageChannel channel)
        {
            var msg = await _db.GetMessageAsync(message.Id);
            if (!msg.IsBot)
            {
                var channel_id = GetLogChannelId(msg.GuildId.Value);
                var log_channel = _client.GetChannel(channel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName("Message has been updated by " + user.Username + "#" + user.Discriminator))
                .WithColor(new Color(0, 127, 255));
                await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachemsg, ISocketMessageChannel channel)
        {
            var msg = await _db.GetMessageAsync(cachemsg.Id);
            if (!msg.IsBot)
            {
                var channel_id = GetLogChannelId(msg.GuildId.Value);
                var log_channel = _client.GetChannel(channel_id.ToUlong()) as ISocketMessageChannel;
                var user = _client.GetUser(msg.AuthorId.ToUlong());
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(user.GetAvatarUrl())
                .WithName("Message has been deleted by " + user.Username + "#" + user.Discriminator))
                .WithColor(new Color(0, 127, 127));
                await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
            }
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            var channel_id = GetLogChannelId(user.Guild.Id.ToLong());
            var log_channel = _client.GetChannel(channel_id.ToUlong()) as ISocketMessageChannel;
            var guild_user = _client.GetUser(user.Id);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(guild_user.GetAvatarUrl())
                .WithName(user.Username + "#" + user.Discriminator + " has joined."))
                .WithColor(new Color(0, 255, 127));
            await log_channel.SendMessageAsync(user.Mention, false, builder.Build());
        }

        /*private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var react = EntityHelper.CreateReaction(reaction);

            _db.Reactions.Add(react);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var react = await _db.GetReactionAsync(cachemsg.Id, reaction.UserId, reaction.Emoji.Name);

            react.DeletedAt = DateTime.UtcNow;

            _db.Reactions.Update(react);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task OnReactionsClearedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel)
        {
            var reacts = await _db.GetReactionsAsync(cachemsg.Id);

            foreach (var react in reacts)
                react.DeletedAt = DateTime.UtcNow;

            _db.Reactions.UpdateRange(reacts);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }*/
    }
}
