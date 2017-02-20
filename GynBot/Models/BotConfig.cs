namespace GynBot.Models
{
    #region Using

    using System;
    using System.Reflection;
    using Interfaces;
    using Newtonsoft.Json;

    #endregion

    struct BotConfig : IBotConfig
    {

        [JsonProperty("botToken")]
        public string BotToken { get; set; }

        [JsonProperty("logChannel")]
        public ulong LogChannel { get; set; }

        public BotConfig(string botToken, ulong logChannel)
        {
            BotToken = botToken;
            LogChannel = logChannel;
        }
    }
}