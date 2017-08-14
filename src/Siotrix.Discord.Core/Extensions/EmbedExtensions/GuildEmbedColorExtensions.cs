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
        public static async Task<DiscordColor> GetGuildColorAsync(this SocketCommandContext context)
        {
            var val = new DiscordColor();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gcolors.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    if (val == null)
                    {
                        await CreateDiscordGuildColorAsync(context, SiotrixConstants.BotColor);
                        val = await db.Gcolors.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateDiscordGuildColorAsync(SocketCommandContext context, string colorHex)
        {
            var val = new DiscordColor(context.Guild.Id.ToLong(), colorHex);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gcolors.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetGuildColor(DiscordColor discordGuildColor, string colorHex)
        {
            discordGuildColor.SetGuildColorHex(colorHex);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gcolors.Update(discordGuildColor);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static Color ConvertStringtoColorObject(string colorHex)
        {
            var realColor = Convert.ToUInt32(colorHex, 16);
            var convertedToColor = new Color(realColor);
            return convertedToColor;
        }
    }
}