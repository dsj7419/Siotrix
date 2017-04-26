using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Siotrix.Discord
{
    public static class GuildEmbedName
    {
        public static string GetGuildName(this SocketCommandContext context)
        {
            var guild_id = context.Guild.Id;
            string name = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gnames.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        name = context.Guild.Name;
                    }
                    else
                    {
                        name = val.First().GuildName;
                    }
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
