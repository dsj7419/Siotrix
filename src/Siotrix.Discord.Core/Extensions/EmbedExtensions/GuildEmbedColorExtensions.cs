using Discord;
using Discord.Commands;
using System;
using System.Linq;

namespace Siotrix.Discord
{

    public static partial class GuildEmbedColorExtensions
    {
        public static Color GetGuildColor(this SocketCommandContext context)
        {
            string SIOTRIX_COLOR = "0x010101";
            var guild_id = context.Guild.Id;
            string colorHex = null;

            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gcolors.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        db.Gcolors.Add(new DiscordColor()
                        {
                            ColorHex = SIOTRIX_COLOR,
                            GuildId = guild_id.ToLong()
                        });
                        db.SaveChanges();
                        colorHex = SIOTRIX_COLOR;
                    }
                    else
                    {
                        colorHex = val.First().ColorHex;
                        if(colorHex == "0x000000")
                        {
                            colorHex = "0x010101";
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
              return new Color(Convert.ToUInt32(colorHex, 16));
        }
    }
}
