using System;
using System.Linq;
using Discord.Commands;

namespace Siotrix.Discord
{
    public static class GuildEmbedIconUrl
    {
        public static string GetGuildIconUrl(this SocketCommandContext context)
        {
            var guildId = context.Guild.Id;
            string iconurl = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gavatars.Where(p => p.GuildId == guildId.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                        iconurl = db.Authors.First().AuthorIcon;
                    else
                        iconurl = val.First().Avatar;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return iconurl;
        }
    }
}