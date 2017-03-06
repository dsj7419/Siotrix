using Newtonsoft.Json;
using System;
using System.IO;
using GynBot.Common.Utilities;
using Discord;

namespace GynBot.Common.Types
{
    /// <summary> 
    /// A file that contains information you either don't want public
    /// or will want to change without having to compile another bot.
    /// </summary>
    public class Configuration
    {
        /// <summary> The location of your bot's dll, ignored by the json parser. </summary>
        [JsonIgnore]
        public static readonly string appdir = AppContext.BaseDirectory;

        /// <summary> Your bot's command prefix. Please don't pick `!`. </summary>
        public string Prefix { get; set; }
        /// <summary> Ids of users who will have owner access to the bot. </summary>
        public ulong[] Owners { get; set; }
        /// <summary> Your bot's login token. </summary>
        public string Token { get; set; }

        /// <summary> npgsql login </summary>
        public string GynbotDBConnection { get; set; }

        public Configuration()
        {
            Prefix = "!";
            Owners = new ulong[] { 0 };
            Token = "";
            GynbotDBConnection = "";
        }

        /// <summary> Load the configuration from the specified file location. </summary>
        public static Configuration Load(string dir = "data/configuration.json")
        {
            string file = Path.Combine(appdir, dir);
            return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(file));
        }

        public static void EnsureConfigExists()
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data"));

            string loc = Path.Combine(AppContext.BaseDirectory, "data/configuration.json");

            if (!File.Exists(loc))                              // Check if the configuration file exists.
            {
                var config = new Configuration();               // Create a new configuration object.

                BetterConsole.Log(LogSeverity.Warning, "GynBot", "The configuration file has been created at: 'data\\configuration.json',\n\n\n" +
                              "                      ====PLEASE ENTER THE FOLLOWING INFORMATION====\n\n");
                BetterConsole.Log(LogSeverity.Warning, "GynBot", "Please Enter your token: ");

                config.Token = Console.ReadLine();              // Read the bot token from console.

                BetterConsole.Log(LogSeverity.Warning, "GynBot", "Discord Bot Owner ID needed...What is your DISCORD ID: "); // read the bot owner ID
                config.Owners[0] = Convert.ToUInt64(Console.ReadLine());

                BetterConsole.Log(LogSeverity.Warning, "GynBot", "        ---===Database information needed===---");
                BetterConsole.Log(LogSeverity.Warning, "GynBot", "We will be entering info to get the following information complete:");
                BetterConsole.Log(LogSeverity.Warning, "GynBot", "User ID={userid};Password={userpass};Host={host};Port={port};Database={database};Pooling=true\n\n");
                BetterConsole.Log(LogSeverity.Warning, "GynBot", "Host: ");
                var host = Console.ReadLine();
                BetterConsole.Log(LogSeverity.Warning, "GynBot", "Port: ");
                var port = Console.ReadLine();
                BetterConsole.Log(LogSeverity.Warning, "GynBot", "User Login: ");
                var userid = Console.ReadLine();
                BetterConsole.Log(LogSeverity.Warning, "GynBot", "User Password: ");
                var userpass = Console.ReadLine();
                BetterConsole.Log(LogSeverity.Warning, "GynBot", "Database Name: ");
                var database = Console.ReadLine();


                config.GynbotDBConnection = $"User ID={userid};Password={userpass};Host={host};Port={port};Database={database};Pooling=true;";
                config.Save();                                  // Save the new configuration object to file.
            }
            BetterConsole.Log(LogSeverity.Info, "GynBot", "Configuration Loaded...");
        }


        /// <summary> Save the configuration to the specified file location. </summary>
        public void Save(string dir = "data/configuration.json")
        {
            string file = Path.Combine(appdir, dir);
            File.WriteAllText(file, ToJson());
        }

        /// <summary> Convert the configuration to a json string. </summary>
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}