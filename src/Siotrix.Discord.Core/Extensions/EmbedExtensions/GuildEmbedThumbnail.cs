using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Siotrix.Discord
{
    public static class GuildEmbedThumbnail
    {
        public static string GetGuildThumbNail(this SocketCommandContext context)
        {
            var guild_id = context.Guild.Id;
            string thumbnail_url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gthumbnails.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        thumbnail_url = "https://s3.us-east-2.amazonaws.com/siotriximages/ShareX/2017/06/ApplicationFrameHost_2017-06-22_11-10-46.png";
                    }
                    else
                    {
                        thumbnail_url = val.First().ThumbNail;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return thumbnail_url;
        }
    }
}
