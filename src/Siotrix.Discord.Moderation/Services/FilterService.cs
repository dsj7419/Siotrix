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
    public class FilterService : IService
    {
        private DiscordSocketClient _client;
        private LogDatabase _db;

        public FilterService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task StartAsync()
        {
            _client.MessageReceived += OnMessageReceivedAsync;
            _client.MessageUpdated += OnMessageUpdatedAsync;
            _client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
            await PrettyConsole.LogAsync("Info", "Log", "Service started successfully").ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            _client.MessageUpdated -= OnMessageUpdatedAsync;
            _client.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
            _db = null;
            await PrettyConsole.LogAsync("Info", "Log", "Service stopped successfully").ConfigureAwait(false);
        }
        
        private async Task OnMessageReceivedAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            var dictionary = GetFilterWords(context.Guild.Id.ToLong());
            string[] words = msg.Content.Split(' ');
            LogChannelExtensions.IsUsableLogChannel(context.Guild.Id.ToLong());
            var channel = context.Guild.GetChannel(LogChannelExtensions.logchannel_id.ToUlong()) as ISocketMessageChannel;
            var badword = ParseMessages(words, dictionary);
            if(badword != null)
            {
                var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(msg.Author.GetAvatarUrl())
                .WithName("Founded --" + badword + "-- word from message of " + msg.Author.Username + "#" + msg.Author.Discriminator))
                .WithColor(new Color(255, 0, 0));
                await channel.SendMessageAsync("", false, builder.Build());
                await msg.DeleteAsync();
            }
        }

        private Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> cachemsg, SocketMessage msg, ISocketMessageChannel channel)
        {
            return Task.CompletedTask;
        }

        private Task OnGuildMemberUpdatedAsync(SocketGuildUser before, SocketGuildUser after)
        {
            return Task.CompletedTask;
        }

        private bool ContainsFilteredWord(string value)
        {
            return false;
        }

        private Dictionary<int, string> GetFilterWords(long guild_id)
        {
            var dictionary = new Dictionary<int, string>();
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guild_id);
                    if (result.Any())
                    {
                        int i = 0;
                        foreach(var item in result)
                        {
                            dictionary.Add(i, item.Word);
                            i++;
                        }
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return dictionary;
        }

        private string ParseMessages(string[] msg, Dictionary<int, string> dictionary)
        {
            string badword = null;
            foreach(var msg_item in msg)
            {
                foreach(var dic_item in dictionary)
                {
                    if (msg_item.ToLower().Equals(dic_item.Value.ToLower()))
                    {
                        badword = msg_item;
                        break;
                    }
                }
            }
            return badword;
        }
    }
}
