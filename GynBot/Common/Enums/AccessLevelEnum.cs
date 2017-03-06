namespace GynBot.Common.Enums
{
    /// <summary>
    /// The enum used to specify permission levels. A lower
    /// number means less permissions than a higher number.
    /// </summary>
    public enum AccessLevelEnum
    {
        Blocked,
        User,
        ServerMod,
        ServerAdmin,
        ServerOwner,
        BotOwner
    }
}
