using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class SiotrixEmbedWebsiteExtensions
    {
        public static async Task<DiscordSiotrixSiteUrl> GetSiotrixSiteUrlAsync()
        {
            var val = new DiscordSiotrixSiteUrl();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Bwebsiteurls.FirstOrDefaultAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateSiotrixSiteUrl(string siteUrl)
        {
            var val = new DiscordSiotrixSiteUrl(siteUrl);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Bwebsiteurls.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetSiotrixSiteUrl(DiscordSiotrixSiteUrl discordSiotrixSiteUrl, string siteUrl)
        {
            discordSiotrixSiteUrl.SetSiotrixSiteUrl(siteUrl);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Bwebsiteurls.Update(discordSiotrixSiteUrl);
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
