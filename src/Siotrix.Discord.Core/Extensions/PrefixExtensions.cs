using System;
using System.Linq;
using Discord.Commands;

namespace Siotrix.Discord
{
    public static class PrefixExtensions
    {
        public static string GetGuildPrefix(this SocketCommandContext context)
        {
            var guildId = context.Guild.Id;
            string prefix = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gprefixs.Where(p => p.GuildId == guildId.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                        prefix = SiotrixConstants.BotPrefix;
                    else
                        prefix = val.First().Prefix;
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