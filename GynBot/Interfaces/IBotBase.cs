namespace GynBot.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    public interface IBotBase
    {
        DiscordSocketClient Client { get; set; }
        ICommandHandler Commands { get; set; }

        Task StartAsync<T>() where T : IBotConfig, new();
        Task HandleConfigsAsync<T>() where T : IBotConfig, new();
        Task InstallCommandsAsync();
        Task LoginAndConnectAsync(TokenType tokenType);
    }
}