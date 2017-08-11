using Discord.Commands;

namespace Siotrix.Discord
{
    public static class ChannelNameExtensions
    {
        public static long GetChannelIdFromName(this string name, SocketCommandContext context)
        {
            long id = 0;
            if (name == null)
                return -1;
            foreach (var compareChannel in context.Guild.Channels)
            {
                var compareChannelName = "<#" + compareChannel.Id + ">";
                if (compareChannelName.Equals(name))
                    id = compareChannel.Id.ToLong();
            }
            return id;
        }
    }
}