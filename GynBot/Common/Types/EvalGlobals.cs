using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GynBot.Common.Types
{
    public class EvalGlobals
    {
        public SocketUserMessage Message { get; set; }
        public SocketTextChannel Channel { get { return this.Message.Channel as SocketTextChannel; } }
        public SocketGuild Guild { get { return this.Channel.Guild; } }

        public DiscordSocketClient Client { get { return this.Message.Discord; } }
    }
}