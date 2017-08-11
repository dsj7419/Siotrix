using System;
using System.Linq;
using Discord;
using Discord.Commands;

namespace Siotrix.Discord
{
    public static class GuildEmbedColorExtensions
    {
        public static Color GetGuildColor(this SocketCommandContext context)
        {
            var siotrixColor = "0x010101";
            var guildId = context.Guild.Id;
            string colorHex = null;

            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gcolors.Where(p => p.GuildId == guildId.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        db.Gcolors.Add(new DiscordColor
                        {
                            ColorHex = siotrixColor,
                            GuildId = guildId.ToLong()
                        });
                        db.SaveChanges();
                        colorHex = siotrixColor;
                    }
                    else
                    {
                        colorHex = val.First().ColorHex;
                        if (colorHex == "0x000000")
                            colorHex = "0x010101";
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