using Discord;
using Discord.WebSocket;
using GynBot.Common.Types;
using GynBot.Common.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GynBot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandHandler _commands;

        public async Task Start()
        {
            BetterConsole.NewLine("====================================  |   GynBot   |  ====================================");
            BetterConsole.NewLine();
            Configuration.EnsureConfigExists();                            // Ensure the configuration file has been created.
                                                            
            _client = new DiscordSocketClient(new DiscordSocketConfig()     // Create a new instance of DiscordSocketClient.
            {
                LogLevel = LogSeverity.Verbose                  // Specify console verbose information level.
            });

            _client.Log += (l)                               // Register the console log event.
                => Task.Run(()
                => BetterConsole.Log(l.Severity, l.Source, l.Exception?.ToString() ?? l.Message));

            await _client.LoginAsync(TokenType.Bot, Configuration.Load().Token);
            await _client.StartAsync();

            _commands = new CommandHandler();               // Initialize the command handler service
            await _commands.Install(_client);

            await Task.Delay(-1);                            // Prevent the console window from closing.
        }       
    }
}