using System;
using System.IO;
using Discord;

namespace Siotrix.Discord
{
    public class Configuration : ConfigurationBase
    {
        public Configuration() : base("config/configuration.json")
        {
        }

        public AuthTokens Tokens { get; set; } = new AuthTokens();
        public PublicConfigs PConfigs { get; set; } = new PublicConfigs();
        public SiotrixModules Modules { get; set; } = new SiotrixModules();

        public static Configuration Load()
        {
            return Load<Configuration>();
        }

        public static void EnsureExists()
        {
            var file = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(file))
            {
                var path = Path.GetDirectoryName(file);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var config = new Configuration();

                PrettyConsole.Log(LogSeverity.Warning, "Siotrix", "Please enter discord token: ");
                var token = Console.ReadLine();

                config.Tokens.Discord = token;

                Console.WriteLine(
                    "\nDiscord Bot Owner ID needed. What is your discord ID master?"); // read the bot owner ID
                Console.Write("Discord ID: ");
                config.PConfigs.Owners[0] = Convert.ToUInt64(Console.ReadLine());

                config.SaveJson();
            }
            PrettyConsole.Log(LogSeverity.Info, "Siotrix", "Configuration Loaded");
        }
    }

    public class PublicConfigs
    {
        public ulong[] Owners { get; set; } = {0};
    }

    public class AuthTokens
    {
        public string Discord { get; set; } = "";
        public string StackoverflowToken { get; set; } = "";
    }
}