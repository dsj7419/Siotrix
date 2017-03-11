using Discord;
using System;
using System.IO;

namespace Doggo.Discord
{
    public class Configuration : ConfigurationBase
    {
        public AuthTokens Tokens { get; set; } = new AuthTokens();
        public DoggoModules Modules { get; set; } = new DoggoModules();

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

                PrettyConsole.Log(LogSeverity.Warning, "Doggo", "Please enter discord token: ");
                string token = Console.ReadLine();

                config.Tokens.Discord = token;
                config.SaveJson();
            }
            PrettyConsole.Log(LogSeverity.Info, "Doggo", "Configuration Loaded");
        }
    }

    public class AuthTokens
    {
        public string Discord { get; set; } = "";
        public string Github { get; set; } = "";
    }

    public class DoggoModules
    {
        public bool Admin { get; set; } = true;
        public bool Audio { get; set; } = true;
        public bool Events { get; set; } = true;
        public bool Moderation { get; set; } = true;
        public bool Roslyn { get; set; } = true;
        public bool Github { get; set; } = true;
        public bool Statistics { get; set; } = true;
    }
}
