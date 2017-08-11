using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    public class AntilinkService : IService
    {
        private readonly DiscordSocketClient _client;

        public AntilinkService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task StartAsync()
        {
            _client.MessageReceived += OnMessageReceivedAsync;
            _client.MessageUpdated += OnMessageUpdatedAsync;
            _client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
            await PrettyConsole.LogAsync("Info", "Antilink", "Service started successfully").ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            _client.MessageUpdated -= OnMessageUpdatedAsync;
            _client.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
            await PrettyConsole.LogAsync("Info", "Antilink", "Service stopped successfully").ConfigureAwait(false);
        }

        private async Task OnMessageReceivedAsync(SocketMessage msg)
        {
            var regexItem =
                new Regex(@"<?(https?:\/\/)?(www\.)?(discord\.gg|discordapp\.com\/invite)\b([-a-zA-Z0-9/]*)>?");
            var regexDiscordMe = new Regex(@"<?(https?:\/\/)?(www\.)?(discord\.me\/)\b([-a-zA-Z0-9/]*)>?");
            var regexDiscordEmojis = new Regex(@"<?([a-zA-Z]+):([0-9]+)>?");
            var regexUrl =
                new Regex(
                    @"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_=]*)?");

            var message = msg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

            var antilink = await AntilinkExtensions.GetAntilinkAsync(context.Guild.Id);

            if (antilink == null)
            {
                await AntilinkExtensions.CreateAntilinkAsync(context);
                return;
            }
            if (antilink.IsActive == false)
            {
                return;
            }


            var channel = message.Channel as SocketGuildChannel;
            var antilinkChannel = await AntilinkExtensions.GetAntilinkChanneListAsync(context.Guild.Id, channel);

            if (antilinkChannel == null)
                return;
            if (antilinkChannel.IsActive == false)
                return;

            var user = message.Author as SocketGuildUser;
            var antilinkUser = await AntilinkExtensions.GetAntilinkUserListAsync(context.Guild.Id, user.Id, channel.Id);

            if (regexItem.IsMatch(message.Content) || regexDiscordMe.IsMatch(message.Content) ||
                regexUrl.IsMatch(message.Content) && antilinkChannel.IsStrict)
                if (user.IsBot && user.Id == SiotrixConstants.BotId)
                {
                }
                else if (user.GetPermissions(channel).ManageMessages)
                {
                }
                else if (antilinkUser != null && antilinkUser.IsOneTime == false)
                {
                }
                else if (antilinkUser != null && antilinkUser.IsOneTime)
                {
                    await AntilinkExtensions.DeleteAntilinkUserAsync(antilinkUser);
                }
                else
                {
                    LogChannelExtensions.IsUsableLogChannel(context.Guild.Id.ToLong());
                    var channelLog =
                        context.Guild.GetChannel(LogChannelExtensions.LogchannelId.ToUlong()) as ISocketMessageChannel;
                    var builder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(msg.Author.GetAvatarUrl())
                            .WithName("Removed Message - Broke Hyperlink Rules in " + channel.Name + ": " +
                                      message.Content + " " + msg.Author.Username + "#" + msg.Author.Discriminator +
                                      user.Mention))
                        .WithColor(new Color(255, 127, 0));
                    await channelLog.SendMessageAsync("", false, builder.Build());

                    if (antilink.IsDmMessage)
                        await MessageExtensions.DmUser(user, antilink.DmMessage);

                    await msg.DeleteAsync();
                }
        }

        private Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> cachemsg, SocketMessage msg,
            ISocketMessageChannel channel)
        {
            return Task.CompletedTask;
        }

        private Task OnGuildMemberUpdatedAsync(SocketGuildUser before, SocketGuildUser after)
        {
            return Task.CompletedTask;
        }
    }
}