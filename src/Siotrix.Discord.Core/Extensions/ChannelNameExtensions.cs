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
            foreach (var compare_channel in context.Guild.Channels)
            {
                var compare_channel_name = "<#" + compare_channel.Id + ">";
                if (compare_channel_name.Equals(name))
                    id = compare_channel.Id.ToLong();
            }
            return id;
        }
    }
}