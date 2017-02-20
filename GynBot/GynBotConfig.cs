namespace GynBot
{
    using Interfaces;
    using Newtonsoft.Json;

    public struct GynBotConfig : IBotConfig
    {
        #region Implementation of IBotConfig

        [JsonProperty("botToken")]
        public string BotToken { get; set; }
        [JsonProperty("logChannel")]
        public ulong LogChannel { get; set; }
        [JsonProperty("dbToken")]
        public string DatabaseToken { get; set; }

        public GynBotConfig(string botToken, ulong logChannel, string dbToken)
        {
            BotToken = botToken;
            LogChannel = logChannel;
            DatabaseToken = dbToken;
        }

        #endregion
    }
}