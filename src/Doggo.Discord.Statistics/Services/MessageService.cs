using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Doggo.Discord.Statistics
{
    public class MessageService : IService
    {
        private DiscordSocketClient _client;

        public MessageService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task StartAsync()
        {
            _client.MessageReceived += OnMessageReceivedAsync;
            _client.MessageUpdated += OnMesssageUpdatedAsync;
            _client.MessageDeleted += OnMessageDeletedAsync;
            _client.ReactionAdded += OnReactionAddedAsync;
            _client.ReactionRemoved += OnReactionRemovedAsync;
            _client.ReactionsCleared += OnReactionsClearedAsync;

            await PrettyConsole.LogAsync("Info", "Message", "Service started successfully");
        }

        public async Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            _client.MessageUpdated -= OnMesssageUpdatedAsync;
            _client.MessageDeleted -= OnMessageDeletedAsync;
            _client.ReactionAdded -= OnReactionAddedAsync;
            _client.ReactionRemoved -= OnReactionRemovedAsync;
            _client.ReactionsCleared -= OnReactionsClearedAsync;

            await PrettyConsole.LogAsync("Info", "Message", "Service started successfully");
        }
        
        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            var msg = EntityHelper.CreateMessage(message);

            using (var db = new LogDatabase())
            {
                await db.Messages.AddAsync(msg);
                await db.SaveChangesAsync();
            }
        }

        private async Task OnMesssageUpdatedAsync(Cacheable<IMessage, ulong> cachemsg, SocketMessage message, ISocketMessageChannel channel)
        {
            using (var db = new LogDatabase())
            {
                var msg = await db.GetMessageAsync(message.Id);

                msg.Content = message.Content;

                db.Messages.Update(msg);
                await db.SaveChangesAsync();
            }
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachemsg, ISocketMessageChannel channel)
        {
            using (var db = new LogDatabase())
            {
                var msg = await db.GetMessageAsync(cachemsg.Id);
                
                msg.DeletedAt = DateTime.UtcNow;
                
                db.Messages.Update(msg);
                await db.SaveChangesAsync();
            }
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var react = EntityHelper.CreateReaction(reaction);

            using (var db = new LogDatabase())
            {
                await db.Reactions.AddAsync(react);
                await db.SaveChangesAsync();
            }
        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            using (var db = new LogDatabase())
            {
                var react = await db.GetReactionAsync(cachemsg.Id, reaction.UserId, reaction.Emoji.Name);

                react.DeletedAt = DateTime.UtcNow;

                db.Reactions.Update(react);
                await db.SaveChangesAsync();
            }
        }

        private async Task OnReactionsClearedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel)
        {
            using (var db = new LogDatabase())
            {
                var reacts = await db.GetReactionsAsync(cachemsg.Id);

                foreach (var react in reacts)
                    react.DeletedAt = DateTime.UtcNow;

                db.Reactions.UpdateRange(reacts);
                await db.SaveChangesAsync();
            }
        }
    }
}
