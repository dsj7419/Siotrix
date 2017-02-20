namespace GynBot.Interfaces
{
    #region Using

    using Discord.Commands;
    using Discord.WebSocket;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    #endregion Using

    public interface ICommandHandler
    {

        #region Public Fields + Properties

        DiscordSocketClient Client { get; set; }
        CommandService Service { get; set; }
        IDependencyMap Map { get; set; }

        #endregion Public Fields + Properties

        #region Public Methods

        Task HandleCommandAsync(SocketMessage msg);

        Task InstallAsync(IDependencyMap map = null);

        #endregion Public Methods
    }
}