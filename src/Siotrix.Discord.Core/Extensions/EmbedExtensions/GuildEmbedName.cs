using System;
using System.Linq;
using Discord.Commands;

namespace Siotrix.Discord
{
    public static class GuildEmbedName
    {
        public static string GetGuildName(this SocketCommandContext context)
        {
            var guildId = context.Guild.Id;
            string name = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gnames.Where(p => p.GuildId == guildId.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                        name = context.Guild.Name;
                    else
                        name = val.First().GuildName;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return name;
        }
    }
}