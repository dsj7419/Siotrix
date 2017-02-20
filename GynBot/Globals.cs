namespace GynBot
{
    using Interfaces;
    using Models;
    using System.Collections.Generic;

    public static class Globals
    {
        public static IBotConfig BotConfig { get; set; }

        public static List<string> EvalImports { get; } = new List<string> {
            "Discord",
            "Discord.API",
            "Discord.Commands",
            "Discord.Rest",
            "Discord.WebSocket",
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Diagnostics",
            "System.IO",
            "System.Linq",
            "System.Math",
            "System.Reflection",
            "System.Runtime",
            "System.Threading.Tasks",
        };

        public static Dictionary<ulong, ServerConfig> ServerConfigs { get; set; } = new Dictionary<ulong, ServerConfig>();
        public const string CONFIG_PATH = "config.json";
        public const string DEFAULT_PREFIX = ";";
        public const ulong OWNER_ID = 173905004661309441;
        public const string SERVER_CONFIG_PATH = "server_configs.json";
    }
}