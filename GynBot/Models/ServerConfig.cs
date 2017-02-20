namespace GynBot.Models
{
    #region Using

    using System.Collections.Generic;
    using Interfaces;
    using Newtonsoft.Json;

    #endregion

    public struct ServerConfig : IServerConfig
    {
        #region Public Fields + Properties

        public const string DefaultPrefix = ";";

        #endregion Public Fields + Properties

        #region Implementation of IServerConfig

        [JsonProperty("commandPrefix")]
        public string CommandPrefix { get; set; }

        [JsonProperty("tags")]
        public Dictionary<string, string> Tags { get; set; }

        #endregion Implementation of IServerConfig

        #region Public Constructors

        public ServerConfig(string commandPrefix, Dictionary<string, string> tags)
        {
            CommandPrefix = commandPrefix;
            Tags = tags;
        }

        #endregion Public Constructors
    }
}