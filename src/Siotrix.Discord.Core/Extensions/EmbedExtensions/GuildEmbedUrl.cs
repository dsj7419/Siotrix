using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class GuildEmbedUrl
    {
        public static async Task<DiscordGuildSiteUrl> GetGuildUrlAsync(this SocketCommandContext context)
        {
            var val = new DiscordGuildSiteUrl();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gwebsiteurls.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateDiscordGuildUrlAsync(SocketCommandContext context, string siteUrl)
        {
            var val = new DiscordGuildSiteUrl(context.Guild.Id.ToLong(), siteUrl);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gwebsiteurls.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetGuildUrl(DiscordGuildSiteUrl discordGuildUrl, string siteUrl)
        {
            discordGuildUrl.SetSiteUrl(siteUrl);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gwebsiteurls.Update(discordGuildUrl);
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