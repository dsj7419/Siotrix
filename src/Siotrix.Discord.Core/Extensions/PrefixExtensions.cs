using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Siotrix.Discord
{
    public static class PrefixExtensions
    {
        public static string GetGuildPrefix(this SocketCommandContext context)
        {
            var guild_id = context.Guild.Id;
            string prefix = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gprefixs.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        prefix = SiotrixConstants.BOT_PREFIX;
                    }
                    else
                    {
                        prefix = val.First().Prefix;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return prefix;
        }
    }
}
