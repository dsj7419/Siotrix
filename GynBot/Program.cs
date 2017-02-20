namespace GynBot
{
    public class Program
    {
        #region Public Methods

        public static void Main(string[] args) => new GynBot().StartAsync<GynBotConfig>().GetAwaiter().GetResult();

        #endregion Public Methods
    }
}