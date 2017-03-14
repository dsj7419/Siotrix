﻿using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Statistics
{
    public class MessageService : IService
    {
        private DiscordSocketClient _client;
        private LogDatabase _db;

        public MessageService(DiscordSocketClient client)
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
            _db = null;

            await PrettyConsole.LogAsync("Info", "Message", "Service started successfully");
        }
        
        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            var msg = EntityHelper.CreateMessage(message);

            await _db.Messages.AddAsync(msg);
            await _db.SaveChangesAsync();
        }

        private async Task OnMesssageUpdatedAsync(Cacheable<IMessage, ulong> cachemsg, SocketMessage message, ISocketMessageChannel channel)
        {
            var msg = await _db.GetMessageAsync(message.Id);

            msg.Content = message.Content;

            _db.Messages.Update(msg);
            await _db.SaveChangesAsync();
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachemsg, ISocketMessageChannel channel)
        {
            var msg = await _db.GetMessageAsync(cachemsg.Id);

            msg.DeletedAt = DateTime.UtcNow;

            _db.Messages.Update(msg);
            await _db.SaveChangesAsync();
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var react = EntityHelper.CreateReaction(reaction);

            await _db.Reactions.AddAsync(react);
            await _db.SaveChangesAsync();
        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var react = await _db.GetReactionAsync(cachemsg.Id, reaction.UserId, reaction.Emoji.Name);

            react.DeletedAt = DateTime.UtcNow;

            _db.Reactions.Update(react);
            await _db.SaveChangesAsync();
        }

        private async Task OnReactionsClearedAsync(Cacheable<IUserMessage, ulong> cachemsg, ISocketMessageChannel channel)
        {
            var reacts = await _db.GetReactionsAsync(cachemsg.Id);

            foreach (var react in reacts)
                react.DeletedAt = DateTime.UtcNow;

            _db.Reactions.UpdateRange(reacts);
            await _db.SaveChangesAsync();
        }
    }
}
