using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Siotrix.Discord
{
    class Program
    {
        public static void Main(string[] args)
            => new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private ServiceManager _manager;

        public async Task Start()
        {
            PrettyConsole.NewLine("===   Siotrix   ===");
            PrettyConsole.NewLine();

            Configuration.EnsureExists();
            await LogDatabase.EnsureExistsAsync();

            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                //AudioMode = AudioMode.Outgoing,
                MessageCacheSize = 1000
            });

            _client.Log += OnLogAsync;

            await _client.LoginAsync(TokenType.Bot, Configuration.Load().Tokens.Discord);
            await _client.StartAsync();

            _manager = new ServiceManager(_client);
            await _manager.StartAsync();

            await Task.Delay(-1);
        }

        private async Task OnLogAsync(LogMessage msg)
        {
            await PrettyConsole.LogAsync(msg.Severity, msg.Source, msg.Message);

            if (msg.Exception?.StackTrace == null && msg.Severity != LogSeverity.Error)
                return;

            var error = new Error(msg.Exception);
            using (var db = new LogDatabase())
            {
                await db.Errors.AddAsync(error);
                await db.SaveChangesAsync();
            }
        }
    }
}