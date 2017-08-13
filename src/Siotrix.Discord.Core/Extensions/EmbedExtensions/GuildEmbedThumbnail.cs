using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class GuildEmbedThumbnail
    {
        public static async Task<DiscordGuildThumbNail> GetGuildThumbNailAsync(this SocketCommandContext context)
        {
            var val = new DiscordGuildThumbNail();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gthumbnails.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    if (val == null)
                    {
                        await CreateDiscordGuildThumbNailAsync(context, SiotrixConstants.BotAvatar);
                        val = await db.Gthumbnails.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateDiscordGuildThumbNailAsync(SocketCommandContext context, string thumbNail)
        {
            var val = new DiscordGuildThumbNail(context.Guild.Id.ToLong(), thumbNail);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gthumbnails.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetGuildThumbNail(DiscordGuildThumbNail discordGuildThumbNail, string thumbNail)
        {
            discordGuildThumbNail.SetGuildThumbNail(thumbNail);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gthumbnails.Update(discordGuildThumbNail);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}