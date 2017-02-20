namespace GynBot.Interfaces
{
    public interface IBotConfig
    {
        #region Public Fields + Properties

        string BotToken { get; set; }
        ulong LogChannel { get; set; }

        #endregion Public Fields + Properties
    }
}