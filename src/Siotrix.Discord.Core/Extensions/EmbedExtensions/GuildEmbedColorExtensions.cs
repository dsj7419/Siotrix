using System;
using System.Linq;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class GuildEmbedColorExtensions
    {
        public static async Task<Color> GetGuildColorAsync(this SocketCommandContext context)
        {
            var siotrixColor = "0x010101";
            var guildId = context.Guild.Id;
            string colorHex = null;

            var val = new DiscordColor();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gcolors.FirstOrDefaultAsync(p => p.GuildId == guildId.ToLong());
                    if (val == null)
                    {
                        await db.Gcolors.AddAsync(new DiscordColor
                        {
                            ColorHex = siotrixColor,
                            GuildId = guildId.ToLong()
                        });
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        colorHex = siotrixColor;
                    }
                    else
                    {
                        colorHex = val.ColorHex;
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