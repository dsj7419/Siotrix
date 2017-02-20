namespace GynBot.Interfaces
{
    #region Using

    using System.Collections.Generic;

    #endregion

    public interface IServerConfig
    {
        #region Public Fields + Properties

        string CommandPrefix { get; set; }
        Dictionary<string, string> Tags { get; set; }

        #endregion Public Fields + Properties
    }
}