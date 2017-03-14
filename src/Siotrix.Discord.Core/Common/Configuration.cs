using Discord;
using System;
using System.IO;

namespace Siotrix.Discord
{
    public class Configuration : ConfigurationBase
    {
        public AuthTokens Tokens { get; set; } = new AuthTokens();
        public SiotrixModules Modules { get; set; } = new SiotrixModules();

        public Configuration() : base("config/configuration.json") { }

        public static Configuration Load()
            => Load<Configuration>();

        public static void EnsureExists()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(file))
            {
                string path = Path.GetDirectoryName(file);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var config = new Configuration();

                PrettyConsole.Log(LogSeverity.Warning, "Siotrix", "Please enter discord token: ");
                string token = Console.ReadLine();

                config.Tokens.Discord = token;
                config.SaveJson();
            }
            PrettyConsole.Log(LogSeverity.Info, "Siotrix", "Configuration Loaded");
        }
    }

    public class AuthTokens
    {
        public string Discord { get; set; } = "";
        public string Github { get; set; } = "";
    }
}
