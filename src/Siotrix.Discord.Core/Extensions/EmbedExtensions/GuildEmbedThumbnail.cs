using System;
using System.Linq;
using Discord.Commands;

namespace Siotrix.Discord
{
    public static class GuildEmbedThumbnail
    {
        public static string GetGuildThumbNail(this SocketCommandContext context)
        {
            var guildId = context.Guild.Id;
            string thumbnailUrl = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gthumbnails.Where(p => p.GuildId == guildId.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                        thumbnailUrl = SiotrixConstants.BotLogo;
                    else
                        thumbnailUrl = val.First().ThumbNail;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return thumbnailUrl;
        }
    }
}