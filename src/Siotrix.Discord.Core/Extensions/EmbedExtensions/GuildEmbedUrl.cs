using System;
using System.Linq;
using Discord.Commands;

namespace Siotrix.Discord
{
    public static class GuildEmbedUrl
    {
        public static string GetGuildUrl(this SocketCommandContext context)
        {
            var guildId = context.Guild.Id;
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gwebsiteurls.Where(p => p.GuildId == guildId.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                        url = db.Authors.First().AuthorUrl;
                    else
                        url = val.First().SiteUrl;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return url;
        }
    }
}